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
        public String User_Id { get; set; }

        //References
        public List<Sensor> Sensors { get; set; }
    }
}
