using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models
{
    public class Timeholder
    {
        public int Id { get; set; }
        public DateTime WebjobInstanceStarted { get; set; }
        public DateTime WebjobInstanceEnded { get; set; }
    }
}
