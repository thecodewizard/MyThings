﻿using System;
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

        #region GetMultiObject Methods

        [HttpGet]
        public HttpResponseMessage GetSensors(int? count = null)
        {
            if (count.HasValue)
            {
                List<Sensor> sensors = _sensorRepository.GetSensors(count);

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

        //[HttpGet]
        //public HttpResponseMessage GroupHasSensor(int? groupId, int? sensorId)
        //{
        //    //TODO: via GroupRepo -> SensorInGroup()
        //}

        #endregion
    }
}