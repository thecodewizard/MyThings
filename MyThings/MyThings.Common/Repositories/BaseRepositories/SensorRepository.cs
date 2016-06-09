using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Context;
using MyThings.Common.Models;
using System.Data.Entity;

namespace MyThings.Common.Repositories
{
    public class SensorRepository : GenericRepository<Sensor>
    {
        #region GenericRepository - Eager Loading Adaptations
        public override IEnumerable<Sensor> All()
        {
            return
                (from s in Context.Sensors
                    .Include(s => s.Containers.Select(c => c.ContainerType))
                 orderby s.CreationDate descending
                 select s)
                    .ToList();
        }

        public override Sensor GetByID(object id)
        {
            int sensorId = -1;
            return !int.TryParse(id.ToString(), out sensorId)
                ? null
                : (from s in Context.Sensors
                    .Include(s => s.Containers.Select(c => c.ContainerType))
                    where s.Id == sensorId
                    select s).FirstOrDefault();
        }

        public override void Update(Sensor sensor)
        {
            if (sensor.Containers != null)
                foreach (Container container in sensor.Containers)
                {
                    if (Context.Entry(container).State != EntityState.Unchanged)
                        Context.Entry(container).State = EntityState.Unchanged;
                    if(container.ContainerType != null && Context.Entry(container.ContainerType).State != EntityState.Unchanged)
                        Context.Entry(container.ContainerType).State = EntityState.Unchanged;
                }

            DbSet.Attach(sensor);
            Context.Entry(sensor).State = EntityState.Modified;
        }

        public override Sensor Insert(Sensor sensor)
        {
            if(sensor.Containers != null)
                foreach (Container container in sensor.Containers)
                {
                    if (Context.Entry<Container>(container).State != EntityState.Unchanged)
                        Context.Entry<Container>(container).State = EntityState.Unchanged;
                    if (container.ContainerType != null && Context.Entry(container.ContainerType).State != EntityState.Unchanged)
                        Context.Entry(container.ContainerType).State = EntityState.Unchanged;
                }

            Context.Sensors.Add(sensor);
            return sensor;
        }
        #endregion

        #region Functionality Methods

        public List<Sensor> GetSensors(int? count = null)
        {
            if (count.HasValue)
                return
                    (from s in Context.Sensors
                        .Include(s => s.Containers.Select(c => c.ContainerType))
                     orderby s.CreationDate descending 
                     select s)
                        .Take(count.Value)
                        .ToList();
            
            return All().ToList();
        }

        public Sensor GetSensorById(int sensorId)
        {
            return GetByID(sensorId);
        }

        public Sensor GetSensorByMacAddress(String MAC)
        {
            return
                (from s in Context.Sensors orderby s.CreationDate descending where s.MACAddress.Equals(MAC) select s)
                    .FirstOrDefault();
        }

        public void DeleteSensor(Sensor sensor)
        {
            Delete(sensor);
            SaveChanges();
        }

        //public Sensor SaveOrUpdateSensor(Sensor sensor)
        //{
        //    if (DbSet.Find(sensor.Id) != null)
        //    {
        //        //The sensor already exists -> Update the sensor
        //        Update(sensor);
        //    }
        //    else
        //    {
        //        //The sensor doesn't exist -> Insert the sensor
        //        sensor = Insert(sensor);
        //    }

        //    SaveChanges();
        //    return sensor;
        //}

        #endregion
    }
}
