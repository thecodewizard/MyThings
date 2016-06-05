using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyThings.Common.Models;
using MyThings.Common.Repositories;

namespace MyThings.Web.Controllers
{
    public class SqlTestController : Controller
    {
        SensorRepository sensorRepository = new SensorRepository();

        [HttpGet]
        public ActionResult Index()
        {
            List<Sensor> sensors = sensorRepository.GetSensors();
            return View(sensors);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Sensor sensor)
        {
            if (ModelState.IsValid)
            {
                sensorRepository.SaveOrUpdateSensor(sensor);
                return RedirectToAction("Index");
            }
            return View(sensor);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Sensor sensor = sensorRepository.GetSensorById(id);
            return View(sensor);
        }

        [HttpPost]
        public ActionResult Edit(Sensor sensor)
        {
            if (ModelState.IsValid)
            {
                sensorRepository.SaveOrUpdateSensor(sensor);
                return RedirectToAction("Index");
            }
            return View(sensor);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            Sensor sensor = sensorRepository.GetSensorById(id);
            return View(sensor);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            Sensor sensor = sensorRepository.GetSensorById(id);
            return View(sensor);
        }

        [HttpPost]
        public ActionResult Delete(Sensor sensor)
        {
            sensorRepository.DeleteSensor(sensor);
            return RedirectToAction("Index");   
        }
    }
}