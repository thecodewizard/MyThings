using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;

namespace MyThings.Common.Repositories
{
    public class MachineLearningRepository
    {
        #region Own Algorithms

        public static TimeSpan CalculateTimeToLive(Container container, double payload)
        {
            //Get all the container values from last year
            if (container.History == null)
            {
                container = TableStorageRepository.GetHistory(container, TimeSpan.FromDays(365));
            }

            //Calculate when the container would be empty
                //Get the average downgrade per update
            int count = container.History.Count;
            double totalDelta = 0;
            //totalDelta = container.History.Last().Value - container.History.First().Value;

            double previousValue = container.History.Last().Value;
            List<ContainerValue> invertedHistory = container.History;
            invertedHistory.Reverse();
            foreach (ContainerValue value in invertedHistory)
            {
                double delta = previousValue - value.Value;
                if (delta > -10) totalDelta += delta; //Do not count recharges on battery
                else if (!container.ContainerType.Name.Equals("Battery level")) totalDelta += delta;
                previousValue = value.Value;
            }

            //Temp. Results
            double averageDelta = totalDelta / count;
            TimeSpan lifeTime = DateTime.Now - container.History.Last().Timestamp;
            long lifeTimeTicks = lifeTime.Ticks;
            long ticksPerUpdate = lifeTimeTicks / count;

            //Get prediction
            int updatesLeft = (int)Math.Floor((payload / averageDelta));
            long ticksToLive = ticksPerUpdate * updatesLeft;
            return TimeSpan.FromTicks(ticksToLive);
        }

        #region Helpers
        public static double? ParseAverageInTime(Container container, DateTime startTime, TimeSpan interval)
        {
            //Calculate the timespan for the tablestoragerepository
            TimeSpan timeToStartDate = DateTime.Now.Subtract(startTime);
            DateTime endDate = startTime.Add(interval);

            //Update the history for the container
            container = TableStorageRepository.GetHistory(container, timeToStartDate);

            //Filter the values for our interval
            List<ContainerValue> valuesInInterval =
                (from h in container.History where h.Timestamp > startTime && h.Timestamp <= endDate select h).ToList();

            if (valuesInInterval.Any())
            {
                //Parse the average from these values
                double totalValue = 0;
                int count = 0;
                foreach (ContainerValue value in valuesInInterval)
                {
                    totalValue += value.Value;
                    count++;
                }

                //Give the average
                if (count == 0) return null;
                return totalValue / count;
            }
            return null;
        }
        #endregion

        #endregion
    }
}
