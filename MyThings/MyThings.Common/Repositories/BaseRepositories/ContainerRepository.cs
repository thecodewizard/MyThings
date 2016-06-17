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
        private List<Sensor> cacheSensors = new List<Sensor>();
        private SensorRepository _sensorRepository = new SensorRepository();

        public override IEnumerable<Container> All()
        {
            List<Container> containers = (from c in Context.Container.Include(c => c.ContainerType).Include(c => c.Threshold) orderby c.CreationTime descending select c).ToList();

            foreach (Container container in containers)
            {
                Sensor sensor = (from s in cacheSensors where s.Id.Equals(container.Id) select s).FirstOrDefault();
                if (sensor == null) sensor = _sensorRepository.GetSensorById(container.SensorId.Value);

                if (sensor != null)
                {
                    container.Name = sensor.Name;
                }
            }
            return containers;
        }

        public override Container GetByID(object id)
        {
            int containerId = -1;
            Container container = !int.TryParse(id.ToString(), out containerId)
                ? null
                : (from c in Context.Container.Include(c => c.ContainerType).Include(c => c.Threshold) where c.Id.Equals(containerId) select c).FirstOrDefault();

            if (container == null) return null;
            Sensor sensor = (from s in cacheSensors where s.Id.Equals(container.Id) select s).FirstOrDefault();
            if (sensor == null) sensor = _sensorRepository.GetSensorById(container.SensorId.Value);

            if (sensor != null)
            {
                container.Name = sensor.Name;
            }
            return container;
        }

        public override Container Insert(Container container)
        {
            if (container.ContainerType != null && Context.Entry(container.ContainerType).State != EntityState.Unchanged)
                Context.Entry(container.ContainerType).State = EntityState.Unchanged;

            Context.Container.Add(container);
            return container;
        }

        public override void Delete(Container container)
        {
            if (container.ContainerType != null && Context.Entry(container.ContainerType).State != EntityState.Unchanged)
                Context.Entry(container.ContainerType).State = EntityState.Unchanged;

            if (Context.Entry(container).State == EntityState.Detached)
            {
                DbSet.Attach(container);
            }
            DbSet.Remove(container);
        }

        #endregion

        #region Functionality Methods

        public List<Container> GetContainers(int? count = null)
        {
            if (count.HasValue)
                return
                    (from c in Context.Container.Include(c => c.ContainerType)
                        orderby c.CreationTime descending
                        select c).Take(count.Value).ToList();

            return All().ToList();
        }

        public Container GetContainerById(int containerId)
        {
            return GetByID(containerId);
        }

        public void DeleteContainer(Container container)
        {
            Delete(container);
            SaveChanges();
        }

        //public Container SaveOrUpdateContainer(Container container)
        //{
        //    if (DbSet.Find(container.Id) != null)
        //    {
        //        //The container already exists -> Update the container
        //        Update(container);
        //    } else
        //    {
        //        //The container doesn't exist -> Insert the container
        //        container = Insert(container);
        //    }

        //    SaveChanges();
        //    return container;
        //}

        #endregion
    }
}
