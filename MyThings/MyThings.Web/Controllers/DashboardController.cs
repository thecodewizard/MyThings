using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MyThings.Common.Helpers;
using MyThings.Common.Models;
using MyThings.Common.Repositories;
using MyThings.Web.ViewModels;

namespace MyThings.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
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
        private readonly ContainerTypeRepository _containerTypeRepository = new ContainerTypeRepository();
        private readonly GroupRepository _groupRepository = new GroupRepository();
        private readonly ErrorRepository _errorRepository = new ErrorRepository();
        private readonly PinRepository _pinRepository = new PinRepository();

        #region Site Controllers
        [HttpGet]
        public ActionResult Index()
        {
            //This will result in the user specific custom homepage
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);

            //Fetch the tiles of the user & their location
            String originalGridsterJson = user.GridsterJson ?? "";

            //Define the possible objects to pin
            List<Sensor> pinnedSensors = new List<Sensor>();
            List<Container> pinnedContainers = new List<Container>();
            List<Group> pinnedGroups = new List<Group>();
            List<Error> pinnedErrors = new List<Error>();

            //Fetch all pinned objects for the current user.
            List<Tile> pinnedTiles = new List<Tile>();
            List<Pin> allPins = _pinRepository.GetPinsForUser(user.Id);

            //Check the default tiles for the user
            allPins = CheckDefaultTilesForUser(user, allPins);

            //Go over all the user's pins and fetch their object. Filter the faulty pins
            foreach (Pin pin in allPins)
            {
                //Container & sensor values are fetched with ajax. Group and errors are fetched now
                switch (pin.SavedType)
                {
                    case PinType.Group:
                        //Give the found group to javascript.
                        Group group = _groupRepository.GetGroupById(pin.SavedId);
                        if (group != null)
                        {
                            pinnedGroups.Add(group);
                        } else
                        {
                            _pinRepository.DeletePin(pin); //If the pinned group could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Error:
                        //Give the found error to javascript.
                        Error error = _errorRepository.GetErrorById(pin.SavedId);
                        if (error != null)
                        {
                            pinnedErrors.Add(error);
                        } else
                        {
                            _pinRepository.DeletePin(pin); //If the pinned error could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Sensor:
                        //Give the found sensor to javascript
                        Sensor sensor = _sensorRepository.GetSensorById(pin.SavedId);
                        if (sensor != null)
                        {
                            pinnedSensors.Add(sensor);
                        } else
                        {
                            _pinRepository.DeletePin(pin); //If the pinned sensor could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Container:
                        //Give the found container to javascript
                        Container container = _containerRepository.GetContainerById(pin.SavedId);
                        if (container != null)
                        {
                            pinnedContainers.Add(container);
                        } else
                        {
                            _pinRepository.DeletePin(pin); //If the pinned container could not be resolved, remove the faulty pin.
                        }
                        break;
                    default:
                        _pinRepository.DeletePin(pin);//If the pin doesn't match any of the known types, remove the faulty pin.
                        break;
                }

                _pinRepository.SaveChanges();
            }

            //Go over all the tiles and map their pins.
            if (String.IsNullOrWhiteSpace(originalGridsterJson))
            {
                List<Tile> tiles = GridsterHelper.JsonToTileList(originalGridsterJson);
                foreach (Tile tile in tiles)
                {
                    Pin pin = (from p in allPins where p.TileId == tile.Id select p).FirstOrDefault();
                    if (pin != null)
                    {
                        tile.Pin = pin;
                        allPins.Remove(pin);
                        pinnedTiles.Add(tile);
                    }
                }
            }

            //Inject new tiles for unassigned pins
            foreach (Pin pin in allPins)
            {
                Tile tile = new Tile();
                tile.Pin = pin;
                pinnedTiles.Add(tile);
            }

            //Based on the validation checks in the logic above, re-generate a filtered gridster json
            String gridsterJson = GridsterHelper.TileListToJson(pinnedTiles) ?? "";

            //Check the user's sensors for warnings and errors
            List<Error> errors = _errorRepository.GetErrors(); //TODO: Make this only the errors valid to this user.

            //Make the viewbag variables
            ViewBag.CustomTileCount = pinnedTiles.Count;
            ViewBag.FixedTileCount = 9; //Clock, Warnings, Errors, Logout, 4x Nav, Map
            ViewBag.TotalTileCount = ViewBag.FixedTileCount + ViewBag.CustomTileCount;
            ViewBag.ErrorCount = (from e in errors where e.Type == ErrorType.Error select e).Count();
            ViewBag.WarningCount = (from e in errors where e.Type == ErrorType.Warning select e).Count();

            //Return the view
            return View(new HomePageViewModel()
            {
                OriginalGridsterJson = originalGridsterJson,
                GridsterJson = gridsterJson,

                PinnedSensors = pinnedSensors,
                PinnedContainers = pinnedContainers,
                PinnedGroups = pinnedGroups,
                PinnedErrors = pinnedErrors,

                Errors = errors
            });
        }

        private List<Pin> CheckDefaultTilesForUser(ApplicationUser user, List<Pin> allPins)
        {
            //Check if the user's pin already include the static pins. If not, add them
            //Check clock
            if (!(from p in allPins where p.SavedType == PinType.FixedClock select p).Any())
            {
                allPins.Add(_pinRepository.Insert(PinRepository.RenderClockPinForUser(user.Id)));
            }
            //Check navigation
            List<Pin> navPins = (from p in allPins where p.SavedType == PinType.FixedNavigation select p).ToList();
            if (navPins.Count() < 6) //If the count isn't correct, delete all the tiles
            {
                foreach (Pin navPin in navPins) _pinRepository.Delete(navPin);
                _pinRepository.SaveChanges();
                allPins = _pinRepository.GetPinsForUser(user.Id);
            }
            if (!(from p in allPins where p.SavedType == PinType.FixedNavigation select p).Any())
            {
                foreach (Pin navPin in PinRepository.RenderNavigationPinsForUser(user.Id)) //If no tile is found, regenerate
                    allPins.Add(_pinRepository.Insert(navPin));
            }
            //Check errorpins
            List<Pin> errorPins = (from p in allPins where p.SavedType == PinType.FixedError select p).ToList();
            if (errorPins.Count() < 2)
            {
                foreach (Pin errorPin in errorPins) _pinRepository.Delete(errorPin);
                _pinRepository.SaveChanges();
                allPins = _pinRepository.GetPinsForUser(user.Id);
            }
            if (!(from p in allPins where p.SavedType == PinType.FixedError select p).Any())
            {
                foreach (Pin errorPin in PinRepository.RenderErrorPinsForUser(user.Id))
                    allPins.Add(_pinRepository.Insert(errorPin));
            }

            //Save the new pins
            _pinRepository.SaveChanges();
            return allPins;
        }

        #endregion

        #region DummyDataGenerator
        //TODO: Remove the 'generate dummy data' method
        private void GenerateDummyData()
        {
            //Make a new dummy sensor
            Sensor dummySensor = new Sensor()
            {
                Name = "Lora Mc Loraface",
                Company = "Telenet",
                MACAddress = Guid.NewGuid().ToString(),
                Location = "Hier",
                CreationDate = DateTime.Now,
                SensorEntries = 1,
            };
            _sensorRepository.Insert(dummySensor);
            _sensorRepository.SaveChanges();

            //Make a dummy containertype
            ContainerType type = new ContainerType() { Name = "Drughs Container" };
            _containerTypeRepository.Insert(type);
            _containerTypeRepository.SaveChanges();

            //Make a dummy container
            Container dummyContainer = new Container()
            {
                Name = "Frank",
                MACAddress = dummySensor.MACAddress,
                CreationTime = DateTime.Now,
                ContainerType = type,
                SensorId = dummySensor.Id
            };
            _containerRepository.Insert(dummyContainer);
            _containerRepository.SaveChanges();

            //Add container to sensor
            dummySensor.Containers = new List<Container>() { dummyContainer };
            _sensorRepository.Update(dummySensor);
            _sensorRepository.SaveChanges();

            //Make a dummy group
            Group dummyGroup = new Group()
            {
                Name = "Plankton",
                Sensors = new List<Sensor>()
                {
                    dummySensor
                }
            };
            _groupRepository.Insert(dummyGroup);
            _groupRepository.SaveChanges();

            //Make a dummy error
            Error dummyWarning = Error.GenericWarning(dummySensor, dummyContainer);
            Error dummyError = Error.GenericError(dummySensor, dummyContainer);
            _errorRepository.Insert(dummyError);
            _errorRepository.Insert(dummyWarning);
            _errorRepository.SaveChanges();

            //Make a dummy pin
            Pin dummyPin = new Pin()
            {
                UserId = UserManager.FindByName(User.Identity.Name).Id,
                SavedId = dummySensor.Id,
                SavedType = PinType.Sensor
            };
            _pinRepository.Insert(dummyPin);

            //Make a second dummy pin
            Pin dummyPin2 = new Pin()
            {
                UserId = UserManager.FindByName(User.Identity.Name).Id,
                SavedId = dummyGroup.Id,
                SavedType = PinType.Sensor
            };
            _pinRepository.Insert(dummyPin2);
            _pinRepository.SaveChanges();
        }

        #endregion

        #region Site API Functionality

        #region Gridster API

        [HttpPost]
        public HttpResponseMessage UpdateGridString(String gridsterJson)
        {
            if (User.Identity.IsAuthenticated)
            {
                //Fetch the user
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                //Save the position and the tilelist.
                user.GridsterJson = gridsterJson;

                //Acknowledge the save
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;
        }
        #endregion

        #region Pin Objects Methods

        [HttpPost]
        public HttpResponseMessage PinSensor(int? sensorId = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (sensorId.HasValue)
                {
                    if (!_pinRepository.IsSensorPinned(sensorId.Value))
                    {
                        Pin pin = new Pin();
                        pin.SavedId = sensorId.Value;
                        pin.SavedType = PinType.Sensor;
                        pin.UserId = UserManager.FindByName(User.Identity.Name).Id;
                        _pinRepository.Insert(pin);
                        _pinRepository.SaveChanges();

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;
        }

        [HttpPost]
        public HttpResponseMessage PinContainer(int? containerId = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (containerId.HasValue)
                {
                    if (!_pinRepository.IsContainerPinned(containerId.Value))
                    {
                        Pin pin = new Pin();
                        pin.SavedId = containerId.Value;
                        pin.SavedType = PinType.Container;
                        pin.UserId = UserManager.FindByName(User.Identity.Name).Id;
                        _pinRepository.Insert(pin);
                        _pinRepository.SaveChanges();

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;

        }

        [HttpPost]
        public HttpResponseMessage PinGroup(int? groupId = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (groupId.HasValue)
                {
                    if (!_pinRepository.IsGroupPinned(groupId.Value))
                    {
                        Pin pin = new Pin();
                        pin.SavedId = groupId.Value;
                        pin.SavedType = PinType.Group;
                        pin.UserId = UserManager.FindByName(User.Identity.Name).Id;
                        _pinRepository.Insert(pin);
                        _pinRepository.SaveChanges();

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;

        }

        [HttpPost]
        public HttpResponseMessage PinError(int? errorId = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (errorId.HasValue)
                {
                    if (!_pinRepository.IsErrorPinned(errorId.Value))
                    {
                        Pin pin = new Pin();
                        pin.SavedId = errorId.Value;
                        pin.SavedType = PinType.Error;
                        pin.UserId = UserManager.FindByName(User.Identity.Name).Id;
                        _pinRepository.Insert(pin);
                        _pinRepository.SaveChanges();

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;

        }
        #endregion

        #region Unpin Objects Methods

        [HttpPost]
        public HttpResponseMessage UnpinSensor(int? sensorId = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (sensorId.HasValue)
                {
                    if (_pinRepository.IsSensorPinned(sensorId.Value))
                    {
                        int pinId = _pinRepository.GetPinId(sensorId.Value, PinType.Sensor);
                        Pin pin = _pinRepository.GetPinById(pinId);
                        _pinRepository.DeletePin(pin);
                        _pinRepository.SaveChanges();

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;
        }

        [HttpPost]
        public HttpResponseMessage UnpinContainer(int? containerId = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (containerId.HasValue)
                {
                    if (_pinRepository.IsContainerPinned(containerId.Value))
                    {
                        int pinId = _pinRepository.GetPinId(containerId.Value, PinType.Container);
                        Pin pin = _pinRepository.GetPinById(pinId);
                        _pinRepository.DeletePin(pin);
                        _pinRepository.SaveChanges();

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;
        }

        [HttpPost]
        public HttpResponseMessage UnpinGroup(int? groupId = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (groupId.HasValue)
                {
                    if (_pinRepository.IsGroupPinned(groupId.Value))
                    {
                        int pinId = _pinRepository.GetPinId(groupId.Value, PinType.Group);
                        Pin pin = _pinRepository.GetPinById(pinId);
                        _pinRepository.DeletePin(pin);
                        _pinRepository.SaveChanges();

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;
        }

        [HttpPost]
        public HttpResponseMessage UnpinError(int? errorId = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (errorId.HasValue)
                {
                    if (_pinRepository.IsErrorPinned(errorId.Value))
                    {
                        int pinId = _pinRepository.GetPinId(errorId.Value, PinType.Error);
                        Pin pin = _pinRepository.GetPinById(pinId);
                        _pinRepository.DeletePin(pin);
                        _pinRepository.SaveChanges();

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            //Throw a 'Not Allowed' Error
            HttpResponseMessage message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.MethodNotAllowed;
            message.Content = new StringContent("You must be logged in to perform this operation");
            return message;
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

        #endregion
    }
}