﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Repositories;

namespace MyThings.Common.Models
{
    public class Sensor
    {
        //Fields
        public int Id { get; set; }
        public String Name { get; set; }
        public String Company { get; set; }
        public String MACAddress { get; set; }
        public String Location { get; set; }
        public DateTime CreationDate { get; set; }
        public long SensorEntries { get; set; }

        //Location
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Accuracy { get; set; }

        //References
        public List<Container> Containers { get; set; }

        //Virtual Sensor
        public bool IsVirtual { get; set; }
    }
}
