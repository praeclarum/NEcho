using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
 
namespace WebApplication.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1111", "value2".ToUpperInvariant(), DateTime.Now.ToString() };
        }
 
        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
 
        // POST api/values
        [HttpPost]
        public EchoServiceResponse Post([FromBody]EchoServiceRequest request)
        {
            Console.WriteLine ("GOT REQUEST " + request.Request.Intent.Name);
            return new EchoServiceResponse {
Version="1.0",
Response=new EchoResponse {
OutputSpeech = new EchoSpeech { Type = "PlainText", Text = "Hello World" }
}
};
        }
 
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
 
        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class EchoServiceRequest
{
public string Version;
public EchoSession Session;
public EchoRequest Request;
}
public class EchoSession
{
public string SessionId;
}
public class EchoRequest
{
public EchoIntent Intent;
}
public class EchoIntent
{
public string Name;
}
public class EchoServiceResponse
{
public string Version = "1.0";
public Dictionary<string, dynamic> SessionAttributes = new Dictionary<string, dynamic>();
public EchoResponse Response;
}
public class EchoResponse
{
public EchoSpeech OutputSpeech;
public bool ShouldEndSession = true;
}
public class EchoSpeech
{
public string Type = "PlainText";
public string Text = "";
}
}

