using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EchoService
{
    public class Session
    {
        readonly ReflectedSkill skill;

        readonly TimeSpan listenTimeout = TimeSpan.FromSeconds(5.0);

        bool inited;
        readonly Dictionary<string, Func<Intent, Task>> startIntents =
            new Dictionary<string, Func<Intent, Task>>();
        HashSet<string> listenIntents = null;
        TaskCompletionSource<Intent> listenSource = null;
        readonly AutoResetEvent listening = new AutoResetEvent(false);

        string lastSaid = "";
        public Session(ReflectedSkill skill)
        {
            this.skill = skill;
        }

        public void InitIfNeeded() {
            if (!inited) Init();
        }
        public EchoServiceResponse HandleRequest (EchoServiceRequest request)
        {
            if (!inited) Init();
            var intentValue = request.Request.Intent;
            var intentFullName = intentValue.Name;

            Console.WriteLine("REQUEST INTENT " + intentFullName);

            lastSaid = "";

            if (listenSource != null && listenIntents != null && listenIntents.Count > 0 && listenIntents.Contains(intentFullName)) {
                Console.WriteLine("CONTINUE SESSION");
                var intent = skill.ParseIntent(intentValue);
                listenSource.SetResult(intent);
            }
            else {
                if (listenSource != null) {
                    listenSource.SetCanceled();
                    listenSource = null;
                }
                listenIntents = null;
                Console.WriteLine("START NEW SESSION");
                Func<Intent, Task> startFunc;
                if (startIntents.TryGetValue(intentFullName, out startFunc)) {
                    var intent = skill.ParseIntent(intentValue);
                    var startTask = startFunc(intent);
                }
            }

            Console.WriteLine("LISTENING...");
            listening.WaitOne(listenTimeout);

            Console.WriteLine("DONE LISTENING, RESPONDING: " + lastSaid);

            return new EchoServiceResponse
            {
                Version = "1.0",
                Response = new EchoResponse
                {
                    OutputSpeech = new EchoSpeech { Type = "PlainText", Text = lastSaid },
                    ShouldEndSession = true,
                },
            };
        }

        protected void Say(string message)
        {
            lastSaid = message;
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
                    var i = skill.TryFindIntent(p.ParameterType); 
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

