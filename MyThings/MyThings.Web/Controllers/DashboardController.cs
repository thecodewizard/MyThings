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
using MyThings.Common.Models.NoSQL_Entities;
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
        private readonly ThresholdRepository _thresholdRepository = new ThresholdRepository();

        #region Site Controllers

        [HttpGet]
        public ActionResult Index()
        {
            //This will result in the user specific custom homepage
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);

            //Fetch the tiles and pins of the user
            GridLayoutEntity grid = _pinRepository.GetGridsterJson(user.Id);
            List<Tile> tiles = GridsterHelper.JsonToTileList(grid.GridsterJson ?? "");

            //Define the possible objects to pin
            List<Sensor> pinnedSensors = new List<Sensor>();
            List<Container> pinnedContainers = new List<Container>();
            List<Group> pinnedGroups = new List<Group>();
            List<Error> pinnedErrors = new List<Error>();

            //Check the default tiles for the user
            tiles = CheckDefaultTilesForUser(user, tiles);

            //Set up caches to improve efficiency
            List<Sensor> cacheSensors =
                (from s in _sensorRepository.GetSensors(null, true) where s.Company.Equals(user.Company) select s)
                    .ToList();
            List<Container> cacheContainers = (from c in _containerRepository.GetContainers()
                where c.SensorId.HasValue &&
                      (from s in cacheSensors select s.Id).ToList<int>().Contains(c.SensorId.Value)
                select c).ToList();
            List<Group> cacheGroups = _groupRepository.GetGroupsForUser(user.Id);
            List<Error> cacheErrors = _errorRepository.GetErrorsForUser(user.Company);

            //Go over all the user's pins and fetch their object. Filter the faulty pins
            List<Tile> userTiles = (from t in tiles select t).ToList();
            foreach (Tile tile in userTiles)
            {
                //Container & sensor values are fetched with ajax. Group and errors are fetched now
                switch (tile.Pin.SavedType)
                {
                    case PinType.Group:
                        //Give the found group to javascript.
                        Group group =
                            (from g in cacheGroups where g.Id.Equals(tile.Pin.SavedId) select g).FirstOrDefault();
                        if (group != null)
                        {
                            pinnedGroups.Add(group);
                        }
                        else
                        {
                            tiles.Remove(tile); //If the pinned group could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Error:
                        //Give the found error to javascript.
                        Error error =
                            (from e in cacheErrors where e.Id.Equals(tile.Pin.SavedId) select e).FirstOrDefault();
                        if (error != null)
                        {
                            pinnedErrors.Add(error);
                        }
                        else
                        {
                            tiles.Remove(tile); //If the pinned group could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Sensor:
                        //Give the found sensor to javascript
                        Sensor sensor =
                            (from s in cacheSensors where s.Id.Equals(tile.Pin.SavedId) select s).FirstOrDefault();
                        if (sensor != null)
                        {
                            pinnedSensors.Add(sensor);
                        }
                        else
                        {
                            tiles.Remove(tile); //If the pinned group could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.Container:
                        //Give the found container to javascript
                        Container container =
                            (from c in cacheContainers where c.Id.Equals(tile.Pin.SavedId) select c).FirstOrDefault();
                        if (container != null)
                        {
                            pinnedContainers.Add(container);
                        }
                        else
                        {
                            tiles.Remove(tile); //If the pinned group could not be resolved, remove the faulty pin.
                        }
                        break;
                    case PinType.FixedClock:
                    case PinType.FixedError:
                    case PinType.FixedNavigation:
                        break; // Don't drop the default pins.
                    default:
                        tiles.Remove(tile); //If the pin doesn't match any of the known types, remove the faulty pin.
                        break;
                }
            }

            //Based on the validation checks in the logic above, re-generate a filtered gridster json
            String gridsterJson = GridsterHelper.TileListToJson(tiles) ?? "";

            //Make the viewbag variables
            ViewBag.TotalTileCount = tiles.Count;
            ViewBag.FixedTileCount = 9; //Clock, Warnings, Errors, Logout, 4x Nav, Map
            ViewBag.CustomTileCount = ViewBag.TotalTileCount - ViewBag.FixedTileCount;
            ViewBag.ErrorCount = (from e in cacheErrors where e.Type == ErrorType.Error && e.Read == false select e).Count();
            ViewBag.WarningCount = (from e in cacheErrors where e.Type == ErrorType.Warning && e.Read == false select e).Count();

            //Return the view
            return View(new HomePageViewModel()
            {
                OriginalGridsterJson = grid.GridsterJson,
                GridsterJson = gridsterJson,

                PinnedSensors = pinnedSensors,
                PinnedContainers = pinnedContainers,
                PinnedGroups = pinnedGroups,
                PinnedErrors = pinnedErrors,

                Errors = cacheErrors
            });
        }

        #region HomeHelpers

        private List<Tile> CheckDefaultTilesForUser(ApplicationUser user, List<Tile> tiles)
        {
            //Check if the user's pin already include the static pins. If not, add them
            //Check clock
            if (!(from t in tiles where t.Pin.SavedType == PinType.FixedClock select t).Any())
            {
                tiles.Add(new Tile() {Pin = PinRepository.RenderClockPinForUser(user.Id)});
            }

            //Check navigation
            List<Tile> navTiles = (from t in tiles where t.Pin.SavedType == PinType.FixedNavigation select t).ToList();
            if (navTiles.Count() != 4 && navTiles.Any()) //If the count isn't correct, delete all the pins
            {
                foreach (Tile navTile in navTiles) tiles.Remove(navTile);
            }
            if (!(from t in tiles where t.Pin.SavedType == PinType.FixedNavigation select t).Any())
            {
                List<Pin> generatedNavPins = PinRepository.RenderNavigationPinsForUser(user.Id);
                foreach (Pin navPin in generatedNavPins) tiles.Add(new Tile() {Pin = navPin});
            }

            //Check errorpins
            List<Tile> errorTiles = (from t in tiles where t.Pin.SavedType == PinType.FixedError select t).ToList();
            if (errorTiles.Count() < 2 && errorTiles.Any()) //If the count isn't correct, delete all the pins
            {
                foreach (Tile errorTile in errorTiles) tiles.Remove(errorTile);
            }
            if (!(from t in tiles where t.Pin.SavedType == PinType.FixedError select t).Any())
            {
                List<Pin> generatedErrorPins = PinRepository.RenderErrorPinsForUser(user.Id);
                foreach (Pin errorPin in generatedErrorPins) tiles.Add(new Tile() {Pin = errorPin});
            }

            return tiles;
        }

        #endregion

        [HttpGet]
        [Route("sensormanagement")]
        public ActionResult Sensormanagement(String query = "", int? selectedSensor = null, int? selectedGroup = null)
        {
            //Get the current user
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);

            //Get the first 50 sensors for the user
            List<Sensor> sensors =
                (from s in _sensorRepository.GetSensors(50) where s.Company.Equals(user.Company) select s).ToList();

            //Get the different containertypes for the filtering in the combobox
            List<ContainerType> types = _containerTypeRepository.GetContainerTypes();

            //Get the pins for the user
            List<Tile> tiles = GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
            List<Sensor> pinnedSensors = new List<Sensor>();
            foreach (Tile tile in (from t in tiles where t.Pin.SavedType.Equals(PinType.Sensor) select t).ToList())
            {
                Sensor sensor = (from s in sensors where s.Id.Equals(tile.Pin.SavedId) select s).FirstOrDefault();
                if (sensor != null) pinnedSensors.Add(sensor);
            }

            //Get the groups for the user
            List<Group> groups = _groupRepository.GetGroupsForUser(user.Id);

            //Populate the suggestionlist
            List<String> suggestionList = SuggestionListHelper.GetSuggestionList();

            //Fill the viewbag
            ViewBag.SelectedGroup = selectedGroup;
            ViewBag.SelectedSensor = selectedSensor;
            ViewBag.Query = query;

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

        [HttpGet]
        [Route("sensor/{id}")]
        public ActionResult SensorDetail(int? id)
        {
            if (id.HasValue)
            {
                //Get the current user
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                //Get the sensor
                int sensorId = id.Value;
                Sensor sensor = _sensorRepository.GetSensorById(sensorId);
                if (sensor == null) return RedirectToAction("Index");
                if (!sensor.Company.Equals(user.Company)) return RedirectToAction("Index");

                //Get the warnings and errors for the sensor
                List<Error> errorsForSensor = _errorRepository.GetErrorsForUser(user.Company);

                //Fill the viewbag
                ViewBag.ContainerCount = sensor.Containers.Count;
                ViewBag.ErrorList =
                    (from e in errorsForSensor
                        where e.SensorId.Equals(sensor.Id) && e.Type.Equals(ErrorType.Error)
                        select e).ToList();
                ViewBag.WarningList =
                    (from e in errorsForSensor
                        where e.SensorId.Equals(sensor.Id) && e.Type.Equals(ErrorType.Warning)
                        select e).ToList();

                return View(sensor);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("container/{id}")]
        public ActionResult ContainerDetail(int? id)
        {
            if (id.HasValue)
            {
                //Get the current user
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                //Get the container
                int containerId = id.Value;
                Container container = _containerRepository.GetContainerById(containerId);
                if (container == null) return RedirectToAction("Index");

                //Get the parent sensor
                Sensor sensor = (container.SensorId.HasValue)
                    ? _sensorRepository.GetSensorById(container.SensorId.Value)
                    : null;
                if (sensor == null || !user.Company.Equals(sensor.Company)) return RedirectToAction("Index");

                //Create a new threshold the container
                if (container.Threshold == null)
                {
                    container.Threshold = new Threshold()
                    {
                        BetweenValuesActive = false,
                        FrequencyActive = false,
                        MatchValueActive = false,
                        MatchValue = "",
                        MinUpdateInterval = TimeSpan.FromHours(23)
                    };
                    _containerRepository.Update(container);
                    _containerRepository.SaveChanges();
                }

                //Fill the Viewbag
                ViewBag.ParentSensor = sensor;
                ViewBag.IsPinned = _pinRepository.IsContainerPinned(user.Id, container.Id);
                return View(container);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("error")]
        public ActionResult ErrorList(int? selectedError = null, bool onlyErrors = false, String query = "",
            ErrorCategory category = ErrorCategory.All)
        {
            //Get the current user
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);

            //Get the errors for the user
            List<Error> allErrors = _errorRepository.GetErrorsForUser(user.Company);

            //Fill the category list for the combobox
            List<ErrorCategory> errorCategories = Enum.GetValues(typeof(ErrorCategory)).Cast<ErrorCategory>().ToList();
            List<String> errorCategoryStrings = (from e in errorCategories select e.ToString()).ToList();

            //Fill the auto completion list for the textbox
            List<String> suggestionList = SuggestionListHelper.GetSuggestionList(true, true, true, true, true, true,
                true, true, true, true);

            //Check the pinnedsensors
            List<Sensor> sensors = (from s in _sensorRepository.GetSensors(null, true) where s.Company.Equals(user.Company) select s).ToList();
            List<Tile> tiles = GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
            List<Sensor> pinnedSensors = new List<Sensor>();
            foreach (Tile tile in (from t in tiles where t.Pin.SavedType.Equals(PinType.Sensor) select t).ToList())
            {
                Sensor sensor = (from s in sensors where s.Id.Equals(tile.Pin.SavedId) select s).FirstOrDefault();
                if (sensor != null) pinnedSensors.Add(sensor);
            }

            //Fill the viewbag
            ViewBag.Query = query;
            ViewBag.SelectedError = selectedError;
            ViewBag.SelectedCategory = category;
            ViewBag.ErrorCount = (from e in allErrors where e.Type.Equals(ErrorType.Error) select e).Count();
            ViewBag.WarningCount = (from e in allErrors where e.Type.Equals(ErrorType.Warning) select e).Count();

            ViewBag.PinnedSensors = pinnedSensors;
            return View(new ErrorListViewModel()
            {
                AllErrorsWarnings = allErrors,
                Errors = (from e in allErrors where e.Type.Equals(ErrorType.Error) select e).ToList(),
                Warnings = (from e in allErrors where e.Type.Equals(ErrorType.Warning) select e).ToList(),
                AutoCompleteSuggestionList = suggestionList,
                ErrorsOnly = onlyErrors,
                ErrorCategories = errorCategories,
                ErrorCategoryStrings = errorCategoryStrings
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
                _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(gridsterJson));

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
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    if (!_pinRepository.IsSensorPinned(user.Id, sensorId.Value))
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        tiles.Add(new Tile()
                        {
                            Pin = new Pin()
                            {
                                SavedId = sensorId.Value,
                                SavedType = PinType.Sensor,
                                UserId = user.Id
                            }
                        });
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));

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
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    if (!_pinRepository.IsContainerPinned(user.Id, containerId.Value))
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        tiles.Add(new Tile()
                        {
                            Pin = new Pin()
                            {
                                SavedId = containerId.Value,
                                SavedType = PinType.Container,
                                UserId = user.Id
                            }
                        });
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));

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
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    if (!_pinRepository.IsGroupPinned(user.Id, groupId.Value))
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        tiles.Add(new Tile()
                        {
                            Pin = new Pin()
                            {
                                SavedId = groupId.Value,
                                SavedType = PinType.Group,
                                UserId = user.Id
                            }
                        });
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));

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
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    if (!_pinRepository.IsErrorPinned(user.Id, errorId.Value))
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        tiles.Add(new Tile()
                        {
                            Pin = new Pin()
                            {
                                SavedId = errorId.Value,
                                SavedType = PinType.Error,
                                UserId = user.Id
                            }
                        });
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));

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
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    if (_pinRepository.IsSensorPinned(user.Id, sensorId.Value))
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        Tile tile =
                            (from t in tiles
                                where t.Pin.SavedId.Equals(sensorId.Value) && t.Pin.SavedType.Equals(PinType.Sensor)
                                select t).FirstOrDefault();
                        tiles.Remove(tile);
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));

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
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    if (_pinRepository.IsContainerPinned(user.Id, containerId.Value))
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        Tile tile =
                            (from t in tiles
                                where
                                    t.Pin.SavedId.Equals(containerId.Value) && t.Pin.SavedType.Equals(PinType.Container)
                                select t).FirstOrDefault();
                        tiles.Remove(tile);
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));

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
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    if (_pinRepository.IsGroupPinned(user.Id, groupId.Value))
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        Tile tile =
                            (from t in tiles
                                where t.Pin.SavedId.Equals(groupId.Value) && t.Pin.SavedType.Equals(PinType.Group)
                                select t).FirstOrDefault();
                        tiles.Remove(tile);
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));

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
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                    if (_pinRepository.IsErrorPinned(user.Id, errorId.Value))
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        Tile tile =
                            (from t in tiles
                                where t.Pin.SavedId.Equals(errorId.Value) && t.Pin.SavedType.Equals(PinType.Error)
                                select t).FirstOrDefault();
                        tiles.Remove(tile);
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));

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

        [HttpPost]
        public HttpResponseMessage CreateGroup([System.Web.Http.FromBody] GroupCreator groupCreator)
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
                        if (sensor == null) return new HttpResponseMessage(HttpStatusCode.NotFound);
                        sensors.Add(sensor);
                    }

                    //Make the new group
                    Group group = new Group();
                    group.Name = groupCreator.name;
                    group.User_Id = user.Id;
                    group.Sensors = sensors;
                    _groupRepository.Insert(group);
                    _groupRepository.SaveChanges();

                    //Setup a virtual sensor
                    _groupRepository.CreateVirtualSensor(group);

                    //Autopin if requested
                    if (groupCreator.autoPinGroup)
                    {
                        List<Tile> tiles =
                            GridsterHelper.JsonToTileList(_pinRepository.GetGridsterJson(user.Id).GridsterJson);
                        tiles.Add(new Tile()
                        {
                            Pin = new Pin()
                            {
                                SavedId = group.Id,
                                SavedType = PinType.Group,
                                UserId = user.Id
                            }
                        });
                        _pinRepository.UpdateGridsterJson(user.Id, GridsterHelper.TileListToJson(tiles));
                    }

                    //Return the ID on success
                    HttpResponseMessage successMessage = new HttpResponseMessage();
                    successMessage.StatusCode = HttpStatusCode.OK;
                    successMessage.Content = new StringContent(group.Id.ToString());
                    return successMessage;
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
                            group.IsChanged = true;
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
                        Sensor sensor = group.Sensors.Find(s => s.Id.Equals(sid));

                        if (group != null && sensor != null)
                        {
                            group.IsChanged = true;
                            group.Sensors.Remove(sensor);
                            _groupRepository.Update(group);
                            _groupRepository.SaveChanges();

                            //Remove empty groups
                            if (group.Sensors.Count == 0)
                            {
                                _groupRepository.DeleteGroup(group);
                            }

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

        #region Sensor Management Methods

        [HttpPost]
        public HttpResponseMessage UpdateSensorName(int? sensorId, String name)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (sensorId.HasValue)
                {
                    int Id = sensorId.Value;

                    if (!String.IsNullOrWhiteSpace(name))
                    {
                        //Fetch the user
                        ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                        Sensor sensor = _sensorRepository.GetSensorById(Id);
                        if (sensor != null)
                        {
                            sensor.Name = name;
                            _sensorRepository.Update(sensor);
                            _sensorRepository.SaveChanges();

                            //Return the sensor when successful
                            HttpResponseMessage success = new HttpResponseMessage();
                            success.StatusCode = HttpStatusCode.OK;
                            success.Content = new StringContent(JsonConvert.SerializeObject(sensor));
                            return success;
                        }
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
                    }
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
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

        #region Threshold Management Methods

        [HttpPost]
        public HttpResponseMessage UpdateThreshold(ThresholdCreator threshold)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid && threshold != null)
                {
                    Threshold dbThreshold = _thresholdRepository.GetByID(threshold.Id);
                    Container container = _containerRepository.GetContainerById(threshold.ContainerId);

                    if (dbThreshold != null && container != null)
                    {
                        //Fetch the user
                        ApplicationUser user = UserManager.FindByName(User.Identity.Name);
                        if (container.SensorId.HasValue)
                        {
                            Sensor sensor = _sensorRepository.GetSensorById(container.SensorId.Value);
                            if (!sensor.Company.Equals(user.Company))
                                return new HttpResponseMessage(HttpStatusCode.Forbidden);
                        }

                        //Check for changes and save them
                        if (!dbThreshold.MinValue.Equals(threshold.MinValue) ||
                            !dbThreshold.MaxValue.Equals(threshold.MaxValue) ||
                            !dbThreshold.BetweenValuesActive.Equals(threshold.BetweenValuesActive))
                        {
                            _thresholdRepository.SetBetweenValueThreshold(container, threshold.MinValue,
                                threshold.MaxValue, threshold.BetweenValuesActive);
                        }

                        if (!dbThreshold.MatchValue.Equals(threshold.MatchValue) ||
                            !dbThreshold.MatchValueActive.Equals(threshold.MatchValueActive))
                        {
                            _thresholdRepository.SetExactValueThreshold(container, threshold.MatchValue,
                                threshold.MatchValueActive);
                        }

                        if (!dbThreshold.MinUpdateInterval.Equals(threshold.MinUpdateInterval) ||
                            !dbThreshold.FrequencyActive.Equals(threshold.FrequencyActive))
                        {
                            _thresholdRepository.SetIntervalThreshold(container, threshold.MinUpdateInterval,
                                threshold.FrequencyActive);
                        }

                        //Return the sensor when successful
                        HttpResponseMessage success = new HttpResponseMessage();
                        success.StatusCode = HttpStatusCode.OK;
                        success.Content =
                            new StringContent(JsonConvert.SerializeObject(_thresholdRepository.GetByID(dbThreshold.Id)));
                        return success;
                    }
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
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

        #region Error Management Methods

        [HttpPost]
        public HttpResponseMessage ChangeErrorReadStatus(int? errorId, bool? readstate)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (errorId.HasValue && readstate.HasValue)
                {
                    //Fetch the user
                    ApplicationUser user = UserManager.FindByName(User.Identity.Name);
                    Error error =
                        (from e in _errorRepository.GetErrorsForUser(user.Company) where e.Id.Equals(errorId.Value) select e)
                            .FirstOrDefault();

                    if (error != null)
                    {
                        //Check for changes and save them
                        _errorRepository.MarkErrorAsReaded(error, readstate.Value);

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
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
    }

    #endregion
}