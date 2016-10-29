using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using EchoService;

namespace WebApplication.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public EchoServiceResponse Post([FromBody]EchoServiceRequest request)
        {
            //Console.WriteLine("GOT REQUEST " + request.Request.Intent.Name);
            return new EchoServiceResponse
            {
                Version = "1.0",
                Response = new EchoResponse
                {
                    OutputSpeech = new EchoSpeech { Type = "PlainText", Text = "Hi Frank, this is fun!" }
                }
            };
        }
    }
}

