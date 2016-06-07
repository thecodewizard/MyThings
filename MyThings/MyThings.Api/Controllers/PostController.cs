using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using MyThings.Common.Models;
using MyThings.Common.Repositories;

namespace MyThings.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class PostController : ApiController
    {
        //This controller will manage all the API's POST requests.

        //Initialise the needed Repositories
        private readonly PinRepository _pinRepository = new PinRepository();

        #region Pin Objects Methods

        [HttpPost]
        public HttpResponseMessage PinSensor(int? sensorId = null)
        {
            if (sensorId.HasValue)
            {
                if (!_pinRepository.IsSensorPinned(sensorId.Value))
                {
                    Pin pin = new Pin();
                    pin.SavedId = sensorId.Value;
                    pin.SavedType = PinType.Sensor;
                    pin.Save();

                    return new HttpResponseMessage(HttpStatusCode.OK);
                } 
                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public HttpResponseMessage PinContainer(int? containerId = null)
        {
            //TODO: Same as PinSensor
        }

        [HttpPost]
        public HttpResponseMessage PinGroup(int? groupId = null)
        {
            //TODO: Same as PinSensor
        }

        [HttpPost]
        public HttpResponseMessage PinError(int? errorId = null)
        {
            //TODO: Same as PinError
        }
        #endregion

        #region Unpin Objects Methods

        [HttpPost]
        public HttpResponseMessage UnpinSensor(int? sensorId = null)
        {
            if (sensorId.HasValue)
            {
                if (_pinRepository.IsSensorPinned(sensorId.Value))
                {
                    int pinId = _pinRepository.GetPinId(sensorId.Value, PinType.Sensor);
                    Pin pin = _pinRepository.GetPinById(pinId);
                    pin.Delete();

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public HttpResponseMessage UnpinContainer(int? containerId = null)
        {
            //TODO: Same as UnpinSensor
        }

        [HttpPost]
        public HttpResponseMessage UnpinGroup(int? groupId = null)
        {
            //TODO: Same as UnpinGroup
        }

        [HttpPost]
        public HttpResponseMessage UnpinError(int? errorId = null)
        {
            //TODO: Same as UnpinError
        }
        #endregion

        #region Group Management Methods

        //[HttpPost]
        //public HttpResponseMessage SaveGroup([FromBody] Group group)
        //{
        //    //Insert & Update in 1 method
        //}

        //[HttpPost]
        //public HttpResponseMessage AddSensor(int? groupId, int? sensorId)
        //{
            
        //}

        //[HttpPost]
        //public HttpResponseMessage RemoveSensor(int? groupId, int? sensorId)
        //{
            
        //}

        #endregion
    }
}