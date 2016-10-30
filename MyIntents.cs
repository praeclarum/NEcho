using System;
using System.Threading.Tasks;

using EchoService;

namespace My
{
    public class MySession : Session
    {
        public MySession (ReflectedSkill skill) : base(skill) {}

        public async Task WhatTimeIsIt (WhatTimeIsItIntent i)
        {
            var t = DateTime.Now;
            Say (t.ToString());
        }
        public async Task Yes (Amazon.YesIntent i)
        {
            Say ("Why did you say yes?");
        }
        public async Task No (Amazon.NoIntent i)
        {
            Say ("No, don't say no!");
        }
        public async Task Stats (StatisticsIntent i)
        {
            Say ("What's the first number?");
            var first = await Listen<NumberIntent> ();
            Say ("Next?");
            var done = false;
            while (!done) {
                var next = await Listen<NumberIntent> ();
                Say ("Now what?");
            }
        }
    }

    public class StatisticsIntent : Intent
    {
        public override string[] Utterances => new string[] {
            "start statistics",
            "statistics",
        };
        
    }

    public class NumberIntent : Intent
    {
        public Amazon.NumberSlot Value; 
        public override string[] Utterances => new string[] {
            "{Value}",
        };        
    }

    public class WhatTimeIsItIntent : Intent
    {
        public override string[] Utterances => new string[] {
            "what time is it",
            "what's the time",
        };
        
    }
}

