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

            container.Value = entity.payload;
            container.ValueTime = entity.Timestamp.LocalDateTime;
            container.LastUpdatedTime = DateTime.Now;
            return container;
        }
    }
}
