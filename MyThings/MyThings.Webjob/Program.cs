using System;
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

        //Static Caches
        

        public static void Main(string[] args)
        {
            // Get the Queue
            CloudQueue queue = ConnectToQueue();

            // Prefetch the static caches
            FillCaches();

            if (queue.Exists())
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

            // Standard Console Logic
            Console.WriteLine("Press any key to exit");
            Console.Read();
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

        static void FillCaches()
        {
            
        }

        static void RunPerSensor(String json)
        {
            //Verwerk de inkomende data
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
                //Get the most recent value
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

                //Check for inactivity
            bool isActive = false;
            foreach (Container sensorContainer in sensor.Containers)
            {
                Container updatedContainer = TableStorageRepository.UpdateValue(sensorContainer);
                TimeSpan updatedSince = DateTime.Now - updatedContainer.CurrentValue.Timestamp;

                if (updatedSince.Hours > 48)
                {
                    Error error = Error.InactiveContainerWarning(sensor, sensorContainer);
                    _errorRepository.Insert(error);
                }
                else isActive = true;
            }

            if (!isActive)
            {
                Error error = Error.InactiveSensorWarning(sensor);
                _errorRepository.Insert(error);
            }
            _errorRepository.SaveChanges();
        }

        static void RunOnceInWebjob()
        {
            //Error-Warning Module
                //Check voor voorspelde connectivity errors
                //Bereken op basis van de sensorentries en de creationdate een gemiddelde responstijd van de sensor.
                //Controleer ofdat de laatste waarde die gekend is van de sensor deze responstijd
                    //-> Indien 2x langer: toon networkconnectivity error.

            //Virtuele Sensors Berekenen
                //

            //Timelogging
                //Schrijf naar SQL wanneer deze webjob voor het laatst ge-execute heeft
        }
    }
}
