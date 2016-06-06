using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Repositories;

namespace MyThings.Common.Models
{
    public class ContainerType
    {
        //Fields
        public int Id { get; set; }
        public String Name { get; set; }

        //Functionality
        public ContainerType Save()
        {
            //Only use this method to create a single containerType. With multiple containerType, working with the repository directly is more efficient.
            ContainerTypeRepository containerTypeRepository = new ContainerTypeRepository();
            if (this.Id == 0)
            {
                //The containerType does not have an ID -> Add this to the database
                ContainerType savedContainerType = containerTypeRepository.Insert(this);
                containerTypeRepository.SaveChanges();
                return savedContainerType;
            } else
            {
                //The containerType has an ID -> Update the existing containerType
                containerTypeRepository.Update(this);
            }
            return this;
        }
    }
}
