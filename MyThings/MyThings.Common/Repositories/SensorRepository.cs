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
                    .Include(s => s.Groups)
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
                    .Include(s => s.Groups)
                    where s.Id == sensorId
                    select s).FirstOrDefault();
        }

        public override Sensor Insert(Sensor sensor)
        {
            foreach (Container container in sensor.Containers)
                if (Context.Entry<Container>(container).State != EntityState.Unchanged)
                    Context.Entry<Container>(container).State = EntityState.Unchanged;

            foreach (Group group in sensor.Groups)
                if (Context.Entry<Group>(group).State != EntityState.Unchanged)
                    Context.Entry<Group>(group).State = EntityState.Unchanged;

            Context.Sensors.Add(sensor);
            return sensor;
        }
        #endregion

        #region Functionality Methods

        public List<Sensor> GetSensors()
        {
            return All().ToList();
        }

        public Sensor SaveSensor(Sensor sensor)
        {
            Sensor savedSensor = Insert(sensor);
            SaveChanges();
            return savedSensor;
        }

        #endregion
    }
}
