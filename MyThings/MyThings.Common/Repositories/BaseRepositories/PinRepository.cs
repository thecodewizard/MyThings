using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MyThings.Common.Helpers;
using MyThings.Common.Models;
using MyThings.Common.Models.NoSQL_Entities;

namespace MyThings.Common.Repositories
{
    public class PinRepository
    {
        #region TableStorage Methods

        public GridLayoutEntity GetGridsterJson(String userId)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("UserGridLayout");
            // Create the table query.
            TableQuery<GridLayoutEntity> rangeQuery =
                new TableQuery<GridLayoutEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId)).Take(1);

            // Loop through the results, displaying information about the entity.
            return table.ExecuteQuery(rangeQuery).FirstOrDefault<GridLayoutEntity>();
        }

        public void UpdateGridsterJson(String userId, String gridJson)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object.
            CloudTable table = tableClient.GetTableReference("UserGridLayout");

            // Create a retrieve operation.
            TableQuery<GridLayoutEntity> rangeQuery =
                new TableQuery<GridLayoutEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId)).Take(1);

            // Execute the operation.
            GridLayoutEntity updateEntity = table.ExecuteQuery(rangeQuery).FirstOrDefault<GridLayoutEntity>();

            if (updateEntity != null)
            {
                // Change the phone number.
                updateEntity.GridsterJson = gridJson;

                // Create the Replace TableOperation.
                TableOperation updateOperation = TableOperation.Replace(updateEntity);

                // Execute the operation.
                table.Execute(updateOperation);
            }
        }

        #endregion

        #region Static Pin Constructors

        public static Pin RenderClockPinForUser(String userid)
        {
            Pin clock = new Pin();
            clock.SavedId = 0;
            clock.SavedType = PinType.FixedClock;
            clock.UserId = userid;

            return clock;
        }

        public static List<Pin> RenderNavigationPinsForUser(String userid)
        {
            List<Pin> navigationpins = new List<Pin>();
            for (int i = 0; i < 6; i++)
            {
                Pin navItem = new Pin();
                navItem.SavedId = i;
                navItem.SavedType = PinType.FixedNavigation;
                navItem.UserId = userid;
                navigationpins.Add(navItem);
            }

            return navigationpins;
        }

        public static List<Pin> RenderErrorPinsForUser(String userid)
        {
            List<Pin> errorPins = new List<Pin>();
            for (int i = 0; i < 2; i++)
            {
                Pin errorItem = new Pin();
                errorItem.SavedId = i;
                errorItem.SavedType = PinType.FixedError;
                errorItem.UserId = userid;
                errorPins.Add(errorItem);
            }

            return errorPins;
        }

        #endregion

        #region Smart Methods

        public List<PinType> GetFoundPinTypesById(String userId, int valueId)
        {
            List<Tile> tiles = GridsterHelper.JsonToTileList(GetGridsterJson(userId).GridsterJson);
            return (from t in tiles select t.Pin.SavedType).Distinct().ToList();
        }

        public bool IsSensorPinned(String userId, int sensorId)
        {
            return GetFoundPinTypesById(userId, sensorId).Contains(PinType.Sensor);
        }

        public bool IsContainerPinned(String userId, int containerId)
        {
            return GetFoundPinTypesById(userId, containerId).Contains(PinType.Container);
        }

        public bool IsGroupPinned(String userId, int groupId)
        {
            return GetFoundPinTypesById(userId, groupId).Contains(PinType.Group);
        }

        public bool IsErrorPinned(String userId, int errorId)
        {
            return GetFoundPinTypesById(userId, errorId).Contains(PinType.Error);
        }

        #endregion
    }
}
