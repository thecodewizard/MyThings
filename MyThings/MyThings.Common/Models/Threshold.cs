using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models
{
    public class Threshold
    {
        public int Id { get; set; }

        //Between Values -> The container payload must be between these values
        public bool BetweenValuesActive { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        //Exact Value -> The container value must match this value
        public bool MatchValueActive { get; set; }
        public String MatchValue { get; set; }

        //Frequency -> The container must update within the given interval
        public bool FrequencyActive { get; set; }
        public TimeSpan MinUpdateInterval { get; set; }

        public Threshold()
        {
            BetweenValuesActive = false;
            MatchValueActive = false;
            FrequencyActive = false;
        }
    }
}
