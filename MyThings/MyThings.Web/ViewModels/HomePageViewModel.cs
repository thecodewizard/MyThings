using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyThings.Common.Models;

namespace MyThings.Web.ViewModels
{
    public class HomePageViewModel
    {
        //Gridster Json
        public String OriginalGridsterJson { get; set; }
        public String FilteredGridsterJson { get; set; }

        //Pinned Tile Lookup
        public List<Sensor> PinnedSensors { get; set; }
        public List<Container> PinnedContainers { get; set; }
        public List<Group> PinnedGroups { get; set; }
        public List<Error> PinnedErrors { get; set; }

        //Errors & Warnings
        public List<Error> Errors { get; set; }
    }
}