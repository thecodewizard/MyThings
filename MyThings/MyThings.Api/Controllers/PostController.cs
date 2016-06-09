using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using MyThings.Common.Models;
using MyThings.Common.Repositories;

namespace MyThings.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class PostController : ApiController
    {
        //This controller will manage all the API's POST requests.
    }
}