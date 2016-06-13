using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MyThings.Common.Models.NoSQL_Entities
{
    public class GridLayoutEntity : TableEntity
    {
        public GridLayoutEntity()
        {
            
        }
        public GridLayoutEntity(String userId, String gridsterJson)
        {
            this.PartitionKey = userId;
            this.RowKey = DateTime.Now.Ticks.ToString();
            this.GridsterJson = gridsterJson;
        }

        public String GridsterJson { get; set; }
    }
}