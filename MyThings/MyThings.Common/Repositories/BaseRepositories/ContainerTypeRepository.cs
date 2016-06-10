using System;
using System.Collections.Generic;
using System.Linq;
using MyThings.Common.Models;

namespace MyThings.Common.Repositories
{
    public class ContainerTypeRepository : GenericRepository<ContainerType>
    {
        #region Functionality Methods

        public List<ContainerType> GetContainerTypes()
        {
            return All().ToList();
        }

        public ContainerType GetContainerTypeByID(int containerTypeId)
        {
            return GetByID(containerTypeId);
        }

        public ContainerType GetContainerTypeByName(String name)
        {
            return (from c in Context.ContainerTypes where c.Name == name select c).FirstOrDefault();
        }

        public void DeleteContainerType(ContainerType containerType)
        {
            Delete(containerType);
            SaveChanges();
        }

        #endregion
    }
}

