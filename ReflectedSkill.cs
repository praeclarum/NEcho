using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NEcho.WebServiceData;

namespace NEcho
{
    public class ReflectedSkill
    {
        readonly List<ReflectedIntentInfo> intents = new List<ReflectedIntentInfo> ();
        readonly Dictionary<string, ReflectedIntentInfo> intentFromFullName =
            new Dictionary<string, ReflectedIntentInfo> ();
        readonly Dictionary<Type, ReflectedIntentInfo> intentFromType =
            new Dictionary<Type, ReflectedIntentInfo> ();
        readonly List<ReflectedSlotInfo> slots = new List<ReflectedSlotInfo> ();
        readonly Dictionary<string, ReflectedSlotInfo> slotFromFullName =
            new Dictionary<string, ReflectedSlotInfo> ();
        readonly EchoSkill echoSkill;

        public EchoSkill EchoSkill => echoSkill;
        public ReflectedSkill ()
        {
            var asm = Assembly.GetEntryAssembly();
            var types = asm.ExportedTypes;
            var utterances = new List<EchoSampleUtterance>();
            foreach (var t in types) {
                if (t.Name.EndsWith("Intent") && t.Name != "Intent") {
                    var ns = t.Namespace.ToUpperInvariant();
                    var name = t.Name;
                    var fullName = (ns == "AMAZON") ? ns + "." + name : name;
                    var fields = t.GetFields();
                    var echoIntent = t.Namespace == "NEcho.Amazon" ?
                        (EchoBaseIntentInfo)(new EchoBuiltinIntentInfo { Intent = fullName }) :
                        (new EchoIntentInfo {
                            Intent = fullName,
                            Slots = fields.Select (x =>
                                new EchoIntentSlotInfo {
                                    Name = x.Name,
                                    Type = GetSlotFullName(x.FieldType)
                                }).ToArray(),
                             });                    
                    var intent = new ReflectedIntentInfo {
                        FullName = fullName,
                        IntentType = t,
                        EchoIntent = echoIntent,
                        Fields = fields.ToDictionary(x => x.Name),
                    };
                    var intentObj = Activator.CreateInstance(t) as Intent;
                    if (intentObj != null) {
                        foreach (var u in intentObj.Samples) {
                            utterances.Add(new EchoSampleUtterance {
                                Intent = fullName,
                                Utterance = u,
                            });
                        }
                    }
                    intents.Add(intent);
                    intentFromFullName.Add(intent.FullName, intent);
                    intentFromType.Add(t, intent);
                }
                else if (t.Name.EndsWith("Slot") && t.Name != "Slot") {
                    var values = new string[0];
                    var slotObj = Activator.CreateInstance(t) as Slot;
                    if (slotObj != null) {
                        values = slotObj.Samples;
                    }
                    var fullName = GetSlotFullName(t);
                    var echoSlot =
                        new EchoCustomSlotType {
                            Name = fullName,
                            Values = values};
                    var slot = new ReflectedSlotInfo {
                        FullName = fullName,
                        SlotType = t,
                        EchoSlot = echoSlot,
                    };
                    slots.Add(slot);
                    slotFromFullName.Add(slot.FullName, slot);
                }
            }

            var echoIntents =
                intents.Select(x => x.EchoIntent).ToArray();
            var echoSlots =
                slots.Where(x => !x.FullName.StartsWith("AMAZON.")).
                Select(x => x.EchoSlot).ToArray();
            echoSkill = new EchoSkill {
                IntentSchema = new EchoIntentSchema {
                    Intents = echoIntents,
                },
                SampleUtterances = utterances.ToArray(),
                CustomSlotTypes = echoSlots, 
            };
        }
        public ReflectedIntentInfo TryFindIntent (string fullName)
        {
            ReflectedIntentInfo i;
            intentFromFullName.TryGetValue (fullName, out i);
            return i;
        }
        public ReflectedIntentInfo TryFindIntent (Type type)
        {
            ReflectedIntentInfo i;
            intentFromType.TryGetValue (type, out i);
            return i;
        }

        public Intent ParseIntent (WebServiceData.EchoIntentValue intentValue)
        {
            var info = intentFromFullName[intentValue.Name];
            var intent = (Intent)Activator.CreateInstance(info.IntentType);

            foreach (var s in intentValue.Slots) {
                var name = s.Value.Name;
                var value = s.Value.Value;
                FieldInfo f;
                if (info.Fields.TryGetValue(name, out f)) {
                    var slott = f.FieldType;
                    var slot = (Slot)Activator.CreateInstance(slott);
                    slot.Value = value;
                    f.SetValue(intent, slot);
                }
            }

            return intent;            
        }

        static string GetSlotFullName (Type slotType)
        {
            var ns = slotType.Namespace.ToUpperInvariant();
            var rawName = slotType.Name.Substring(0, slotType.Name.Length-4);
            var parts = new List<StringBuilder>();
            var lastUpper = false;
            foreach (var c in rawName) {
                var u = char.IsUpper(c);
                if (u) {
                    var sb = new StringBuilder();
                    sb.Append(c);
                    parts.Add(sb);
                }
                else if (parts.Count > 0) {
                    parts.Last().Append(c);
                }
                lastUpper = u;
            }     
            var name = string.Join("_", parts).ToUpperInvariant();
            return (ns == "AMAZON") ? ns + "." + name : name;
        }
    }

    public class ReflectedIntentInfo
    {
        public string FullName = "";
        public Type IntentType;
        public Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>();
        public EchoBaseIntentInfo EchoIntent; 
    }

    public class ReflectedSlotInfo
    {
        public string FullName = "";
        public Type SlotType;
        public EchoCustomSlotType EchoSlot;
    }

    public class Intent
    {
        public virtual string[] Samples { get { return new string[0]; } }        
    } 
    public class Slot
    {
        public string Value = "";
        public string LowerValue => Value.ToLowerInvariant();
        public virtual string[] Samples { get { return new string[0]; } }        
    }
}
