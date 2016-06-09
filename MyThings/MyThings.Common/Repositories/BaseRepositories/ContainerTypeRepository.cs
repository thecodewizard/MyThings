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

        public ContainerType SaveOrUpdateContainerType(ContainerType containerType)
        {
            if (DbSet.Find(containerType.Id) != null)
            {
                //The containerType already exists -> Update the containerType
                Update(containerType);
            } else
            {
                //The containerType doesn't exist -> Insert the containerType
                ContainerType foundType = (from c in Context.ContainerTypes where c.Name == containerType.Name select c).FirstOrDefault();
                if (foundType == null) //Do not allow a containerType with the same name
                {
                    containerType = Insert(containerType);
                } else return foundType;
            }

            SaveChanges();
            return containerType;
        }

        public void DeleteContainerType(ContainerType containerType)
        {
            Delete(containerType);
            SaveChanges();
        }

        #endregion
    }
}

