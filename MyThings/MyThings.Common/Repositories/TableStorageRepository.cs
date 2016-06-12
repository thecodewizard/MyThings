using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MyThings.Common.Models;
using MyThings.Common.Models.NoSQL_Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace MyThings.Common.Repositories
{
    public class TableStorageRepository
    {
        #region Decoded Table Storage

        public static Container UpdateValue(Container container)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("proximusdecodedtable");
            // Create the table query.
            TableQuery<ContainerEntity> rangeQuery = new TableQuery<ContainerEntity>().Where(TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, container.MACAddress), TableOperators.And,
                            TableQuery.GenerateFilterCondition("container", QueryComparisons.Equal, container.ContainerType.Name))
                            ).Take(1);

            // Loop through the results, displaying information about the entity.
            ContainerEntity entity = table.ExecuteQuery(rangeQuery).First<ContainerEntity>();

            container.CurrentValue = new ContainerValue(entity.payload, entity.hexpayload, entity.Timestamp.LocalDateTime);
            container.LastUpdatedTime = DateTime.Now;
            return container;
        }

        public static Container GetHistory(Container container, TimeSpan timespan)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("proximusdecodedtable");
            // Create the table query.

            string tijd = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks + timespan.Ticks).ToString();

            var rangeQuery = (from entry in table.CreateQuery<ContainerEntity>()
                            where entry.PartitionKey == container.MACAddress
                            && entry.container == container.ContainerType.Name
                            && entry.RowKey.CompareTo(String.Format("{0:D19}", tijd)) <= 0
                            select entry);

            // Loop through the results, displaying information about the entity.
            if (container.History == null) container.History = new List<ContainerValue>();
            foreach (ContainerEntity entity in rangeQuery)
            {
                container.History.Add(new ContainerValue(entity.payload, entity.hexpayload, entity.Timestamp.LocalDateTime));
            }

            if(container.History.Count >= 1)
            {
                container.CurrentValue = new ContainerValue(container.History.First().Value, container.History.First().HexValue, container.History.First().Timestamp);
                container.LastUpdatedTime = DateTime.Now;
            }
            return container;
        }

        public static ContainerEntity GetContainerFromTableStorage(String partitionkey, String rowkey)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("proximusdecodedtable");
            // Create the table query.
            TableQuery<ContainerEntity> rangeQuery = new TableQuery<ContainerEntity>().Where(TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey), TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowkey))
                            ).Take(1);

            ContainerEntity entity;
            try
            {
                // Loop through the results, displaying information about the entity.
                entity = table.ExecuteQuery(rangeQuery).First<ContainerEntity>();
            }
            catch (Exception ex)
            {
                entity = null;
            }

            return entity;
        }

        #endregion

        #region Network Table Storage

        public static Sensor UpdateBasestationCoordinates(Sensor sensor)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("proximusnetwerktable");
            // Create the table query.
            TableQuery<NetwerkEntity> rangeQuery =
                new TableQuery<NetwerkEntity>().Where(TableQuery.GenerateFilterCondition("DevEUI",
                    QueryComparisons.Equal, sensor.MACAddress)).Take(1);
            NetwerkEntity entity = table.ExecuteQuery(rangeQuery).FirstOrDefault();
            if (entity == null) return sensor;

            sensor.Accuracy = 1;
            bool success = true;
            if (entity.LrrLAT != null)
            {
                double lat;
                success = double.TryParse(entity.LrrLAT, out lat);
                if (success && Math.Abs(lat) > 0)
                {
                    sensor.Lat = lat;
                }
            }
            if (entity.LrrLON != null)
            {
                double lng;
                success = double.TryParse(entity.LrrLON, out lng);
                if (success && Math.Abs(lng) > 0)
                {
                    sensor.Lng = lng;
                }
            }

            return sensor;
        }

        #endregion

        #region Virtual Sensors

        public static void WriteToVirtualSensorTable(ContainerEntity raw, bool putOnStorageQueue = true)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("virtualsensorvalues");
            table.CreateIfNotExists();
            // Make the entity
            DecodedEntity entity = new DecodedEntity(raw.company, raw.macaddress, raw.container, raw.locationid, raw.payload.ToString(CultureInfo.InvariantCulture), raw.receivedtimestamp);
            // Create the TableOperation object.
            TableOperation insertOperation = TableOperation.Insert(entity);
            // Execute the insert operation.
            table.Execute(insertOperation);
            if (putOnStorageQueue)
            {
                PutOnStorageQueue(entity.PartitionKey, entity.RowKey);
            }
        }

        public static ContainerEntity GetMostRecentVirtualSensorEntity(String macAddress, String containerTypeName)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("virtualsensorvalues");
            // Create the table query.
            TableQuery<ContainerEntity> rangeQuery = new TableQuery<ContainerEntity>().Where(TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, macAddress), TableOperators.And,
                            TableQuery.GenerateFilterCondition("container", QueryComparisons.Equal, containerTypeName))
                            ).Take(1);

            ContainerEntity entity;
            try
            {
                // Loop through the results, displaying information about the entity.
                entity = table.ExecuteQuery(rangeQuery).First<ContainerEntity>();
            }
            catch (Exception ex)
            {
                entity = null;
            }

            return entity;
        }

        public static void RemoveValuesFromTablestorage(String macAddress)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                       ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("virtualsensorvalues");
            // Create the table query.
            var rangeQuery = (from entry in table.CreateQuery<ContainerEntity>()
                              where entry.macaddress == macAddress
                              select entry);

            // Delete the entities
            List<TableOperation> operations = new List<TableOperation>();
            foreach (ContainerEntity entity in rangeQuery)
            {
                if (entity != null) operations.Add(TableOperation.Delete(entity));
            }
            foreach (TableOperation operation in operations)
            {
                table.Execute(operation);
            }
        }

        #region Entity

        private class DecodedEntity : TableEntity
        {
            public DecodedEntity(string company, string macaddress, string container, string locationid, string payload, string timestamp)
            {
                this.PartitionKey = macaddress;
                this.RowKey = String.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
                this.company = company;
                this.macaddress = macaddress;
                this.container = container;
                this.locationid = locationid;
                //this.payload = double.Parse(payload, CultureInfo.InvariantCulture);
                this.receivedtimestamp = timestamp;
                this.Timestamp = DateTime.Now;

                double parsedDouble;
                bool isHex = !double.TryParse(payload, out parsedDouble);
                if (isHex) this.hexpayload = payload;
                else this.payload = parsedDouble;
            }

            public DecodedEntity() { }
            public string company { get; set; }
            public string macaddress { get; set; }
            public string container { get; set; }
            public string locationid { get; set; }
            public double payload { get; set; }
            public string hexpayload { get; set; }
            public string receivedtimestamp { get; set; }
        }

        #endregion

        #endregion

        #region Storage queue

        public static void PutOnStorageQueue(String partitionkey, String rowkey)
        {
            // Make the object that will carry the queuemessage
            QueueMessageHolder holder = new QueueMessageHolder(partitionkey, rowkey);
            String json = JsonConvert.SerializeObject(holder);

            //Retrieve Connection String
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            //Create Queue Client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            //Make or Append to queue
            CloudQueue queue = queueClient.GetQueueReference("mythingsdecodedqueue");

            //Create If The Queue doesn't already exists
            queue.CreateIfNotExists();

            //Add the Order to the Queue
            CloudQueueMessage message = new CloudQueueMessage(json);
            queue.AddMessage(message);
        }

        #endregion
    }
}
