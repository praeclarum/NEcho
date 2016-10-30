using System;

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
}

