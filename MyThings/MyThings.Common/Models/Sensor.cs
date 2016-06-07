using System;
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

        public Double BasestationLat { get; set; }
        public Double BasestationLng { get; set; }

        //References
        public List<Container> Containers { get; set; }

        //Functionality
        public Sensor Save()
        {
            //Only use this method to create a single sensor. With multiple sensors, working with the repository directly is more efficient.
            SensorRepository sensorRepository = new SensorRepository();
            if (this.Id == 0)
            {
                //The sensor does not have an ID -> Add this to the database
                Sensor savedSensor = sensorRepository.Insert(this);
                sensorRepository.SaveChanges();
                this.Id = savedSensor.Id;
                return savedSensor;
            }
            else
            {
                //The sensor has an ID -> Update the existing sensor
                sensorRepository.Update(this);
            }
            return this;
        }
    }
}
