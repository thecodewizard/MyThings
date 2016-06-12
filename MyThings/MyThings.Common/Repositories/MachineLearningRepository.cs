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

        public static void CalculateTimeToLive(Container container)
        {
            
        }

        #region Helpers
        public static double ParseAverageInTime(Container container, DateTime startTime, TimeSpan interval)
        {
            //Calculate the timespan for the tablestoragerepository
            TimeSpan timeToStartDate = DateTime.Now.Subtract(startTime);
            DateTime endDate = startTime.Add(interval);

            //Update the history for the container
            container = TableStorageRepository.GetHistory(container, timeToStartDate);

            //Filter the values for our interval
            List<ContainerValue> valuesInInterval =
                (from h in container.History where h.Timestamp > startTime && h.Timestamp <= endDate select h).ToList();

            //Parse the average from these values
            double totalValue = 0;
            int count = 0;
            foreach (ContainerValue value in valuesInInterval)
            {
                totalValue += value.Value;
                count++;
            }

            //Give the average
            return totalValue/count;
        }
        #endregion

        #endregion
    }
}
