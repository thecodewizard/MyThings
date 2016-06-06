using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MyThings.Common.Models;
using MyThings.Common.Models.NoSQL_Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("proximusdecodedtable");
            // Create the table query.
            TableQuery<ContainerEntity> rangeQuery = new TableQuery<ContainerEntity>().Where(TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, container.Sensor.MACAddress), TableOperators.And,
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
            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("proximusdecodedtable");
            // Create the table query.

            string tijd = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks + timespan.Ticks).ToString();

            var rangeQuery = (from entry in table.CreateQuery<ContainerEntity>()
                            where entry.PartitionKey == container.Sensor.MACAddress
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
    }
}
