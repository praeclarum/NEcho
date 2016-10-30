using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EchoService
{
    public class ReflectedSkill
    {
        readonly List<ReflectedIntentInfo> intents = new List<ReflectedIntentInfo> ();
        readonly Dictionary<string, ReflectedIntentInfo> intentFromFullName =
            new Dictionary<string, ReflectedIntentInfo> ();
        readonly EchoSkill echoSkill;

        public EchoSkill EchoSkill => echoSkill;
        public ReflectedSkill ()
        {
            var asm = Assembly.GetEntryAssembly();
            var types = asm.ExportedTypes;
            foreach (var t in types) {
                if (t.Name.EndsWith("Intent")) {
                    var ns = t.Namespace.ToUpperInvariant();
                    var name = t.Name;
                    var fullName = ns + "." + name;
                    var fields = t.GetFields();
                    var echoIntent = new EchoIntentInfo {
                        Intent = fullName,
                        Slots = fields.Select (x => {
                            return new EchoIntentSlot {
                                Name = x.Name,
                                Type = GetSlotFullName(x.FieldType)
                            };
                        }).ToArray(),
                    };
                    var intent = new ReflectedIntentInfo {
                        FullName = fullName,
                        IntentType = t,
                        EchoIntent = echoIntent,
                        Fields = fields,
                    };
                    intents.Add(intent);
                    intentFromFullName.Add(intent.FullName, intent);
                }
            }

            var echoIntents =
                intents.Select(x => x.EchoIntent).ToArray();
            echoSkill = new EchoSkill {
                IntentSchema = new EchoIntentSchema {
                    Intents = echoIntents,
                },
            };
        }
        public ReflectedIntentInfo TryFindIntent (string fullName)
        {
            ReflectedIntentInfo i;
            intentFromFullName.TryGetValue (fullName, out i);
            return i;
        }

        static string GetSlotFullName (Type slotType)
        {
            var ns = slotType.Namespace.ToUpperInvariant();
            var rawName = slotType.Name.Substring(slotType.Name.Length-4);
            var name = rawName.ToUpperInvariant();
            return ns + "." + name;
        }
    }

    public class ReflectedIntentInfo
    {
        public string FullName = "";
        public Type IntentType;
        public FieldInfo[] Fields = new FieldInfo[0];
        public EchoIntentInfo EchoIntent; 
    }
}
