using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models.FrontEndModels
{
    [NotMapped]
    public class ThresholdCreator : Threshold
    {
        public int ContainerId { get; set; }
    }
}
