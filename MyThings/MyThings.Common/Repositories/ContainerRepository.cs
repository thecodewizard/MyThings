using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;
using System.Data.Entity;

namespace MyThings.Common.Repositories
{
    public class ContainerRepository : GenericRepository<Container>
    {
        #region GenericRepository - Eager Loading Adaptations

        public override IEnumerable<Container> All()
        {
            return (from c in Context.Container.Include(c => c.ContainerType) select c).ToList();
        }

        public override Container GetByID(object id)
        {
            return (from c in Context.Container.Include(c => c.ContainerType) select c).FirstOrDefault();
        }

        public override Container Insert(Container entity)
        {
            return base.Insert(entity);
        }

        //public override Sensor Insert(Sensor sensor)
        //{
        //    foreach (Container container in sensor.Containers)
        //        if (Context.Entry<Container>(container).State != EntityState.Unchanged)
        //            Context.Entry<Container>(container).State = EntityState.Unchanged;

        //    foreach (Group group in sensor.Groups)
        //        if (Context.Entry<Group>(group).State != EntityState.Unchanged)
        //            Context.Entry<Group>(group).State = EntityState.Unchanged;

        //    Context.Sensors.Add(sensor);
        //    return sensor;
        //}
        #endregion
    }
}
