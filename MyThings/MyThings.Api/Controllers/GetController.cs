using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using MyThings.Common.Models;
using MyThings.Common.Repositories;
using Newtonsoft.Json;

namespace MyThings.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    public class GetController : ApiController
    {
        //This controller will manage all the API's GET requests.

        //Initialise the needed Repositories
        private readonly SensorRepository _sensorRepository = new SensorRepository();
        private readonly ContainerRepository _containerRepository = new ContainerRepository();
        private readonly GroupRepository _groupRepository = new GroupRepository();
        private readonly ErrorRepository _errorRepository = new ErrorRepository();

        #region GetSingleObject Methods
        [HttpGet]
        public HttpResponseMessage GetSensor(int? sensorId = null)
        {
            if (sensorId.HasValue)
            {
                //Fetch the sensor
                int id = sensorId.Value;
                Sensor sensor = _sensorRepository.GetSensorById(id);

                if (sensor != null)
                {
                    String json = JsonConvert.SerializeObject(sensor);

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StringContent(json);
                    message.Headers.Add("Access-Control-Allow-Origin", "*");
                    return message;
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        public HttpResponseMessage GetContainer(int? containerId = null)
        {
            if (containerId.HasValue)
            {
                //Fetch the container
                int id = containerId.Value;
                Container container = _containerRepository.GetContainerById(id);

                if (container != null)
                {
                    String json = JsonConvert.SerializeObject(container);

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StringContent(json);
                    message.Headers.Add("Access-Control-Allow-Origin", "*");
                    return message;
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        public HttpResponseMessage GetGroup(int? groupId = null)
        {
            if (groupId.HasValue)
            {
                //Fetch the Group
                int id = groupId.Value;
                Group group = _groupRepository.GetGroupById(id);

                if (group != null)
                {
                    String json = JsonConvert.SerializeObject(group);

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StringContent(json);
                    message.Headers.Add("Access-Control-Allow-Origin", "*");
                    return message;
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        public HttpResponseMessage GetError(int? errorId = null)
        {
            if (errorId.HasValue)
            {
                //Fetch the Error
                int id = errorId.Value;
                Error error = _errorRepository.GetErrorById(id);

                if (error != null)
                {
                    String json = JsonConvert.SerializeObject(error);

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StringContent(json);
                    message.Headers.Add("Access-Control-Allow-Origin", "*");
                    return message;
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
        #endregion

        #region SearchQuery Methods

        [HttpGet]
        public HttpResponseMessage GetSensorsOnQuery(String query)
        {
            List<Sensor> sensors = _sensorRepository.GetSensors();
            List<Group> groups = _groupRepository.GetGroups();
            String json = String.Empty;
            query = query.ToLower();

            if (String.IsNullOrWhiteSpace(query))
            {
                json = JsonConvert.SerializeObject(sensors);
            }
            else
            {
                List<Sensor> filteredSensors =
                    (from s in sensors
                        where
                            s.Name.ToLower().Contains(query) || s.Location.ToLower().Contains(query) || s.MACAddress.ToLower().Contains(query) ||
                            (from c in s.Containers
                                where !c.Name.ToLower().Contains(query) && c.ContainerType.Name.ToLower().Contains(query)
                                select c.SensorId).Contains(s.Id)
                        select s).ToList();

                foreach (Group group in groups)
                {
                    if (group.Name.ToLower().Contains(query))
                    {
                        foreach (Sensor sensor in group.Sensors)
                        {
                            if (!(from s in filteredSensors select s.Id).ToList<int>().Contains(sensor.Id))
                            {
                                filteredSensors.Add(sensor);
                            }
                        }
                    }
                }

                json = JsonConvert.SerializeObject(filteredSensors);
            }

            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            message.Headers.Add("Access-Control-Allow-Origin", "*");
            return message;
        }

        #endregion

        #region GetMultiObject Methods

        [HttpGet]
        public HttpResponseMessage GetSensors(int? count = null, bool includeVirtual = false)
        {
            if (count.HasValue)
            {
                List<Sensor> sensors = _sensorRepository.GetSensors(count, includeVirtual);

                if (sensors != null && sensors.Count > 0)
                {
                    String json = JsonConvert.SerializeObject(sensors);

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StringContent(json);
                    message.Headers.Add("Access-Control-Allow-Origin", "*");
                    return message;
                } 
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        public HttpResponseMessage GetErrors(int? count = null)
        {
            if (count.HasValue)
            {
                List<Error> errors = _errorRepository.GetErrors(count);

                if (errors != null && errors.Count > 0)
                {
                    String json = JsonConvert.SerializeObject(errors);

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StringContent(json);
                    message.Headers.Add("Access-Control-Allow-Origin", "*");
                    return message;
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        #endregion

        #region GetNoSqlValue Methods

        [HttpGet]
        public HttpResponseMessage GetMostRecentValue(int? containerId = null)
        {
            if (containerId.HasValue)
            {
                int id = containerId.Value;
                Container container = _containerRepository.GetByID(id);
                if (container != null)
                {
                    if(container.MACAddress == null || container.ContainerType == null)
                        return new HttpResponseMessage(HttpStatusCode.BadRequest);

                    //Update the values of the container
                    container = TableStorageRepository.UpdateValue(container);

                    //Return the json of the updated container
                    String json = JsonConvert.SerializeObject(container);

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StringContent(json);
                    message.Headers.Add("Access-Control-Allow-Origin", "*");
                    return message;
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        public HttpResponseMessage GetHistory(int? containerId = null, int? historyTimeInHours = null)
        {
            if (containerId.HasValue && historyTimeInHours.HasValue)
            {
                int id = containerId.Value;
                int timeInHours = historyTimeInHours.Value;
                Container container = _containerRepository.GetByID(id);
                if (container != null)
                {
                    if (container.MACAddress == null || container.ContainerType == null)
                        return new HttpResponseMessage(HttpStatusCode.BadRequest);

                    //Update the values of the container
                    TimeSpan span = TimeSpan.FromHours(timeInHours);
                    container = TableStorageRepository.GetHistory(container, span);

                    //Return the json of the updated container
                    String json = JsonConvert.SerializeObject(container);

                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StringContent(json);
                    message.Headers.Add("Access-Control-Allow-Origin", "*");
                    return message;
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Group Management Methods


        [HttpGet]
        public HttpResponseMessage GroupHasSensor(int? groupId = null, int? sensorId = null)
        {
            if (groupId.HasValue && sensorId.HasValue)
            {
                int groupID = groupId.Value;
                int sensorID = sensorId.Value;
                Group group = _groupRepository.GetGroupById(groupID);
                Sensor sensor = _sensorRepository.GetSensorById(sensorID);
                if (group != null && sensor != null)
                {
                    bool sensorInGroup = _groupRepository.SensorInGroup(groupID, sensorID);
                    if (sensorInGroup)
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    else
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        #endregion
    }
}