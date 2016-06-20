using MyThings.Common.Models;
using MyThings.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Data.OData.Query.SemanticAst;
using MyThings.Common.Helpers;
using MyThings.Common.Models.FrontEndModels;
using Newtonsoft.Json;

namespace MyThings.Web.Controllers
{
    //TODO: Delete this code
    public class TestController : Controller
    {
        private Random randomGenerator = new Random();

        //Define the usermanager
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        //Define the repositories
        private readonly SensorRepository _sensorRepository = new SensorRepository();
        private readonly ContainerRepository _containerRepository = new ContainerRepository();
        private readonly PinRepository _pinRepository = new PinRepository();
        private readonly ErrorRepository _errorRepository = new ErrorRepository();

        [HttpGet]
        public ActionResult Index()
        {
            Sensor s = new Sensor();
            s.MACAddress = "0E7E35430610005F";

            Container c = new Container();
            c.ContainerType = new ContainerType();
            c.ContainerType.Name = "Battery level";
            c.MACAddress = s.MACAddress;
            s.Containers = new List<Container>() {c};

            c =  TableStorageRepository.GetHistory(c, new TimeSpan(24, 0, 0));

            return View();
        }

        [HttpGet]
        public ActionResult GenerateFixedErrors(String message)
        {
            List<Sensor> sensors = _sensorRepository.GetSensors(null, true);
            Sensor sensor = sensors.First();
            Container container = sensor.Containers.First();

            Error error = Error.GenericError(sensor, container, message);
            _errorRepository.Insert(error);
            _errorRepository.SaveChanges();

            return View("PinEverything");
        }

        [HttpGet]
        public ActionResult DeleteAndRegenerateErrors(int count = 10)
        {
            List<Sensor> sensors = _sensorRepository.GetSensors(null, true);
            List<Error> errors = _errorRepository.GetErrors();

            List<int> sensorIds = (from s in sensors select s.Id).ToList();

            foreach (Error error in errors)
            {
                _errorRepository.Delete(error);
            }
            _errorRepository.SaveChanges();

            for (int i = 0; i < count; i++)
            {
                Error newError = null;
                int errorcode = randomGenerator.Next(0, 9);

                int randomSensorId = randomGenerator.Next(0, sensors.Count);
                Sensor sensor = _sensorRepository.GetSensorById(sensorIds[randomSensorId]);
                if (sensor != null && sensor.Containers != null)
                {
                    int randomContainerId = randomGenerator.Next(0, sensor.Containers.Count);
                    //Container container = _containerRepository.GetContainerById(containerIds[randomContainerId]);
                    Container container = sensor.Containers[randomContainerId];
                    if (container != null)
                    {
                        switch (errorcode)
                        {
                            case 0:
                                container = TableStorageRepository.UpdateValue(container);
                                newError = Error.BatteryCriticalError(sensor, container, MachineLearningRepository.CalculateTimeToLive(container, container.CurrentValue.Value));
                                break;
                            case 1:
                                container = TableStorageRepository.UpdateValue(container);
                                newError = Error.BatteryWarning(sensor, container,
                                    MachineLearningRepository.CalculateTimeToLive(container, container.CurrentValue.Value));
                                break;
                            case 2:
                                newError = Error.InactiveContainerWarning(sensor, container);
                                break;
                            case 3:
                                newError = Error.InactiveSensorWarning(sensor);
                                break;
                            case 4:
                                newError = Error.MaxThresholdWarning(sensor, container);
                                break;
                            case 5:
                                newError = Error.MinThresholdWarning(sensor, container);
                                break;
                            case 6:
                                newError = Error.NetworkConnectivityError(sensor);
                                break;
                            case 7:
                                newError = Error.GenericError(sensor, container, "Look outside. Is it raining? Maybe. Is it relevant to this error? Nope.");
                                break;
                            case 8:
                                newError = Error.GenericWarning(sensor, container,
                                    "Hallo, this is IT. Have you tried turning it off and on again?");
                                break;
                        }

                        _errorRepository.Insert(newError);
                        _errorRepository.SaveChanges();
                    }
                }           
            }

            return View("PinEverything");
        }

        [HttpGet]
        public ActionResult ReviveAllErrors()
        {
            List<Error> errors = _errorRepository.GetErrors();
            foreach (Error error in errors)
            {
                error.Read = false;
                _errorRepository.Update(error);
            }
            _errorRepository.SaveChanges();
            return View("PinEverything");
        }

        [HttpGet]
        public string s()
        {
            GroupCreator creator = new GroupCreator();
            creator.autoPinGroup = true;
            creator.name = "pikachu";
            creator.sensors = new List<int>() { 1, 2, 3 };
            return JsonConvert.SerializeObject(creator);
        }

        [HttpGet]
        public ActionResult PinEverything()
        {
            if (User.Identity.IsAuthenticated)
            {
                //This will result in the user specific custom homepage
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                List<Tile> tiles = new List<Tile>();
                List<Sensor> sensors = _sensorRepository.GetSensors(null, true);
                foreach (Sensor sensor in sensors)
                {
                    tiles.Add(new Tile() {Pin = new Pin()
                    {
                        SavedId = sensor.Id,
                        SavedType = PinType.Sensor,
                        UserId = user.Id
                    } });

                    foreach (Container container in sensor.Containers)
                    {
                        tiles.Add(new Tile()
                        {
                            Pin = new Pin()
                            {
                                SavedId = container.Id,
                                SavedType = PinType.Container,
                                UserId = user.Id
                            }
                        });
                    }
                }
                _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));
            }

            return View();
        }
    }
}