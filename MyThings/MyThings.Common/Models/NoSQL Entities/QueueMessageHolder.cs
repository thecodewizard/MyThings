using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models.NoSQL_Entities
{
    public class QueueMessageHolder
    {
        public String PartitionKey { get; set; }
        public String RowKey { get; set; }

        public QueueMessageHolder(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}
