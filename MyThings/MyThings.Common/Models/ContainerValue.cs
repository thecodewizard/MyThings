using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models
{
    [NotMapped]
    public class ContainerValue
    {
        public ContainerValue(double value, DateTime timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }

        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
