using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using MyThings.Common.Models;
using MyThings.Common.Models.NoSQL_Entities;
using MyThings.Common.Repositories;
using MyThings.Common.Repositories.RemoteRepositories;
using Newtonsoft.Json;

namespace MyThings.TableToQueueConverter
{
    class Program
    {
        static SensorRepository sensorRepository = new SensorRepository();
        static ContainerRepository containerRepository = new ContainerRepository();
        static GroupRepository groupRepository = new GroupRepository();

        static void Main(string[] args)
        {
            //PortTableStorageToQueue();
            FetchLocationForAllSensors().Wait();
            RenameAllContainers();
        }

        private static void RenameAllContainers()
        {
            ////Rename all containers after their sensor
            //List<Container> containers = containerRepository.GetContainers();
            //foreach (Container container in containers)
            //{
            //    if (container.SensorId.HasValue)
            //    {
            //        Sensor sensor = sensorRepository.GetSensorById(container.SensorId.Value);
            //        if (sensor != null)
            //        {
            //            container.Name = sensor.Name;
            //            containerRepository.Update(container);
            //        }
            //    }
            //}
            //containerRepository.SaveChanges();

            //Rename all virtual containers to their group and type
            List<Group> groups = groupRepository.GetGroups();
            foreach (Group group in groups)
            {
                Sensor virtSensor = sensorRepository.GetSensorById(group.VirtualSensorIdentifier);
                if (virtSensor != null)
                {
                    if(virtSensor.Containers != null)
                    foreach (Container container in virtSensor.Containers)
                    {
                        String name = "VContainer " + group.Id + " - " + container.ContainerType.Name;
                        container.Name = name;
                        containerRepository.Update(container);
                    }
                }
            }
            containerRepository.SaveChanges();
        }

        private static async Task FetchLocationForAllSensors()
        {
            List<Sensor> sensors = sensorRepository.GetSensors();
            foreach (Sensor sensor in sensors)
            {
                Log("Fetching Location for sensor " + sensor.Name);
                await LocationApiRepository.UpdateSensorLocation(sensor.MACAddress);
                //Log("Subscribing sensor " + sensor.Name);
                //await LocationApiRepository.SubscribeOnLocation(sensor.MACAddress);
            }
        }

        private static void PortTableStorageToQueue()
        {
            //Get all tablestorage rows
            Log("Querying Tablestorage");
            List<ContainerEntity> entities = GetAllTableEntries();
            Log("Querying Tablestorage - Completed");

            //Sort to oldest entry first
            List<ContainerEntity> containers =
                (from e in entities orderby e.Timestamp ascending select e).ToList();

            //Push to queue, oldest first on queue
            int count = 0;
            foreach (ContainerEntity container in containers)
            {
                PutOnStorageQueue(container.PartitionKey, container.RowKey);
                Log("Adding To Queue - " + count++);
            }
        }

        private static void Log(String logtext)
        {
            Console.WriteLine(logtext);
        }

        private static List<ContainerEntity> GetAllTableEntries()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("proximusdecodedtable");

            // Query the table 1000 row at a time.
            TableContinuationToken token = null;
            var entities = new List<ContainerEntity>();
            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<ContainerEntity>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);
            return entities;
        }

        public static void PutOnStorageQueue(String partitionkey, String rowkey)
        {
            // Make the object that will carry the queuemessage
            QueueMessageHolder holder = new QueueMessageHolder(partitionkey, rowkey);
            String json = JsonConvert.SerializeObject(holder);

            //Retrieve Connection String
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            //Create Queue Client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            //Make or Append to queue
            CloudQueue queue = queueClient.GetQueueReference("mythingsdecodedqueuetest");

            //Create If The Queue doesn't already exists
            queue.CreateIfNotExists();

            //Add the Order to the Queue
            CloudQueueMessage message = new CloudQueueMessage(json);
            queue.AddMessage(message);
        }
    }
}
