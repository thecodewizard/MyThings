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
            return (from c in Context.Container.Include(c => c.ContainerType) orderby c.CreationTime descending select c).ToList();
        }

        public override Container GetByID(object id)
        {
            return (from c in Context.Container.Include(c => c.ContainerType) select c).FirstOrDefault();
        }

        public override Container Insert(Container container)
        {
            if (container.ContainerType != null && Context.Entry(container.ContainerType).State != EntityState.Unchanged)
                Context.Entry(container.ContainerType).State = EntityState.Unchanged;

            Context.Container.Add(container);
            return container;
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
