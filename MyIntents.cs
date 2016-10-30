using System;
using System.Threading.Tasks;

using EchoService;

namespace My
{
    public class ILikePersonIntent : Intent
    {
        public Amazon.UsFirstNameSlot Person;

        public override string[] Utterances => new string[] {
            "i like {Person}",
            "i think like {Person}",
            "maybe i think i like {Person}",
            "{Person} is cool",
        };
    }

    public class BinaryNumberExpressionIntent : Intent
    {
        public Amazon.NumberSlot X;
        public BinaryOperatorSlot Op;
        public Amazon.NumberSlot Y;

        public override string[] Utterances => new string[] {
            "{X} {Op} {Y}",
            "{Op} {X} and {Y}",
        };
    }

    public class BinaryOperatorSlot : Slot
    {
        public override string[] Values => new string[] {
            "add",
            "subtract",
            "multiply",
            "divide",
        };
    }

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
    }

    public class WhatTimeIsItIntent : Intent
    {
        public override string[] Utterances => new string[] {
            "what time is it",
            "what's the time",
        };
        
    }
}

