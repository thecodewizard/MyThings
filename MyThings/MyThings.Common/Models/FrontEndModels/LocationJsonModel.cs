using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models.FrontEndModels
{
    public class LocationJsonModel
    {
        //Accurancy Fields
        public int Accuracy { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }

        //Average Fields
        public double averageCount { get; set; }
        public double averageLat { get; set; }
        public double averageLong { get; set; }
    }
}
