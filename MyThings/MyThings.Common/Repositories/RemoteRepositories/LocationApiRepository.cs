using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MyThings.Common.Models;
using MyThings.Common.Models.FrontEndModels;
using Newtonsoft.Json;

namespace MyThings.Common.Repositories.RemoteRepositories
{
    public class LocationApiRepository
    {
        private const String getLocationBySensorIdUrl =
            "https://lora.azure-api.net//api/sensor/getRecentSensorByID?ID=";

        private const String registerOnWebhooks = 
            "https://lora.azure-api.net//api/webhook/register";


        private static SensorRepository SensorRepository = new SensorRepository();

        public static async Task UpdateSensorLocation(String MacAddress)
        {
            //Get the sensor
            Sensor sensor = SensorRepository.GetSensorByMacAddress(MacAddress);
            if (sensor == null) return;

            //Fetch the coordinates from the LocationAPI from bart
            bool success = true;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(getLocationBySensorIdUrl + MacAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "4ab5bd3c135348359affd162283d9370");

                
                HttpResponseMessage response = await client.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    String s = await response.Content.ReadAsStringAsync();
                    if (!String.IsNullOrWhiteSpace(s) && !s.Equals("null"))
                    {
                        //If the locationAPI returns a valid response, use it.
                        LocationJsonModel result = JsonConvert.DeserializeObject<LocationJsonModel>(s);
                        if(result.Accuracy != 0) sensor.Accuracy = result.Accuracy;
                        if (Math.Abs(result.lat) > 0)  sensor.Lat = result.lat;
                        if (Math.Abs(result.lng) > 0)  sensor.Lng = result.lng;
                        SensorRepository.Update(sensor);
                        SensorRepository.SaveChanges();
                    }
                    else success = false;
                }
                else success = false;
            }

            if (!success)
            {
                //If the locationAPI returns an invalid response, search the tablestorage for a locationupdate
                sensor = TableStorageRepository.UpdateBasestationCoordinates(sensor);
                SensorRepository.Update(sensor);
                SensorRepository.SaveChanges();
            }
        }

        public static async Task SubscribeOnLocation(String MacAddress)
        {
            //Subscribe on the webhooks
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "4ab5bd3c135348359affd162283d9370");

                var values = new Dictionary<string, string>
                {
                   { "filter", MacAddress },
                   { "key", "4ab5bd3c135348359affd162283d9370" },
                   { "uri", "https://mythingsapi.azurewebsites.net/api/post/updatelocation" },
                   { "user", "esteban.denis.ed@gmail.com" },
                   { "tll", "-1" }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(registerOnWebhooks, content);

                var responseString = await response.Content.ReadAsStringAsync();
            }
        }
    }
}
