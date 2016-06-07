﻿using System;
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
            String originalGridsterJson = user.GridsterJson;
            List<Tile> tiles = GridsterHelper.JsonToTileList(originalGridsterJson);

            //Define the possible objects to pin
            List<Sensor> pinnedSensors = new List<Sensor>();
            List<Container> pinnedContainers = new List<Container>();
            List<Group> pinnedGroups = new List<Group>();
            List<Error> pinnedErrors = new List<Error>();

            //Fetch the content of the user tiles
            List<Tile> pinnedTiles = new List<Tile>();
            List<Pin> allPins = _pinRepository.GetPinsForUser(user.Id);
            foreach (Tile tile in tiles)
            {
                Pin pin = (from p in allPins where p.TileId == tile.Id select p).FirstOrDefault();
                if (pin != null)
                {
                    //Remove the pin from the list of all pins
                    allPins.Remove(pin);

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
            String gridsterJson = GridsterHelper.TileListToJson(pinnedTiles);

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
    }
}