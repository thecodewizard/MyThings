using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models.FrontEndModels
{
    public class GroupCreator
    {
        [Required]
        public String name { get; set; }
        [Required]
        public List<int> sensors { get; set; }

        public bool autoPinGroup { get; set; }
    }
}
