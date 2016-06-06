using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MyThings.Common.Helpers;
using MyThings.Common.Models;
using MyThings.Common.Repositories;
using MyThings.Common.Repositories.BaseRepositories;
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
        private readonly GroupRepository _groupRepository = new GroupRepository();
        private readonly ErrorRepository _errorRepository = new ErrorRepository();
        private readonly PinRepository _pinRepository = new PinRepository();

        //Site Pages Backend
        [HttpGet]
        public ActionResult Index()
        {
            //This will result in the user specific custom homepage
            ApplicationUser user = UserManager.FindByName(User.Identity.Name);

            //Fetch the tiles of the user & their location
            String tileJson = user.RawGridsterJson;
            List<Tile> tiles = user.UserTilesHome ?? new List<Tile>();

            //Define the possible objects to pin
            List<Sensor> pinnedSensors = new List<Sensor>();
            List<Container> pinnedContainers = new List<Container>();
            List<Group> pinnedGroups = new List<Group>();
            List<Error> pinnedErrors = new List<Error>();

            //Fetch the content of the user tiles
            List<Tile> pinnedTiles = new List<Tile>();
            foreach (Tile tile in tiles)
            {
                Pin pin = _pinRepository.GetPinForTile(user.Id, tile.Id);
                if (pin != null)
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
                            }
                            else
                            {
                                pin.Delete(); //If the pinned group could not be resolved, remove the faulty pin.
                            }
                            break;
                        case PinType.Error:
                            //Give the found error to javascript.
                            Error error = _errorRepository.GetErrorById(pin.SavedId);
                            if (error != null)
                            {
                                pinnedErrors.Add(error);
                            }
                            else
                            {
                                pin.Delete(); //If the pinned error could not be resolved, remove the faulty pin.
                            }
                            break;
                        case PinType.Sensor:
                            //Give the found sensor to javascript
                            Sensor sensor = _sensorRepository.GetSensorById(pin.SavedId);
                            if (sensor != null)
                            {
                                pinnedSensors.Add(sensor);
                            }
                            else
                            {            
                                pin.Delete(); //If the pinned sensor could not be resolved, remove the faulty pin.
                            }
                            break;
                        case PinType.Container:
                            //Give the found container to javascript
                            Container container = _containerRepository.GetContainerById(pin.SavedId);
                            if (container != null)
                            {
                                pinnedContainers.Add(container);
                            }
                            else
                            {
                                pin.Delete(); //If the pinned container could not be resolved, remove the faulty pin.
                            }
                            break;
                        default:
                            pin.Delete(); //If the pin doesn't match any of the known types, remove the faulty pin.
                            break; 
                    }

                    if (!pin.IsDeleted)
                    {
                        tile.Pin = pin;
                        pinnedTiles.Add(tile);
                    }
                    else tile.Delete(); //If the backing pin was deleted, the tile has no use and has to be removed.
                }
                else
                {
                    tile.Delete(); //If an tile was not set to a pin, remove the faulty tile.
                }
            }

            //Based on the validation checks in the logic above, re-generate a filtered gridster json
            String filteredGridsterJson = GridsterHelper.TileListToJson(pinnedTiles);

            //Check the user's sensors for warnings and errors
            List<Error> errors = _errorRepository.GetErrors(); //TODO: Make this only the errors valid to this user.

            ViewBag.CustomTileCount = pinnedTiles.Count;
            ViewBag.FixedTileCount = 9; //Clock, Warnings, Errors, Logout, 4x Nav, Map
            ViewBag.TotalTileCount = ViewBag.FixedTileCount + ViewBag.CustomTileCount;
            return View(new HomePageViewModel()
            {
                OriginalGridsterJson = tileJson,
                FilteredGridsterJson = filteredGridsterJson,

                PinnedSensors = pinnedSensors,
                PinnedContainers = pinnedContainers,
                PinnedGroups = pinnedGroups,
                PinnedErrors = pinnedErrors,

                Errors = errors
            });
        }

        [HttpPost]
        public HttpResponseMessage UpdateGridString(String gridsterJson)
        {
            if (User.Identity.IsAuthenticated)
            {
                //Convert the json to a list of tiles.
                List<Tile> tiles = new List<Tile>();
                try
                {
                    tiles = GridsterHelper.JsonToTileList(gridsterJson);
                }
                catch (Exception ex)
                {
                    //Throw an 'Internal Server Error'
                    HttpResponseMessage message = new HttpResponseMessage();
                    message.StatusCode = HttpStatusCode.InternalServerError;
                    message.Content = new StringContent("Something went wrong whilst parsing the received JSON.");
                    return message;
                }

                //Save the position and the tilelist.
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);
                user.RawGridsterJson = gridsterJson;
                user.UserTilesHome = tiles; //TODO: Recycle Already existing tiles?

                //Acknowledge the save
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                //Throw a 'Not Allowed' Error
                HttpResponseMessage message = new HttpResponseMessage();
                message.StatusCode = HttpStatusCode.MethodNotAllowed;
                message.Content = new StringContent("You must be logged in to perform this operation");
                return message;
            }
        }
    }
}