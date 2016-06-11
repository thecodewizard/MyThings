using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MyThings.Common.Models;
using MyThings.Common.Models.NoSQL_Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MyThings.Common.Repositories
{
    public class TableStorageRepository
    {
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

            container.CurrentValue = new ContainerValue(entity.payload, entity.Timestamp.LocalDateTime);
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
                container.History.Add(new ContainerValue(entity.payload, entity.Timestamp.LocalDateTime));
            }

            if(container.History.Count >= 1)
            {
                container.CurrentValue = new ContainerValue(container.History.First().Value, container.History.First().Timestamp);
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
    }
}
