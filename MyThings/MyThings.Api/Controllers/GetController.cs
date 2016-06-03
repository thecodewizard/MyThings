using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using MyThings.Common.Models;
using Newtonsoft.Json;

namespace MyThings.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    public class GetController : ApiController
    {
        //This controller will manage all the API's GET requests.

        // TODO: Methods to create to backup Javascript's functionality:
        // Load Sensor details on ID,
        // Load Container details on ID
        // Load Group details on ID
        // Get last value from container on ID. (return json with value and valueid)
        
        //TODO: Delete The Testmethods
        [HttpGet]
        public HttpResponseMessage GetSensor(int? sensorId)
        {
            if (sensorId.HasValue)
            {
                int id = sensorId.Value;
                Sensor sensor = new Sensor();
                sensor.Name = "test";
                sensor.Containers = new List<Container>()
                {
                    new Container() { Id= 1, Name = "TestContainer", SensorId =  sensor.Id},
                    new Container() { Id= 2, Name = "TestContainer2", SensorId =  sensor.Id},
                    new Container() { Id= 3, Name = "TestContainer3", SensorId =  sensor.Id}
                };

                String json = JsonConvert.SerializeObject(sensor);

                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StringContent(json);
                message.Headers.Add("Access-Control-Allow-Origin", "*");
                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        public HttpResponseMessage GetSensors(int? count)
        {
            if (count.HasValue)
            {
                List<Sensor> sensors = new List<Sensor>();
                sensors.Add(new Sensor() { Name = "test1"});
                sensors.Add(new Sensor() { Name = "test2"});
                sensors.Add(new Sensor() { Name = "test3"});
                sensors.Add(new Sensor() { Name = "test4"});

                String json = JsonConvert.SerializeObject(sensors);

                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StringContent(json);
                message.Headers.Add("Access-Control-Allow-Origin", "*");
                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        public HttpResponseMessage GetValue(int? sensorId, int? containerId)
        {
            if (sensorId.HasValue && containerId.HasValue)
            {
                Container container = new Container();
                container.Name = "testContainer69";
                container.Value = 13;
                container.ValueTime = DateTime.Now;

                String json = JsonConvert.SerializeObject(container);

                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StringContent(json);
                message.Headers.Add("Access-Control-Allow-Origin", "*");
                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }
}