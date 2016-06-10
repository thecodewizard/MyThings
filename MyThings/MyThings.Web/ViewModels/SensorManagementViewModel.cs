using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyThings.Common.Models;

namespace MyThings.Web.ViewModels
{
    public class SensorManagementViewModel
    {
        public List<Sensor> Sensors { get; set; }
        public List<ContainerType> ContainerTypes { get; set; }
        public List<Pin> PinnedSensors { get; set; }
        public List<Group> Groups { get; set; }
        public int TotalSensors { get; set; }
        public List<String> AutoCompleteSuggestionList { get; set; }
    }
}