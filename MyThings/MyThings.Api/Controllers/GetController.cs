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

        // TODO: Methods to create to backup Javascript's functionality:
        // Load Sensor details on ID,
        // Load Container details on ID
        // Load Group details on ID
        // Get last value from container on ID. (return json with value and valueid)

        //TODO: Delete The Testmethods
        #region TestMethods javascript Objects
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
                container.CurrentValue = new ContainerValue(13, DateTime.Now);

                String json = JsonConvert.SerializeObject(container);

                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StringContent(json);
                message.Headers.Add("Access-Control-Allow-Origin", "*");
                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
        #endregion

        #region TestMethods ErrorHandling
        [HttpGet]
        public HttpResponseMessage GetRandomError()
        {
            Sensor sensor = new Sensor();
            sensor.Name = "test";
            sensor.Containers = new List<Container>()
                {
                    new Container() { Id= 1, Name = "TestContainer", SensorId =  sensor.Id},
                    new Container() { Id= 2, Name = "TestContainer2", SensorId =  sensor.Id},
                    new Container() { Id= 3, Name = "TestContainer3", SensorId =  sensor.Id}
                };

            Error randomError = Error.GenericError(sensor, sensor.Containers.FirstOrDefault());

            String json = JsonConvert.SerializeObject(randomError);

            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            message.Headers.Add("Access-Control-Allow-Origin", "*");
            return message;
        }
        #endregion

        #region TestMethods SQL Database

        [HttpGet]
        public HttpResponseMessage GetSensorsDb()
        {
            SensorRepository sensorRepository = new SensorRepository();
            List<Sensor> sensors = sensorRepository.GetSensors();

            //Send back
            String json = JsonConvert.SerializeObject(sensors);
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            message.Headers.Add("Access-Control-Allow-Origin", "*");
            return message;
        }

        [HttpGet]
        public HttpResponseMessage GetSensorsFromDb()
        {
            SensorRepository sensorRepository = new SensorRepository();
            ErrorRepository errorRepository = new ErrorRepository();
            List<Sensor> sensors = sensorRepository.GetSensors(); //TEST: 'GetSensors'

            //TODO: TEST: 'GetSensorById'

            //TOO SLOW - DELETED
            ////TEST1: 'Stress Test - SaveOrUpdateSensor'
            //foreach (Sensor sensor in sensors)
            //{
            //    sensorRepository.SaveOrUpdateSensor(sensor);
            //}

            //TEST2: 'Stress Test - UpdateSensor'
            foreach (Sensor sensor in sensors)
            {
                sensorRepository.Update(sensor);
            }
            sensorRepository.SaveChanges();

            //Generate Errors
            if (errorRepository.GetErrors().Count == 0) //Test: 'GetErrors'
            {
                foreach (Sensor sensor in sensors)
                {
                    Container container = sensor.Containers.FirstOrDefault();
                    if (container != null)
                    {
                        Error error = Error.BatteryCriticalError(sensor, container);
                        errorRepository.Insert(error); //TEST: 'SaveOrUpdateError' - Insert
                        errorRepository.SaveChanges();

                        Error fetchedError = errorRepository.GetByID(error.Id); //TEST: 'GetErrorById'
                        error.Time = DateTime.Now;
                        errorRepository.Update(error); //TEST: 'SaveOrUpdateError' - Update
                        errorRepository.SaveChanges();
                    }
                }
            }

            //Send back
            String json = JsonConvert.SerializeObject(sensors);
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            message.Headers.Add("Access-Control-Allow-Origin", "*");
            return message;
        }

        [HttpGet]
        public HttpResponseMessage GetErrors()
        {
            ErrorRepository errorRepository = new ErrorRepository();
            List<Error> errors = errorRepository.GetErrors(); //TEST: 'GetErrors'

            //TEST: 'SaveOrUpdateError' - Stress test update
            foreach (Error error in errors)
                errorRepository.Update(error);
            errorRepository.SaveChanges();

            //Send back
            String json = JsonConvert.SerializeObject(errors);
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            message.Headers.Add("Access-Control-Allow-Origin", "*");
            return message;
        }

        [HttpGet]
        public HttpResponseMessage GetGroups()
        {
            GroupRepository groupRepository = new GroupRepository();
            List<Group> groups = groupRepository.GetGroups();   //TEST: 'GetGroups'

            //Send back
            String json = JsonConvert.SerializeObject(groups);
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            message.Headers.Add("Access-Control-Allow-Origin", "*");
            return message;
        }

        [HttpGet]
        public HttpResponseMessage SaveSensorsInGroup()
        {
            SensorRepository sensorRepository = new SensorRepository();
            GroupRepository groupRepository = new GroupRepository();

            Group group = new Group();
            group.Name = "Oost-Vlaanderen";
            group = groupRepository.Insert(group); //TEST: 'SaveOrUpdateGroup' - Insert
            groupRepository.SaveChanges();

            group = groupRepository.GetByID(group.Id);
            Sensor sensor = sensorRepository.GetSensors().FirstOrDefault();
            group.Sensors.Add(sensor);
            groupRepository.Update(group); //TEST: 'SaveOrUpdateGroup' - Update
            groupRepository.SaveChanges();
            
            //Send back
            String json = JsonConvert.SerializeObject(group);
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            message.Headers.Add("Access-Control-Allow-Origin", "*");
            return message;
        }

        [HttpGet]
        public HttpResponseMessage MakeRandomData()
        {
            SensorRepository sensorRepository = new SensorRepository();
            ContainerRepository containerRepository = new ContainerRepository();
            ContainerTypeRepository containerTypeRepository = new ContainerTypeRepository();

            Sensor sensor = new Sensor();
            sensor.Name = "Sensor 7";
            sensor.Company = "Proximus";
            sensor.MACAddress = "11:22:33:44:AA:BB";
            sensor.Location = "Brussel";
            sensor.CreationDate = DateTime.Now;
            sensor.SensorEntries = 3;
            sensor.BasestationLat = 100.2;
            sensor.BasestationLng = 53.36;
            sensor = sensorRepository.Insert(sensor); //TEST: 'SaveOrUpdateSensor' - insert
            sensorRepository.SaveChanges();

            ContainerType batteryType = new ContainerType() {Name = "Battery"};
            ContainerType humidityType = new ContainerType() { Name = "Humidity" };
            batteryType = containerTypeRepository.SaveOrUpdateContainerType(batteryType); //TEST: 'SaveOrUpdateContainerType'
            humidityType = containerTypeRepository.SaveOrUpdateContainerType(humidityType);
                
            Container batteryContainer = new Container();
            batteryContainer.Name = "BatteryContainer";
            batteryContainer.CreationTime = DateTime.Now;
            batteryContainer.ContainerType = batteryType;
            batteryContainer.SensorId = sensor.Id;
            batteryContainer = containerRepository.Insert(batteryContainer);

            Container humidityContainer = new Container();
            humidityContainer.Name = "HumidityContainer";
            humidityContainer.CreationTime = DateTime.Now;
            humidityContainer.ContainerType = humidityType;
            humidityContainer.SensorId = sensor.Id;
            humidityContainer = containerRepository.Insert(humidityContainer);
            containerRepository.SaveChanges();

            sensor.Containers = new List<Container>()
            {
                batteryContainer, humidityContainer
            };
            sensorRepository.Update(sensor); //TEST: 'SaveOrUpdateSensor' - update
            sensorRepository.SaveChanges();

            //Send back
            String json = JsonConvert.SerializeObject(sensor);
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(json);
            message.Headers.Add("Access-Control-Allow-Origin", "*");
            return message;
        }

        #endregion
        
    }
}