using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Repositories;

namespace MyThings.Common.Models
{
    public class Container
    {
        //Fields
        public int Id { get; set; }
        public String Name { get; set; }
        public String MACAddress { get; set; }
        public DateTime CreationTime { get; set; }

        //References
        public int ContainerTypeId { get; set; }
        public ContainerType ContainerType { get; set; }

        public int? SensorId { get; set; }

        public int? ThresholdId { get; set; }
        public Threshold Threshold { get; set; }

        //This is fetched from the NoSql -> Exclude from relational
        [NotMapped]
        public DateTime LastUpdatedTime { get; set; }
        [NotMapped]
        public ContainerValue CurrentValue { get; set; }
        [NotMapped]
        public List<ContainerValue> History { get; set; }
    }
}
