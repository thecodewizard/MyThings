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
        public ContainerValue(double value, String hexvalue, DateTime timestamp)
        {
            Value = value;
            HexValue = hexvalue;
            Timestamp = timestamp;
        }

        public double Value { get; set; }
        public String HexValue { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
