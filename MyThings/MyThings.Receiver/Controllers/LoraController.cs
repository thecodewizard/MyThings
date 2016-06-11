using Proximus_API.Models;
using Proximus_Webservice.Repositories;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace Proximus_API.Controllers
{
    public class LoraController : ApiController
    {
        [Route("decoded")]
        [HttpPost]
        public HttpResponseMessage JSON([FromBody] DecodedData body)
        {
            if (body == null || !ModelState.IsValid)
            {
                AzureRepository.WriteErrorToTable(new Exception("Bad Request on JSON API."));
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            try
            {
                AzureRepository.WriteToTableDecoded(body);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                AzureRepository.WriteErrorToTable(ex);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [Route("netwerk")]
        [HttpPost]
        public HttpResponseMessage XML(HttpRequestMessage request)
        {
            if (request == null || !ModelState.IsValid)
            {
                AzureRepository.WriteErrorToTable(new Exception("Bad Request on XML API."));
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            } 
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(request.Content.ReadAsStreamAsync().Result);
                MemoryStream stm = new MemoryStream();
                StreamWriter stw = new StreamWriter(stm);
                stw.Write(xmlDoc.OuterXml);
                stw.Flush();
                stm.Position = 0;
                XmlSerializer ser = new XmlSerializer(typeof(DevEUI_uplink));
                AzureRepository.WriteToTableNetwerk(ser.Deserialize(stm) as DevEUI_uplink);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                AzureRepository.WriteErrorToTable(ex);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

    }
}
