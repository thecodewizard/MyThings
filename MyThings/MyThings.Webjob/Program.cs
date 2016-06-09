using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MyThings.Common.Models;
using MyThings.Common.Models.NoSQL_Entities;
using MyThings.Common.Repositories;
using Newtonsoft.Json;

namespace DataStorageQueue
{
    public class Program
    {
        //Repository Declarations
        private static readonly ContainerTypeRepository _containerTypeRepository = new ContainerTypeRepository();
        private static readonly SensorRepository _sensorRepository = new SensorRepository();
        private static readonly ContainerRepository _containerRepository = new ContainerRepository();
        private static readonly ErrorRepository _errorRepository = new ErrorRepository();
        private static readonly GenericRepository<Timeholder> _timeholderRepository = new GenericRepository<Timeholder>();       

        public static void Main(string[] args)
        {
            // Write the WebJob start time to the Timeholder database
            Timeholder holder = (_timeholderRepository.All().First()) ?? new Timeholder();
            holder.WebjobInstanceStarted = DateTime.Now;
            _timeholderRepository.Update(holder);
            _timeholderRepository.SaveChanges();

            CloudQueue queue = null;
            try
            {
                // Get the Queue
                queue = ConnectToQueue();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            if (queue != null && queue.Exists())
            {
                int cachedMessageCount = 0;
                do
                {
                    // Peek at the next message
                    CloudQueueMessage peekedMessage = queue.PeekMessage();
                    if (peekedMessage != null)
                    {
                        // Get the next message
                        CloudQueueMessage retrievedMessage = queue.GetMessage();

                        // Process the message
                        RunPerSensor(retrievedMessage.AsString);

                        // Process the message in less than 30 seconds, and then delete the message
                        queue.DeleteMessage(retrievedMessage);
                    }

                    // Get and Display number of messages.
                    queue.FetchAttributes();
                    cachedMessageCount = queue.ApproximateMessageCount ?? 0;
                    Console.WriteLine("Number of messages in queue: " + cachedMessageCount);

                } while (cachedMessageCount > 0);
            }

            // Do the onCompletedWork
            RunOnceInWebjob();

            // Write the WebJob end time to the Timeholder database
            holder = (_timeholderRepository.All().First()) ?? new Timeholder();
            holder.WebjobInstanceEnded = DateTime.Now;
            _timeholderRepository.Update(holder);
            _timeholderRepository.SaveChanges();
        }

        static CloudQueue ConnectToQueue()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference("mythingsdecodedqueue");
            return queue;
        }

