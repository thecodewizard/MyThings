using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proximus_API.Models
{
    public class DecodedData
    {
        [Required]
        public string company { get; set; }
        [Required]
        public string macaddress { get; set; }
        [Required]
        public string container { get; set; }
        [Required]
        public string locationid { get; set; }
        [Required]
        public string payload { get; set; }
        [Required]
        public string timestamp { get; set; }
    }
}
