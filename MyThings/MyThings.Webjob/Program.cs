using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MyThings.Common.Models;
using MyThings.Common.Models.NoSQL_Entities;
using MyThings.Common.Repositories;
using MyThings.Common.Repositories.RemoteRepositories;
using Newtonsoft.Json;

namespace DataStorageQueue
{
    public class Program
    {
        private const String queueName = "mythingsdecodedqueue";
        //private const String queueName = "mythingsdecodedqueuetest";

        //Repository Declarations
        private static readonly ContainerTypeRepository _containerTypeRepository = new ContainerTypeRepository();
        private static readonly SensorRepository _sensorRepository = new SensorRepository();
        private static readonly ContainerRepository _containerRepository = new ContainerRepository();
        private static readonly GroupRepository _groupRepository = new GroupRepository();
        private static readonly ErrorRepository _errorRepository = new ErrorRepository();
        private static readonly GenericRepository<Timeholder> _timeholderRepository = new GenericRepository<Timeholder>();       

        //Caches
        private static readonly List<ContainerType> ContainerTypeCache = new List<ContainerType>();
        private static readonly List<Sensor> SensorCache = new List<Sensor>();

        public static void Main(string[] args)
        {
            // Write the WebJob start time to the Timeholder database
            Timeholder holder = (_timeholderRepository.All().FirstOrDefault()) ?? new Timeholder();
            holder.WebjobInstanceStarted = DateTime.Now;
            _timeholderRepository.Update(holder);
            _timeholderRepository.SaveChanges();

            // Do the onCompletedWork
            Console.Write("Started Grouplogic");
            RunOnceInWebjobStart();
            Console.WriteLine(" -- Finished");

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
            Console.Write("Started General Checkup");
            RunOnceInWebjobEnd();
            Console.WriteLine(" -- Finished");

            // Write the WebJob end time to the Timeholder database
            holder = (_timeholderRepository.All().FirstOrDefault()) ?? new Timeholder();
            holder.WebjobInstanceEnded = DateTime.Now;
            _timeholderRepository.Update(holder);
            _timeholderRepository.SaveChanges();
        }

        private static CloudQueue ConnectToQueue()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference(queueName);
            return queue;
        }

        private static void RunOnceInWebjobStart()
        {
            //Get all the groups
            List<Group> groups = _groupRepository.GetGroups();

            //For each group, update the values
            foreach (Group group in groups)
            {
                Sensor virtSensor = _sensorRepository.GetSensorById(group.VirtualSensorIdentifier);
                if (virtSensor?.Containers == null)
                {
                    _groupRepository.DeleteGroup(group); //Remove invalid groups.
                }

                if (group.IsChanged)
                {
                    //The group has changed -> All the virtual sensors have to be recalculated
                    TableStorageRepository.RemoveValuesFromTablestorage(virtSensor.MACAddress);

                    // Get the oldest date in the group
                    DateTime oldestDate = DateTime.Now;
                    List<Container> containers = new List<Container>();
                    foreach (Sensor gSensor in group.Sensors)
                    {
                        Sensor gS = _sensorRepository.GetSensorById(gSensor.Id);
                        if (oldestDate > gS.CreationDate) oldestDate = gS.CreationDate;
                        foreach(Container gC in gS.Containers) containers.Add(gC);
                    }

                    foreach (Container container in virtSensor.Containers)
                    {
                        // Insert the First value in tablestorage to keep the system crashing when someone queries the new virt sensor
                        double payloadTotal = 0;
                        double numberOfContainers = 0;
                        foreach (Container c in containers)
                        {
                            if (c.ContainerType.Name.Equals(container.ContainerType.Name))
                            {
                                double? cPayload = MachineLearningRepository.ParseAverageInTime(c, oldestDate, TimeSpan.FromHours(1));
                                if (cPayload.HasValue)
                                {
                                    payloadTotal += cPayload.Value;
                                    numberOfContainers++;
                                }
                            }
                        }
                        double payload = (Math.Abs(numberOfContainers) <= 0) ? 0 : payloadTotal / numberOfContainers;

                        String payloadValue = payload.ToString(CultureInfo.InvariantCulture);
                        ContainerEntity entity = new ContainerEntity(virtSensor.Company, container.MACAddress, container.ContainerType.Name, virtSensor.Location, payloadValue, oldestDate.Ticks.ToString(), null);
                        TableStorageRepository.WriteToVirtualSensorTable(entity, false);
                    }
                }

                //Check for new virtual sensor entries
                _groupRepository.UpdateVirtualSensor(group);
            }
        }

