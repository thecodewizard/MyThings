using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using MyThings.Common.Models;
using MyThings.Common.Models.FrontEndModels;
using MyThings.Common.Repositories;
using MyThings.Common.Repositories.RemoteRepositories;

namespace MyThings.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class PostController : ApiController
    {
        //This controller will manage all the API's POST requests.
        public SensorRepository _sensorRepository = new SensorRepository();

        public HttpResponseMessage UpdateLocation(LocationWebhookElement element)
        {
            if (element.DevEUI != null)
            {
                LocationApiRepository.UpdateSensorLocation(element.DevEUI).Wait();
            } else return new HttpResponseMessage(HttpStatusCode.BadRequest);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}