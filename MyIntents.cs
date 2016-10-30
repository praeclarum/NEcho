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
        public async Task Stats (StatisticsIntent i)
        {
            Say ("What's the first number?");
            var first = await Listen<NumberIntent> ();
            Say ("Next?");
            var done = false;
            while (!done) {
                var next = await Listen<NumberIntent, GetStatIntent> ();
                if (next is GetStatIntent) {
                    Say("The stat is not a number. What's next?");
                }
                else {
                    Say ("Now what?");
                }
            }
        }
    }

    public class StatisticsIntent : Intent
    {
        public override string[] Samples => new string[] {
            "start statistics",
            "statistics",
        };
        
    }

    public class NumberIntent : Intent
    {
        public Amazon.NumberSlot Value; 
        public override string[] Samples => new string[] {
            "{Value}",
        };        
    }

    public class GetStatIntent : Intent
    {
        public StatSlot Stat;
        public override string[] Samples => new string[] {
            "what's the {Stat}",
            "{Stat}",
            "now what's the {Stat}",
            "{Stat} is",
            "{Stat} is equal to",
        };
        
    }
    public class StatSlot : Slot
    {
        public override string[] Samples => new string[] {
            "average",
            "sum",
            "variance",
            "mean",
            "median",
            "mode",
        };
        
    }
    public class WhatTimeIsItIntent : Intent
    {
        public override string[] Samples => new string[] {
            "what time is it",
            "what's the time",
        };
        
    }
}

