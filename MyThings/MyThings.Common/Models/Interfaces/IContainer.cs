using System;

namespace MyThings.Common.Models
{
    interface IContainer
    {
        ContainerType ContainerType { get; set; }
        int ContainerTypeId { get; set; }
        DateTime CreationTime { get; set; }
        int Id { get; set; }
        DateTime LastUpdatedTime { get; set; }
        string Name { get; set; }
        Sensor Sensor { get; set; }
        int? SensorId { get; set; }
        double Value { get; set; }
        DateTime ValueTime { get; set; }
    }
}