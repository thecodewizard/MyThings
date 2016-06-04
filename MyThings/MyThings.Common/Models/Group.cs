using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models
{
    public class Group
    {
        public int Id { get; set; }
        public String Name { get; set; }

        //References
        public List<Sensor> Sensors { get; set; }
    }
}
