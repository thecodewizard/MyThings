using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Repositories;

namespace MyThings.Common.Models
{
    public class Group
    {
        //Fields
        public int Id { get; set; }
        public String Name { get; set; }

        //References
        public List<Sensor> Sensors { get; set; }

        //Functionality
        public Group Save()
        {
            //Only use this method to create a single group. With multiple groups, working with the repository directly is more efficient.
            GroupRepository groupRepository = new GroupRepository();
            if (this.Id == 0)
            {
                //The group does not have an ID -> Add this to the database
                Group savedGroup = groupRepository.Insert(this);
                groupRepository.SaveChanges();
                this.Id = savedGroup.Id;
                return savedGroup;
            } else
            {
                //The group has an ID -> Update the existing group
                groupRepository.Update(this);
            }
            return this;
        }
    }
}
