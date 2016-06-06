using MyThings.Common.Models;
using MyThings.Common.Models.NoSQL_Entities;
using MyThings.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyThings.Web.Controllers
{
    public class TestController : Controller
    {
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
    }
}