        private static void RunPerSensor(String json)
        {
            //Parse the incoming data
            QueueMessageHolder holder = JsonConvert.DeserializeObject<QueueMessageHolder>(json);
            ContainerEntity containerEntity = TableStorageRepository.GetContainerFromTableStorage(holder.PartitionKey,
                holder.RowKey);
            if (containerEntity == null) return;

            //Database Population Management
                //Check Existance Containertype
            String containerTypeName = containerEntity.container;
            ContainerType type = GetContainerType(containerTypeName);
            if (type == null)
            {
                type = new ContainerType() {Name = containerTypeName};
                _containerTypeRepository.Insert(type);
                _containerTypeRepository.SaveChanges();
                ContainerTypeCache.Add(type);
            }  

            //Check Existance Sensor
            Sensor sensor = GetSensor(containerEntity.macaddress);
            if (sensor == null)
            {
                //Make the new sensor
                sensor = new Sensor();
                sensor.CreationDate = DateTime.Now;
                sensor.MACAddress = containerEntity.macaddress;
                sensor.SensorEntries = 1;
                sensor.Company = containerEntity.company;
                sensor.Location = containerEntity.locationid;
                sensor.Name = containerEntity.macaddress;
                sensor.Containers = new List<Container>();
                sensor = _sensorRepository.Insert(sensor);
                SensorCache.Add(sensor);

                //Subscribe on the location API
                LocationApiRepository.SubscribeOnLocation(sensor.MACAddress).Wait();
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
                //Create the container
                container = new Container();
                container.Name = containerEntity.macaddress;
                container.ContainerType = type;
                container.CreationTime = DateTime.Now;
                container.MACAddress = containerEntity.macaddress;
                container.SensorId = sensor.Id;
                _containerRepository.Insert(container);
                _containerRepository.SaveChanges();
                
                //Update the sensor
                sensor.Containers.Add(container);
                _sensorRepository.Update(sensor);
                _sensorRepository.SaveChanges();
            }

            //Error-Warning Module

            //TODO: Implement Threshold
                //Check voor de huidige container of de waarde overschreden is.
                    //-> Ja: Resulteerd in MinThresholdWarning of MaxThresholdWarning
                    //-> Indien containertype battery:

                //Check for battery Level warnings
            if (container.ContainerType.Name == "Battery level")
            {
                if (containerEntity.payload < 5)
                {
                    //Get the time to live
                    TimeSpan timeToLive = MachineLearningRepository.CalculateTimeToLive(container, containerEntity.payload);

                    //Give the error
                    Error error = Error.BatteryCriticalError(sensor, container, timeToLive);
                    _errorRepository.Insert(error);
                } else if (containerEntity.payload < 15)
                {                    
                    //Get the time to live
                    TimeSpan timeToLive = MachineLearningRepository.CalculateTimeToLive(container, containerEntity.payload);

                    //Give the error
                    Error error = Error.BatteryWarning(sensor, container, timeToLive);
                    _errorRepository.Insert(error);
                }
            }
            _errorRepository.SaveChanges();
        }

        private static void RunOnceInWebjobEnd()
        {
            //Error-Warning Module
                //Predict Network Connectivity Errors per Sensor (-> Triggered when update is slower than 5 times the average updateSpan.)
            List<Sensor> sensors = _sensorRepository.GetSensors();
            foreach (Sensor sensor in sensors)
            {
                //Calculate a rudimentary the average ticks between all entries for a sensor
                long creationTimeTicks = sensor.CreationDate.Ticks;
                long averageTicksBetweenEntry = creationTimeTicks/sensor.SensorEntries;

                bool isActive = false;
                DateTime mostRecentValue = DateTime.MinValue;
                foreach (Container container in sensor.Containers)
                {
                    //Set the most recent value
                    Container UpdatedContainer = TableStorageRepository.UpdateValue(container);
                    if (UpdatedContainer.CurrentValue.Timestamp > mostRecentValue)
                        mostRecentValue = UpdatedContainer.CurrentValue.Timestamp;

                    //While looping the containers in the sensor, check for sensor/container inactivity as well
                    if (isActive) break;

                    TimeSpan updatedSince = DateTime.Now - UpdatedContainer.CurrentValue.Timestamp;
                    if (updatedSince.Hours > 48)
                    {
                        Error error = Error.InactiveContainerWarning(sensor, container);
                        _errorRepository.Insert(error);
                    } else isActive = true;

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
        }

        #region CacheLogic

        private static ContainerType GetContainerType(String containerTypeName)
        {
            ContainerType cacheType =
                (from t in ContainerTypeCache where t.Name.Equals(containerTypeName) select t).FirstOrDefault();
            if (cacheType == null)
            {
                ContainerType dbType = _containerTypeRepository.GetContainerTypeByName(containerTypeName);
                if(dbType != null) ContainerTypeCache.Add(dbType);
                return dbType;
            }

            return cacheType;
        }

        private static Sensor GetSensor(String MacAddress)
        {
            return _sensorRepository.GetSensorByMacAddress(MacAddress);
            //Sensor cacheSensor = (from s in SensorCache where s.MACAddress.Equals(MacAddress) select s).FirstOrDefault();
            //if (cacheSensor == null)
            //{
            //    Sensor dbSensor = _sensorRepository.GetSensorByMacAddress(MacAddress);
            //    if(dbSensor != null) SensorCache.Add(dbSensor);
            //    return dbSensor;
            //}
            //return cacheSensor;
        }

        #endregion
    }
}
