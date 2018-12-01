using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using NEcho;
using Amazon;

namespace My
{
    public class MySession : EchoSession
    {
        public Task WhatTimeIsIt (WhatTimeIsItIntent i)
        {
            var t = DateTime.Now;
            Say (t.ToString());
            return Task.FromResult (0);
        }
        public async Task Stats (StatisticsIntent si)
        {
            var numbers = new List<double> ();

            Say ("What's the first number?");
            var i = await Listen<NumberIntent> ();

            Say ("Next?");
            numbers.Add(((NumberIntent)i).NumberValue);
            var done = false;
            while (!done) {
                i = await Listen<NumberIntent, GetStatIntent> ();

                if (i is NumberIntent) {
                    numbers.Add(((NumberIntent)i).NumberValue);
                }
                else if (i is GetStatIntent) {
                    var x = 0.0;
                    switch (((GetStatIntent)i).Stat.LowerValue) {
                    case "mean":
                    case "average":
                        x = Math.Round(numbers.Average());
                        Say($"The average is {x}.");
                        break;
                    case "sum":
                        x = numbers.Sum();
                        Say($"The sum is {x}.");
                        break;
                    case "min":
                    case "minimum":
                        x = numbers.Min();
                        Say($"The min is {x}.");
                        break;
                    case "max":
                    case "maximum":
                        x = numbers.Max();
                        Say($"The max is {x}.");
                        break;
                    }
                }
                Say("What's next?");
            }
        }
    }

    public class StatisticsIntent : Intent
    {
        public override string[] Samples => new string[] {
            "start statistics",
            "statistics",
            "stats",
            "start stats",
            "begin stats",
        };
        
    }

    public class NumberIntent : Intent
    {
        public NumberSlot Value;
        public double NumberValue => double.Parse(Value.Value); 
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
            "mean",
            "min",
            "max",
            "minimum",
            "maximum",
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