        static void RunPerSensor(String json)
        {
            //Parse the incoming data
            QueueMessageHolder holder = JsonConvert.DeserializeObject<QueueMessageHolder>(json);
            ContainerEntity containerEntity = TableStorageRepository.GetContainerFromTableStorage(holder.PartitionKey,
                holder.RowKey);
            if (containerEntity == null) return;

            //Database Population Management
                //Check Existance Containertype
            String containerTypeName = containerEntity.container;
            ContainerType type = new ContainerType() { Name = containerTypeName };
            _containerTypeRepository.SaveOrUpdateContainerType(type);

                //Check Existance Sensor
            Sensor sensor = _sensorRepository.GetSensorByMacAddress(containerEntity.macaddress);
            if (sensor == null)
            {
                sensor = new Sensor();
                sensor.CreationDate = DateTime.Now;
                sensor.MACAddress = containerEntity.macaddress;
                sensor.SensorEntries = 1;
                sensor.Company = containerEntity.company;
                sensor.Location = containerEntity.locationid;
                sensor.Name = containerEntity.macaddress;
                sensor = _sensorRepository.Insert(sensor);
            }
            else
            {
                sensor.SensorEntries++;
                _sensorRepository.Update(sensor);
            }
            _sensorRepository.SaveChanges();

                //Check existance container
            Container container = (from c in sensor.Containers where c.ContainerType.Name == containerTypeName select c).FirstOrDefault();
            if (container == null)
            {
                container = new Container();
                container.ContainerType = type;
                container.ContainerTypeId = type.Id;
                container.CreationTime = DateTime.Now;
                container.MACAddress = containerEntity.macaddress;
                container.SensorId = sensor.Id;
                _containerRepository.Insert(container);
                _containerRepository.SaveChanges();
            }

            //Error-Warning Module

            //TODO: Implement Threshold
                //Check voor de huidige container of de waarde overschreden is.
                    //-> Ja: Resulteerd in MinThresholdWarning of MaxThresholdWarning
                    //-> Indien containertype battery:

                //Check for battery Level warnings
            if (container.ContainerType.Name == "Battery level")
            {
                //Get all the battery values from last year
                container = TableStorageRepository.GetHistory(container, TimeSpan.FromDays(365));

                //Calculate when the battery would be empty
                    //Get the average downgrade per update
                int count = container.History.Count;
                double totalDelta = 0;
                double previousValue = container.History.Last().Value;

                foreach (ContainerValue value in container.History)
                {
                    double delta = previousValue - value.Value;
                    totalDelta += delta;
                    previousValue = value.Value;
                }

                    //Temp. Results
                double average = totalDelta/count;
                TimeSpan lifeTime = DateTime.Now - container.CreationTime;
                long lifeTimeTicks = lifeTime.Ticks;
                long ticksPerUpdate = lifeTimeTicks/count;

                    //Get prediction
                int updatesLeft = (int)Math.Floor((containerEntity.payload / average));
                long ticksToLive = ticksPerUpdate*updatesLeft;
                TimeSpan timeToLive = TimeSpan.FromTicks(ticksToLive);

                //Give the error
                if (containerEntity.payload < 5)
                {
                    Error error = Error.BatteryCriticalError(sensor, container, timeToLive);
                    _errorRepository.Insert(error);
                } else if (containerEntity.payload < 15)
                {
                    Error error = Error.BatteryWarning(sensor, container, timeToLive);
                    _errorRepository.Insert(error);
                }
            }
            _errorRepository.SaveChanges();
        }

        static void RunOnceInWebjob()
        {
            //Error-Warning Module
                //Predict Network Connectivity Errors per Sensor (-> Triggered when update is slower than 5 times the average updateSpan.)
            List<Sensor> sensors = _sensorRepository.GetSensors();
            foreach (Sensor sensor in sensors)
            {
                //Calculate a rudimentary the average ticks between all entries for a sensor
                long creationTimeTicks = sensor.CreationDate.Ticks;
                long averageTicksBetweenEntry = creationTimeTicks/sensor.SensorEntries;

                DateTime mostRecentValue = DateTime.MinValue;
                foreach (Container container in sensor.Containers)
                {
                    //Set the most recent value
                    Container UpdatedContainer = TableStorageRepository.UpdateValue(container);
                    if (UpdatedContainer.CurrentValue.Timestamp > mostRecentValue)
                        mostRecentValue = UpdatedContainer.CurrentValue.Timestamp;

                    //While looping the containers in the sensor, check for sensor/container inactivity as well
                    bool isActive = false;
                    foreach (Container sensorContainer in sensor.Containers)
                    {
                        if (isActive) break;

                        Container updatedContainer = TableStorageRepository.UpdateValue(sensorContainer);
                        TimeSpan updatedSince = DateTime.Now - updatedContainer.CurrentValue.Timestamp;

                        if (updatedSince.Hours > 48)
                        {
                            Error error = Error.InactiveContainerWarning(sensor, sensorContainer);
                            _errorRepository.Insert(error);
                        } else isActive = true;
                    }

                    if (!isActive)
                    {
                        Error error = Error.InactiveSensorWarning(sensor);
                        _errorRepository.Insert(error);
                    }
                }

                //Determine when the sensor last had an active container
                TimeSpan lastUpdatedSpan = DateTime.Now - mostRecentValue;
                long lastUpdatedTicks = lastUpdatedSpan.Ticks;

                //If the sensor didn't get a message for 5 times the average waittime, throw a networkconnectivityerror
                if ((averageTicksBetweenEntry*5) < lastUpdatedTicks)
                {
                    Error error = Error.NetworkConnectivityError(sensor);
                    _errorRepository.Insert(error);
                }
            }
            _errorRepository.SaveChanges();

            //Virtuele Sensors Berekenen
                //TODO: Calculate virtual sensors.
        }
    }
}
