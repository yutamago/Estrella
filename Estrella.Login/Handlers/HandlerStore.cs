using System;
using System.Collections.Generic;
using System.Reflection;
using Estrella.FiestaLib.Networking;
using Estrella.Util;

namespace Estrella.Login.Handlers
{
    [ServerModule(InitializationStage.Metadata)]
    public static class HandlerStore
    {
        private static Dictionary<byte, Dictionary<byte, MethodInfo>> _handlers;

        [InitializerMethod]
        public static bool Load()
        {
            _handlers = new Dictionary<byte, Dictionary<byte, MethodInfo>>();
            foreach (var info in Reflector.FindMethodsByAttribute<PacketHandlerAttribute>())
            {
                var attribute = info.First;
                var method = info.Second;
                if (!_handlers.ContainsKey(attribute.Header))
                    _handlers.Add(attribute.Header, new Dictionary<byte, MethodInfo>());
                if (_handlers[attribute.Header].ContainsKey(attribute.Type))
                {
                    Log.WriteLine(LogLevel.Warn, "Duplicate handler found: {0}:{1}", attribute.Header, attribute.Type);
                    _handlers[attribute.Header].Remove(attribute.Type);
                }

                _handlers[attribute.Header].Add(attribute.Type, method);
            }

            var count = 0;
            foreach (var dict in _handlers.Values)
                count += dict.Count;
            Log.WriteLine(LogLevel.Info, "{0} Handlers loaded.", count);
            return true;
        }

        public static MethodInfo GetHandler(byte header, byte type)
        {
            if (!_handlers.TryGetValue(header, out var dict)) return null;
            
            return dict.TryGetValue(type, out var methodInfo) ? methodInfo : null;
        }

        public static Action GetCallback(MethodInfo method, params object[] parameters)
        {
            return () => method.Invoke(null, parameters);
        }
    }
}