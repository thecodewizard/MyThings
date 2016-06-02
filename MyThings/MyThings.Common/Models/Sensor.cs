using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models
{
    public class Sensor
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Company { get; set; }
        public String MACAddress { get; set; }
        public String Location { get; set; }
        public DateTime CreationDate { get; set; }
        public long SensorEntries { get; set; }

        public Double BasestationLat { get; set; }
        public Double BasestationLng { get; set; }

        public List<Container> Containers { get; set; }
    }
}
