using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models.NoSQL_Entities
{
    public class ContainerEntity : TableEntity
    {
        public ContainerEntity(string company, string macaddress, string container, string locationid, string payload, string timestamp)
        {
            this.PartitionKey = macaddress;
            this.RowKey = container + DateTime.Now.Ticks.ToString();
            this.company = company;
            this.macaddress = macaddress;
            this.container = container;
            this.locationid = locationid;
            this.payload = double.Parse(payload, CultureInfo.InvariantCulture);
            this.receivedtimestamp = timestamp;
            this.Timestamp = DateTime.Now;
        }

        public ContainerEntity() { }
        public string company { get; set; }
        public string macaddress { get; set; }
        public string container { get; set; }
        public string locationid { get; set; }
        public double payload { get; set; }
        public string receivedtimestamp { get; set; }

    }
}
