using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using NEcho.WebServiceData;

namespace NEcho
{
    public class EchoSession
    {
        readonly static ReflectedSkill reflectedSkill = new ReflectedSkill ();
        public EchoSkill Skill => reflectedSkill.EchoSkill;

        readonly TimeSpan listenTimeout = TimeSpan.FromSeconds(5.0);

        bool inited;
        readonly Dictionary<string, Func<Intent, Task>> startIntents =
            new Dictionary<string, Func<Intent, Task>>();
        HashSet<string> listenIntents = null;
        TaskCompletionSource<Intent> listenSource = null;
        readonly AutoResetEvent listening = new AutoResetEvent(false);

        string lastSaid = "";
        public EchoSession()
        {
        }
        public void InitIfNeeded() {
            if (!inited) Init();
        }
        public EchoServiceResponse HandleRequest (EchoServiceRequest request)
        {
            if (!inited) Init();

            var intentValue = request.Request.Intent;

            if (intentValue == null) {
                return new EchoServiceResponse
                {
                    Response = new EchoResponse
                    {
                        OutputSpeech = new EchoSpeech { Type = "PlainText", Text = "" },
                        ShouldEndSession = true,
                    },
                };
            }
            else {
                var intentFullName = intentValue.Name;

                Console.WriteLine("REQUEST INTENT " + intentFullName);

                var ls = listenSource;
                var li = listenIntents;

                lastSaid = "";
                listenSource = null;
                listenIntents = null;

                if (ls != null && li != null && li.Count > 0 && li.Contains(intentFullName)) {
                    Console.WriteLine("CONTINUE SESSION");
                    var intent = reflectedSkill.ParseIntent(intentValue);
                    ls.SetResult(intent);
                }
                else {
                    if (ls != null) {
                        ls.SetCanceled();
                        ls = null;
                    }
                    Console.WriteLine("START NEW SESSION");
                    Func<Intent, Task> startFunc;
                    if (startIntents.TryGetValue(intentFullName, out startFunc)) {
                        var intent = reflectedSkill.ParseIntent(intentValue);
                        var startTask = startFunc(intent);
                    }
                    else {
                        listening.Set();
                    }
                }

                Console.WriteLine("LISTENING...");
                listening.WaitOne(listenTimeout);

                Console.WriteLine("DONE LISTENING, RESPONDING: " + lastSaid);

                return new EchoServiceResponse
                {
                    Response = new EchoResponse
                    {
                        OutputSpeech = new EchoSpeech { Type = "PlainText", Text = lastSaid },
                        ShouldEndSession = listenIntents == null || listenIntents.Count == 0,
                    },
                };
            }
        }

        protected void Say(string message)
        {
            if (lastSaid.Length == 0) lastSaid = message;
            else lastSaid += " " + message;
        }

        protected Task<Intent> Listen(params Type[] intentTypes)
        {
            if (listenSource != null) {
                listenSource.SetCanceled();
            }
            listenSource = new TaskCompletionSource<Intent>();
            var intentNames = intentTypes.Select(x => reflectedSkill.TryFindIntent(x).FullName);
            listenIntents = new HashSet<string>(intentNames);
            listening.Set();
            return listenSource.Task;
        }
        protected Task<Intent> Listen<T0>()
        {
            return Listen(typeof(T0));
        }
        protected Task<Intent> Listen<T0,T1>()
        {
            return Listen(typeof(T0), typeof(T1));
        }
        protected Task<Intent> Listen<T0,T1,T2>()
        {
            return Listen(typeof(T0), typeof(T1), typeof(T2));
        }
        protected Task<Intent> Listen<T0,T1,T2,T3>()
        {
            return Listen(typeof(T0), typeof(T1), typeof(T2), typeof(T3));
        }

        void Init()
        {
            inited = true;
            var type = GetType();
            var methods = type.GetMethods();
            var taskt = typeof(Task);
            foreach (var m in methods) {
                var isTask = taskt.IsAssignableFrom(m.ReturnType);
                var ps = m.GetParameters();
                if (isTask && ps.Length == 1) {
                    var p = ps[0];
                    var n = m.Name;
                    var i = reflectedSkill.TryFindIntent(p.ParameterType); 
                    if (i != null && !startIntents.ContainsKey(i.FullName)) {
                        startIntents.Add(i.FullName, async x => {
                            await (Task)m.Invoke(this, new object[]{x});
                            listening.Set();
                        });
                    }
                }
            }
        }
    }
}

