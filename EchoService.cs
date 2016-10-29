using System;
using System.Collections.Generic;

namespace EchoService
{
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
