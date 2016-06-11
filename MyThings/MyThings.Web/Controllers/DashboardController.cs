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
using MyThings.Common.Models.FrontEndModels;
using MyThings.Common.Repositories;
using MyThings.Web.ViewModels;
using Newtonsoft.Json;

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

            //Set up caches to improve efficiency
            List<Sensor> cacheSensors =
                (from s in _sensorRepository.GetSensors() where s.Company.Equals(user.Company) select s).ToList();
            List<Container> cacheContainers = (from c in _containerRepository.GetContainers()
                where
                    c.SensorId.HasValue && (from s in cacheSensors select s.Id).ToList<int>().Contains(c.SensorId.Value)
                select c).ToList();
            List<Group> cacheGroups = _groupRepository.GetGroupsForUser(user.Id);
            List<Error> cacheErrors = _errorRepository.GetErrorsForUser(user.Company);

            //Go over all the user's pins and fetch their object. Filter the faulty pins
            bool pinsDeleted = false;
            foreach (Pin pin in allPins)
            {
                //Container & sensor values are fetched with ajax. Group and errors are fetched now
                switch (pin.SavedType)
                {
                    case PinType.Group:
                        //Give the found group to javascript.
                        Group group = (from g in cacheGroups where g.Id.Equals(pin.SavedId) select g).FirstOrDefault();
                        if (group != null)
                        {
                            pinnedGroups.Add(group);
                        }
                        else
                        {
                            _pinRepository.DeletePin(pin);
                            pinsDeleted = true; //If the pinned group could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Error:
                        //Give the found error to javascript.
                        Error error = (from e in cacheErrors where e.Id.Equals(pin.SavedId) select e).FirstOrDefault();
                        if (error != null)
                        {
                            pinnedErrors.Add(error);
                        }
                        else
                        {
                            _pinRepository.DeletePin(pin);
                            pinsDeleted = true; //If the pinned error could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Sensor:
                        //Give the found sensor to javascript
                        Sensor sensor =
                            (from s in cacheSensors where s.Id.Equals(pin.SavedId) select s).FirstOrDefault();
                        if (sensor != null)
                        {
                            pinnedSensors.Add(sensor);
                        }
                        else
                        {
                            _pinRepository.DeletePin(pin);
                            pinsDeleted = true; //If the pinned sensor could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Container:
                        //Give the found container to javascript
                        Container container =
                            (from c in cacheContainers where c.Id.Equals(pin.SavedId) select c).FirstOrDefault();
                        if (container != null)
                        {
                            pinnedContainers.Add(container);
                        }
                        else
                        {
                            _pinRepository.DeletePin(pin);
                            pinsDeleted = true; //If the pinned container could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.FixedClock:
                    case PinType.FixedError:
                    case PinType.FixedNavigation:
                        break; // Don't drop the default pins.
                    default:
                        _pinRepository.DeletePin(pin);
                        pinsDeleted = true; //If the pin doesn't match any of the known types, remove the faulty pin.
                        break;
                }
            }
            if(pinsDeleted) _pinRepository.SaveChanges(); //Save all the pin changes after filtering

            //Go over all the tiles and map their pins.
            if (!String.IsNullOrWhiteSpace(originalGridsterJson))
            {
                List<Tile> tiles = GridsterHelper.JsonToTileList(originalGridsterJson);
                foreach (Tile tile in tiles)
                {
                    Pin pin = (from p in allPins where p.TileId == tile.Id select p).FirstOrDefault();
                    if (pin != null)
                    {
                        tile.Pin = pin;
                        tile.Pin.SavedTypeString = tile.Pin.SavedType.ToString();
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
            List<Error> errors = cacheErrors;

            //Make the viewbag variables
            ViewBag.TotalTileCount = pinnedTiles.Count;
            ViewBag.FixedTileCount = 9; //Clock, Warnings, Errors, Logout, 4x Nav, Map
            ViewBag.CustomTileCount = ViewBag.TotalTileCount - ViewBag.FixedTileCount;
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
                Pin pin = PinRepository.RenderClockPinForUser(user.Id);
                _pinRepository.Insert(pin);
                _pinRepository.SaveChanges();
                allPins.Add(pin);
            }
            //Check navigation
            List<Pin> navPins = (from p in allPins where p.SavedType == PinType.FixedNavigation select p).ToList();
            if (navPins.Count() < 6 && navPins.Any()) //If the count isn't correct, delete all the tiles
            {
                foreach (Pin navPin in navPins) _pinRepository.Delete(navPin);
                _pinRepository.SaveChanges();
                allPins = _pinRepository.GetPinsForUser(user.Id);
            }
            if (!(from p in allPins where p.SavedType == PinType.FixedNavigation select p).Any())
            {
                List<Pin> generatedNavPins = PinRepository.RenderNavigationPinsForUser(user.Id);
                foreach (Pin navPin in generatedNavPins) _pinRepository.Insert(navPin); //If no tile is found, regenerate
                _pinRepository.SaveChanges();
                foreach (Pin navPin in generatedNavPins) allPins.Add(navPin);
            }
            //Check errorpins
            List<Pin> errorPins = (from p in allPins where p.SavedType == PinType.FixedError select p).ToList();
            if (errorPins.Count() < 2 && errorPins.Any())
            {
                foreach (Pin errorPin in errorPins) _pinRepository.Delete(errorPin);
                _pinRepository.SaveChanges();
                allPins = _pinRepository.GetPinsForUser(user.Id);
            }
            if (!(from p in allPins where p.SavedType == PinType.FixedError select p).Any())
            {
                List<Pin> generatedErrorPins = PinRepository.RenderErrorPinsForUser(user.Id);
                foreach (Pin errorPin in generatedErrorPins) _pinRepository.Insert(errorPin);
                _pinRepository.SaveChanges();
                foreach (Pin errorPin in generatedErrorPins) allPins.Add(errorPin);
            }

            return allPins;
        }

        public ActionResult Sensormanagement()
        {
            //This will result in the user specific custom homepage
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);

            //Get the first 50 sensors for the user
            List<Sensor> sensors = (from s in _sensorRepository.GetSensors(50) where s.Company.Equals(user.Company) select s).ToList();

            //Get the different containertypes for the filtering in the combobox
            List<ContainerType> types = _containerTypeRepository.GetContainerTypes();

            //Get the pins for the user
            bool pinsDeleted = false;
            List<Pin> pins = (from p in _pinRepository.GetPinsForUser(user.Id) where p.SavedType.Equals(PinType.Sensor) select p).ToList();
            List<Sensor> pinnedSensors = new List<Sensor>();
            foreach (Pin pin in pins)
            {
                Sensor sensor = (from s in sensors where s.Id.Equals(pin.SavedId) select s).FirstOrDefault();
                if (sensor == null)
                {
                    pins.Remove(pin);
                    _pinRepository.DeletePin(pin); //Delete invalid pins
                    pinsDeleted = true;
                } else pinnedSensors.Add(sensor);
            }
            if(pinsDeleted) _pinRepository.SaveChanges();

            //Get the groups for the user
            List<Group> groups = _groupRepository.GetGroupsForUser(user.Id);

            //Populate the suggestionlist
            List<String> suggestionList = new List<String>();
            foreach (Sensor sensor in sensors)
            {
                if(!String.IsNullOrWhiteSpace(sensor.Name) && !suggestionList.Contains(sensor.Name)) suggestionList.Add(sensor.Name);
                if (!String.IsNullOrWhiteSpace(sensor.Location) && !suggestionList.Contains(sensor.Location))  suggestionList.Add(sensor.Location);
                foreach (Container container in sensor.Containers)
                {
                    if (!String.IsNullOrWhiteSpace(container.Name) && !suggestionList.Contains(container.Name)) suggestionList.Add(container.Name);
                    if (!String.IsNullOrWhiteSpace(container.ContainerType.Name) && !suggestionList.Contains(container.ContainerType.Name))
                        suggestionList.Add(container.ContainerType.Name);
                }    
            }

            return View(new SensorManagementViewModel()
            {
                Sensors = sensors,
                ContainerTypes = types,
                PinnedSensors = pinnedSensors,
                Groups = groups,
                TotalSensors = sensors.Count,
                AutoCompleteSuggestionList = suggestionList
            });
        }
        #endregion

        #region Site API Functionality

        #region Gridster API

        [HttpPost]
        public HttpResponseMessage UpdateGridString(List<Tile> gridsterJson)
        {
            if (User.Identity.IsAuthenticated)
            {
                //Fetch the user
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                //Save the position and the tilelist.
                user.GridsterJson = JsonConvert.SerializeObject(gridsterJson);
                UserManager.Update(user);

                //update pins
                foreach(Tile t in gridsterJson) _pinRepository.Update(t.Pin);
                _pinRepository.SaveChanges();

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

        [HttpGet]
        public string s()
        {
            GroupCreator creator = new GroupCreator();
            creator.autoPinGroup = true;
            creator.name = "pikachu";
            creator.sensors = new List<int>() {1, 2, 3};
            return JsonConvert.SerializeObject(creator);
        }

        [HttpPost]
        public HttpResponseMessage CreateGroup([System.Web.Http.FromBody]GroupCreator groupCreator)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    //Resolve the sensors
                    List<Sensor> sensors = new List<Sensor>();
                    foreach (int sensorId in groupCreator.sensors)
                    {
                        Sensor sensor = _sensorRepository.GetSensorById(sensorId);
                        if(sensor == null) return new HttpResponseMessage(HttpStatusCode.NotFound);
                        sensors.Add(sensor);
                    }

                    //Make the new group
                    Group group = new Group();
                    group.Name = groupCreator.name;
                    group.User_Id = user.Id;
                    group.Sensors = sensors;
                    _groupRepository.Insert(group);
                    _groupRepository.SaveChanges();

                    //Autopin if requested
                    if (groupCreator.autoPinGroup)
                    {
                        Pin pin = new Pin();
                        pin.SavedId = group.Id;
                        pin.SavedType = PinType.Group;
                        pin.UserId = user.Id;
                        _pinRepository.Insert(pin);
                        _pinRepository.SaveChanges();
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK);
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
        public HttpResponseMessage AddSensor(int? groupId, int? sensorId)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (groupId.HasValue && sensorId.HasValue)
                {
                    int gid = groupId.Value;
                    int sid = sensorId.Value;

                    if (!_groupRepository.SensorInGroup(gid, sid))
                    {
                        Group group = _groupRepository.GetGroupById(gid);
                        Sensor sensor = _sensorRepository.GetSensorById(sid);

                        if (group != null && sensor != null)
                        {
                            group.Sensors.Add(sensor);
                            _groupRepository.Update(group);
                            _groupRepository.SaveChanges();

                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
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
        public HttpResponseMessage RemoveSensor(int? groupId, int? sensorId)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (groupId.HasValue && sensorId.HasValue)
                {
                    int gid = groupId.Value;
                    int sid = sensorId.Value;

                    if (_groupRepository.SensorInGroup(gid, sid))
                    {
                        Group group = _groupRepository.GetGroupById(gid);
                        Sensor sensor = _sensorRepository.GetSensorById(sid);

                        if (group != null && sensor != null)
                        {
                            group.Sensors.Remove(sensor);
                            _groupRepository.Update(group);
                            _groupRepository.SaveChanges();

                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
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

        #endregion
    }
}