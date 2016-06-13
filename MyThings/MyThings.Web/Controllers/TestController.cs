using MyThings.Common.Models;
using MyThings.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace MyThings.Web.Controllers
{
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

        //TODO: Delete this code
        public ActionResult PinEverything()
        {
            if (User.Identity.IsAuthenticated)
            {
                //This will result in the user specific custom homepage
                ApplicationUser user = UserManager.FindByName(User.Identity.Name);

                List<Pin> pins = _pinRepository.GetPinsForUser(user.Id);
                foreach(Pin pin in pins) _pinRepository.Delete(pin);
                _pinRepository.SaveChanges();

                List<Sensor> allSensors = _sensorRepository.GetSensors();
                List<Container> allContainers = _containerRepository.GetContainers();
                foreach (Sensor sensor in allSensors)
                {
                    Pin pin = new Pin();
                    pin.SavedId = sensor.Id;
                    pin.SavedType = PinType.Sensor;
                    pin.UserId = user.Id;
                    _pinRepository.Insert(pin);
                }
                _pinRepository.SaveChanges();
                foreach (Container container in allContainers)
                {
                    Pin pin = new Pin();
                    pin.SavedId = container.Id;
                    pin.SavedType = PinType.Container;
                    pin.UserId = user.Id;
                    _pinRepository.Insert(pin);
                }
                _pinRepository.SaveChanges();
            }

            return View();
        }
    }
}