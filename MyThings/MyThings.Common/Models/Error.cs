using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThings.Common.Models
{
    public class Error
    {
        //Fixed Fields
        [Key]
        public int ErrorCode { get; set; } //1xx == ERROR //2xx == WARNING
        public ErrorType Type { get; set; }
        public ErrorCategory Category { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public String Advice { get; set; }
        public DateTime Time { get; set; }

        //User Triggerd Fields
        public Boolean Read { get; set; }

        //References
        public int? SensorId { get; set; }
        public Sensor Sensor { get; set; }
        public int? ContainerId { get; set; }
        public Container Container { get; set; }

        //Constructor
        public Error(int id, ErrorType type, ErrorCategory category, string title, string description, string advice, Sensor sensor, Container container)
        {
            //Pass Through
            ErrorCode = id;
            Type = type;
            Category = category;
            Title = title;
            Description = description;
            Advice = advice;
            Sensor = sensor;
            Container = container;

            //Calculated
            Read = false;
            SensorId = sensor.Id;
            ContainerId = container.Id;
            Time = DateTime.Now;
        }

        //Creators
        public static Error MinThresholdWarning(Sensor sensor, Container container)
        {
            return new Error(201, ErrorType.Warning, ErrorCategory.Threshold, "Minimum Threshold Not Met", 
                "The minimum threshold is not met on Sensor " + sensor.Name + " at " + container.Name + ".",
                "Check on-site what would cause this message or raise the minimum threshold value at the container.",
                sensor, container);
        }

        public static Error MaxThresholdWarning(Sensor sensor, Container container)
        {
            return new Error(202, ErrorType.Warning, ErrorCategory.Threshold, "Maximum Threshold Exceeded",
                "The maximum threshold was exceeded on Sensor " + sensor.Name + " at " + container.Name + ".",
                "Check on-site what would cause this message or lower the maximum threshold value at the container.",
                sensor, container);
        }

        public static Error NetworkConnectivityError(Sensor sensor)
        {
            return new Error(101, ErrorType.Error, ErrorCategory.Connectivity, "Network Connectivity Issues",
                "We did not receive any network data from sensor " + sensor.Name + " for a long time.",
                "Check on-site what would cause this message, try rebooting the sensor or check the last known battery status at the sensor detail page.",
                sensor, null);
        }

        public static Error BatteryCriticalError(Sensor sensor, Container container)
        {
            return new Error(102, ErrorType.Error, ErrorCategory.Power, "Battery Power Critical",
                "The battery level on sensor " + sensor.Name + " is at " + container.Value + "%!",
                "Change or charge the battery at this sensor. Not doing this could evoke a NetworkConnectivityError in the near future",
                sensor, container);
        }

        public static Error BatteryWarning(Sensor sensor, Container container)
        {
            return new Error(203, ErrorType.Warning, ErrorCategory.Power, "Battery Power Low",
                "The battery level on sensor " + sensor.Name + " is at " + container.Value + "%.",
                "Change or charge the battery at this sensor.",
                sensor, container);
        }

        public static Error InactiveWarning(Sensor sensor, Container container)
        {
            return new Error(204, ErrorType.Warning, ErrorCategory.Connectivity, "Inactive Value Detected",
                "Sensor " + sensor.Name + " has send networkdata but is inactive for the " + container.Name + " value",
                "Check for a malfunctioning " + container.Name + " module, or verify whether it is normal for this value to be idle.",
                sensor, container);
        }

        public static Error GenericError(Sensor sensor, Container container)
        {
            return new Error(199, ErrorType.Error, ErrorCategory.Generic, "Error Detected",
            "An error occured on Sensor " + sensor.Name + " at " + container.Name + "!",
            "Check on-site what would cause this message and reboot the sensor. If this does not resolve the issue, contact our supportTeam.",
            sensor, container);
        }

        public static Error GenericWarning(Sensor sensor, Container container)
        {
            return new Error(299, ErrorType.Warning, ErrorCategory.Generic, "Warning Triggered",
                "A warning was triggered on Sensor " + sensor.Name + " at " + container.Name + "!",
                "Check on-site what would cause this message and reboot the sensor. If this does not resolve the issue, contact our supportTeam.",
                sensor, container);
        }
    }

    public enum ErrorCategory
    {
        Threshold, Connectivity, Power, Generic
    }

    public enum ErrorType
    {
        Warning, Error
    }
}
