using MyThings.Common.Models;
using MyThings.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MyThings.Common.Helpers;
using MyThings.Common.Models.FrontEndModels;
using Newtonsoft.Json;

namespace MyThings.Web.Controllers
{
    //TODO: Delete this code
    public class TestController : Controller
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
        private readonly PinRepository _pinRepository = new PinRepository();

        // GET: Test
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
        public string s()
        {
            GroupCreator creator = new GroupCreator();
            creator.autoPinGroup = true;
            creator.name = "pikachu";
            creator.sensors = new List<int>() { 1, 2, 3 };
            return JsonConvert.SerializeObject(creator);
        }

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