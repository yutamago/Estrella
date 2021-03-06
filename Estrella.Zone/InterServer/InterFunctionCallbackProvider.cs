﻿using System;
using System.Collections.Generic;
using System.Threading;
using Estrella.InterLib.Networking;
using Estrella.Util;

namespace Estrella.Zone.InterServer
{
    [ServerModule(InitializationStage.Networking)]
    public class InterFunctionCallbackProvider
    {
        public InterFunctionCallbackProvider()
        {
            waithandlers = new Dictionary<long, Mutex>();
            results = new Dictionary<long, object>();
            readFuncs = new Dictionary<long, Func<InterPacket, object>>();
            nextId = 0;
        }

        public static InterFunctionCallbackProvider Instance { get; private set; }

        private Dictionary<long, Mutex> waithandlers;
        private Dictionary<long, object> results;
        private Dictionary<long, Func<InterPacket, object>> readFuncs;
        private long nextId;

        [InitializerMethod]
        public static bool Initialize()
        {
            Instance = new InterFunctionCallbackProvider();
            return true;
        }

        public object QueuePacket(Func<long, InterPacket> getPacket, Func<InterPacket, object> readFromPacket)
        {
            var id = GetNextId();
            readFuncs.Add(id, readFromPacket);
            Mutex m;
            using (var packet = getPacket(id))
                m = QueueAndReturnMutex(packet, id);
            m.WaitOne();
            var returnValue = results[id];
            results.Remove(id);
            waithandlers.Remove(id);
            readFuncs.Remove(id);
            return returnValue;
        }

        internal Func<InterPacket, object> GetReadFunc(long id)
        {
            return readFuncs[id];
        }

        internal void OnResult(long id, object result)
        {
            if (id == 0)
            {
                return;
            }

            results.Add(id, result);
            waithandlers[id].ReleaseMutex();
        }

        private Mutex QueueAndReturnMutex(InterPacket packet, long id)
        {
            var m = new Mutex();
            m.WaitOne();
            waithandlers.Add(id, m);
            WorldConnector.Instance.SendPacket(packet);
            return m;
        }

        private long GetNextId()
        {
            var id = nextId;
            nextId++;
            return id;
        }
    }
}