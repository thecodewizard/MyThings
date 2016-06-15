using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;
using MyThings.Common.Repositories;

namespace MyThings.Common.Helpers
{
    public class SuggestionListHelper
    {
        private static readonly SensorRepository _sensorRepository = new SensorRepository();
        private static readonly GroupRepository _groupRepository = new GroupRepository();
        private static readonly ErrorRepository _errorRepository = new ErrorRepository();

        public static List<String> GetSuggestionList(bool includeSensorName = true, bool includeSensorMac = true, bool includeSensorLocation = true,
            bool includeContainerName = true, bool includeContainerTypeName = true, bool includeGroupName = true, bool includeErrorTitle = false,
            bool includeErrorType = false, bool includeErrorDescription = false, bool includeErrorAdvice = false)
        {
            List<Sensor> sensors = _sensorRepository.GetSensors();
            List<Group> groups = _groupRepository.GetGroups();
            List<Error> errors = _errorRepository.GetErrors();

            List<String> suggestionList = new List<String>();
            foreach (Sensor sensor in sensors)
            {
                if (includeSensorName && !String.IsNullOrWhiteSpace(sensor.Name) && !suggestionList.Contains(sensor.Name))
                    suggestionList.Add(sensor.Name);

                if (includeSensorMac && !String.IsNullOrWhiteSpace(sensor.MACAddress) && !suggestionList.Contains(sensor.MACAddress))
                    suggestionList.Add(sensor.MACAddress);

                if (includeSensorLocation && !String.IsNullOrWhiteSpace(sensor.Location) && !suggestionList.Contains(sensor.Location))
                    suggestionList.Add(sensor.Location);

                foreach (Container container in sensor.Containers)
                {
                    if (includeContainerName && !String.IsNullOrWhiteSpace(container.Name) && !suggestionList.Contains(container.Name))
                        suggestionList.Add(container.Name);

                    if (includeContainerTypeName && !String.IsNullOrWhiteSpace(container.ContainerType.Name) && !suggestionList.Contains(container.ContainerType.Name))
                        suggestionList.Add(container.ContainerType.Name);
                }

                foreach (Error error in errors)
                {
                    if (includeErrorTitle && !String.IsNullOrWhiteSpace(error.Title) && !suggestionList.Contains(error.Title))
                        suggestionList.Add(error.Title);

                    if (includeErrorType && !String.IsNullOrWhiteSpace(error.Type.ToString()) && !suggestionList.Contains(error.Type.ToString()))
                        suggestionList.Add(error.Type.ToString());

                    if (includeErrorDescription && !String.IsNullOrWhiteSpace(error.Description) && !suggestionList.Contains(error.Description))
                        suggestionList.Add(error.Description);

                    if (includeErrorAdvice && !String.IsNullOrWhiteSpace(error.Advice) && !suggestionList.Contains(error.Advice))
                        suggestionList.Add(error.Advice);
                }
            }

            if (includeGroupName)
            {
                foreach(Group group in groups)
                    if(!String.IsNullOrWhiteSpace(group.Name) && !suggestionList.Contains(group.Name))
                        suggestionList.Add(group.Name);
            }

            return suggestionList;
        }

        public static List<Error> GetErrorsFromSuggestion(String query)
        {
            List<Error> errors = _errorRepository.GetErrors();
            List<Sensor> sensors = GetSensorsFromSuggestion(query, true, errors);

            return (from e in errors where e.SensorId.HasValue && (from s in sensors select s.Id).Contains(e.SensorId.Value) select e).ToList();
        }

        public static List<Sensor> GetSensorsFromSuggestion(String query)
        {
            return GetSensorsFromSuggestion(query, false, null);
        }

        public static List<Sensor> GetSensorsFromSuggestion(String query, bool includeErrors, List<Error> errors)
        {
            List<Sensor> sensors = _sensorRepository.GetSensors();
            List<Group> groups = _groupRepository.GetGroups();

            if (String.IsNullOrWhiteSpace(query)) return sensors; //When nothing is queried, return everything.

            query = query.Trim();
            if (query.StartsWith("\"") && query.EndsWith("\"")) return GetSensorsFromQueryPart(sensors, groups, query.Replace("\"", ""), includeErrors, errors);
            else
            {
                List<Sensor> filteredSensors = new List<Sensor>();
                String[] queryParts = query.Split(' ');
                foreach (String queryPart in queryParts)
                {
                    foreach (Sensor sensor in GetSensorsFromQueryPart(sensors, groups, queryPart, includeErrors, errors))
                    {
                        if(!(from s in filteredSensors select s.Id).ToList().Contains(sensor.Id)) filteredSensors.Add(sensor);
                    }
                }
                if(filteredSensors.Any()) return filteredSensors;
            }

            return GetSensorsFromQueryPart(sensors, groups, query, includeErrors, errors);
        }

        private static List<Sensor> GetSensorsFromQueryPart(List<Sensor> sensors, List<Group> groups, String query, bool includeErrors, List<Error> errors)
        {
            List<Sensor> filteredSensors =
                    (from s in sensors
                     where
                         s.Name.ToLower().Contains(query) || s.Location.ToLower().Contains(query) || s.MACAddress.ToLower().Contains(query) ||
                         (from c in s.Containers
                          where c.Name.ToLower().Contains(query) || c.ContainerType.Name.ToLower().Contains(query)
                          select c.SensorId).Contains(s.Id)
                     select s).ToList();

            foreach (Group group in groups)
                if (group.Name.ToLower().Contains(query))
                    foreach (Sensor sensor in group.Sensors)
                        if (!(from s in filteredSensors select s.Id).ToList<int>().Contains(sensor.Id))
                            filteredSensors.Add(sensor);

            if (includeErrors)
            {
                List<Error> filteredErrors = (from e in errors
                    where
                        e.Title.ToLower().Contains(query) || e.Type.ToString().ToLower().Contains(query) ||
                        e.Description.ToLower().Contains(query) || e.Advice.ToLower().Contains(query)
                    select e).ToList();

                foreach(Error error in filteredErrors)
                    if (error.SensorId.HasValue && !(from s in filteredSensors select s.Id).ToList().Contains(error.SensorId.Value))
                        filteredSensors.Add(error.Sensor);
            }

            return filteredSensors;
        }
    }
}
