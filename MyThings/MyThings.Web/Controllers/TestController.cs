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
            Container c = new Container();
            c.Sensor = new Sensor();
            c.Sensor.MACAddress = "0E7E35430610005F";
            c.ContainerType = new ContainerType();
            c.ContainerType.Name = "Battery level";

            c =  TableStorageRepository.GetHistory(c, new TimeSpan(24, 0, 0));

            return View();
        }
    }
}