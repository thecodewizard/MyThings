using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Repositories;

namespace MyThings.Common.Models
{
    public class Container
    {
        //Fields
        public int Id { get; set; }
        public String Name { get; set; }
        public DateTime CreationTime { get; set; }

        //References
        public int ContainerTypeId { get; set; }
        public ContainerType ContainerType { get; set; }

        public int? SensorId { get; set; }

        //This is fetched from the NoSql -> Exclude from relational
        [NotMapped]
        public ContainerValue CurrentValue { get; set; }

        //Trend
        [NotMapped]
        public List<ContainerValue> History { get; set; }
        [NotMapped]
        public double Value { get; set; }
        [NotMapped]
        public DateTime ValueTime { get; set; }

        //Functionality
        public Container Save()
        {
            //Only use this method to create a single container. With multiple container, working with the repository directly is more efficient.
            ContainerRepository containerRepository = new ContainerRepository();
            if (this.Id == 0)
            {
                //The container does not have an ID -> Add this to the database
                Container savedContainer = containerRepository.Insert(this);
                containerRepository.SaveChanges();
                return savedContainer;
            } else
            {
                //The containecontainerrType has an ID -> Update the existing container
                containerRepository.Update(this);
            }
            return this;
        }
    }
}
