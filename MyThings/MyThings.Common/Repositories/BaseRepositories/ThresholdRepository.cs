using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;

namespace MyThings.Common.Repositories
{
    public class ThresholdRepository : GenericRepository<Threshold>
    {
        #region Functionality Methods

        private Threshold GetThreshold(Container container)
        {
            //Get the threshold
            Threshold threshold = new Threshold();
            if (container == null) return null;
            if (container.Threshold == null)
            {
                if (container.ThresholdId == null || container.ThresholdId == 0)
                {
                    Insert(threshold);
                    SaveChanges();
                } else
                {
                    threshold = GetByID(container.ThresholdId);
                }
            } else
            {
                threshold = container.Threshold;
            }
            return threshold;
        }

        public void SetBetweenValueThreshold(Container container, double minValue, double maxValue, bool setActive = true)
        {
            Threshold threshold = GetThreshold(container);
            if (threshold == null) return;
            threshold.MinValue = minValue;
            threshold.MaxValue = maxValue;
            threshold.BetweenValuesActive = setActive;
            Update(threshold);
            SaveChanges();
        }

        public void SetExactValueThreshold(Container container, String exactValue, bool setActive = true)
        {
            Threshold threshold = GetThreshold(container);
            if (threshold == null) return;
            threshold.MatchValue = exactValue;
            threshold.MatchValueActive = setActive;
            Update(threshold);
            SaveChanges();
        }

        public void SetIntervalThreshold(Container container, TimeSpan interval, bool setActive = true)
        {
            Threshold threshold = GetThreshold(container);
            if (threshold == null) return;
            threshold.MinUpdateInterval = interval;
            threshold.FrequencyActive = setActive;
            Update(threshold);
            SaveChanges();
        }

        public bool VerifyAllThresholds(Sensor sensor)
        {
            if (sensor == null) return false;
            if (sensor.Containers == null)
            {
                sensor = new SensorRepository().GetSensorById(sensor.Id);
            }

            foreach (Container container in sensor.Containers)
            {
                if (VerifyThresholds(container) != ThresholdVerifications.PASSED)
                    return false;
            }

            return true;
        }

        public ThresholdVerifications VerifyThresholds(Container container)
        {
            //Make sure we have all the data
            if (container == null) return ThresholdVerifications.NULL;
            if (container.Threshold == null)
            {
                container = new ContainerRepository().GetContainerById(container.Id);
            }

            //Set variables
            container = TableStorageRepository.UpdateValue(container);
            Threshold threshold = container.Threshold;
            if(threshold == null) return ThresholdVerifications.NULL;

            //Do the check
            if (threshold.BetweenValuesActive)
            {
                if(threshold.MinValue <= container.CurrentValue.Value &&
                        threshold.MaxValue >= container.CurrentValue.Value)
                    return ThresholdVerifications.BetweenValueMismatch;
            }

            if (threshold.MatchValueActive)
            {
                if(threshold.MatchValue.Equals(container.CurrentValue.ToString()) ||
                        threshold.MatchValue.Equals(container.CurrentValue.HexValue))
                    return ThresholdVerifications.ExactValueMismatch;
            }

            if (threshold.FrequencyActive)
            {
                TimeSpan lastUpdate = DateTime.Now.Subtract(container.CurrentValue.Timestamp);
                if(threshold.MinUpdateInterval >= lastUpdate)
                    return ThresholdVerifications.FrequencyMismatch;
            }

            return ThresholdVerifications.PASSED;
        }

        #endregion

        public enum ThresholdVerifications
        {
            PASSED, NULL, BetweenValueMismatch, ExactValueMismatch, FrequencyMismatch
        }
    }
}
