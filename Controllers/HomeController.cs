using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NEcho.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        static readonly EchoSession session;

        static HomeController()
        {
            session = new My.MySession ();
        }

        [HttpGet]
        public ActionResult Index()
        {
            session.InitIfNeeded();
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            ViewData["intentSchema"] = JsonConvert.SerializeObject(session.Skill.IntentSchema, settings);
            ViewData["sampleUtterances"] = string.Join("\n", session.Skill.SampleUtterances.Select(x => x.Intent + " " + x.Utterance));
            ViewData["customSlotTypes"] = session.Skill.CustomSlotTypes;
            return View();
        }

        [HttpPost]
        public WebServiceData.EchoServiceResponse Post([FromBody]WebServiceData.EchoServiceRequest request)
        {
            session.InitIfNeeded ();
            //Console.WriteLine("GOT REQUEST " + request.Request.Intent.Name);
            return session.HandleRequest(request);
        }
    }
}

