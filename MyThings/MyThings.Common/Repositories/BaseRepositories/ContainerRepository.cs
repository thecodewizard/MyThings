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
            return (from c in Context.Container.Include(c => c.ContainerType).Include(c => c.Threshold) orderby c.CreationTime descending select c).ToList();
        }

        public override Container GetByID(object id)
        {
            int containerId = -1;
            return !int.TryParse(id.ToString(), out containerId)
                ? null
                : (from c in Context.Container.Include(c => c.ContainerType).Include(c => c.Threshold) where c.Id.Equals(containerId) select c).FirstOrDefault();
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

        #endregion
    }
}
