using System;
using System.ComponentModel.DataAnnotations.Schema;
using MyThings.Common.Repositories;

namespace MyThings.Common.Models
{
    public class Pin
    {
        //Fields
        public int Id { get; set; }

        //Reference Fields
        public String UserId { get; set; }
        public int TileId { get; set; }

        //Saved Fields
        public int SavedId { get; set; }
        public PinType SavedType { get; set; }
    }

    public enum PinType
    {
        Container, Sensor, Error, Group, FixedClock, FixedNavigation, FixedError
    }
}
