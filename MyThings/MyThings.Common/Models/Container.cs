using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models
{
    public class Container
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }

        //References
        public int ContainerTypeId { get; set; }
        public ContainerType ContainerType { get; set; }

        public int? SensorId { get; set; }
        public Sensor Sensor { get; set; }

        //This is fetched from the NoSql -> Exclude from relational
        [NotMapped]
        public ContainerValue CurrentValue { get; set; }

        //Trend
        [NotMapped]
        public List<ContainerValue> History { get; set; }
        [NotMapped]
        public List<ContainerValue> Prediction { get; set; }
    }
}
