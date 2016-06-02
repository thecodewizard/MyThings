using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models
{
    public class Container
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public DateTime CreationTime { get; set; }

        //This is fetched from the NoSql -> Exclude from relational
        [NotMapped]
        public float Value { get; set; }
        [NotMapped]
        public DateTime ValueTime { get; set; }
    }
}
