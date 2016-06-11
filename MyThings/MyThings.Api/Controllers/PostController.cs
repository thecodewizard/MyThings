using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MyThings.Common.Models;
using MyThings.Common.Repositories;
using MyThings.Common.Repositories.RemoteRepositories;
using Newtonsoft.Json;

namespace MyThings.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class PostController : ApiController
    {
        //This controller will manage all the API's POST requests.
        public SensorRepository _sensorRepository = new SensorRepository();

        public HttpResponseMessage UpdateLocation(LocationWebhookModel element)
        {
            if (element.DevEUI != null)
            {
                //Update the location for the received mac address
                LocationApiRepository.UpdateSensorLocation(element.DevEUI).Wait();

                //Write to the blob to acknowledge
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference("locationupdatedlog");
                container.CreateIfNotExists();

                // Retrieve reference to a blob.
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(element.DevEUI + DateTime.Now.Ticks);

                // Create or overwrite the blob with contents from a local file.
                blockBlob.UploadTextAsync(JsonConvert.SerializeObject(element));

            } else return new HttpResponseMessage(HttpStatusCode.BadRequest);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}