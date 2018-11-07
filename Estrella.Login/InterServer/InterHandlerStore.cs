using System;
using System.Collections.Generic;
using System.Reflection;
using Estrella.InterLib.Networking;
using Estrella.Util;

namespace Estrella.Login.InterServer
{
    [ServerModule(InitializationStage.Metadata)]
    public class InterHandlerStore
    {
        private static Dictionary<InterHeader, MethodInfo> _handlers;

        [InitializerMethod]
        public static bool Load()
        {
            _handlers = new Dictionary<InterHeader, MethodInfo>();
            foreach (var info in Reflector.FindMethodsByAttribute<InterPacketHandlerAttribute>())
            {
                var attribute = info.First;
                var method = info.Second;
                if (!_handlers.ContainsKey(attribute.Header))
                {
                    _handlers[attribute.Header] = method;
                }
                else
                {
                    Log.WriteLine(LogLevel.Warn, "Duplicate interhandler found: {0}", attribute.Header.ToString());
                }
            }

            Log.WriteLine(LogLevel.Info, "{0} InterHandlers loaded.", _handlers.Count);
            return true;
        }

        public static MethodInfo GetHandler(InterHeader ih)
        {
            return _handlers.TryGetValue(ih, out var methodInfo) ? methodInfo : null;
        }

        public static Action GetCallback(MethodInfo method, params object[] parameters)
        {
            return () => method.Invoke(null, parameters);
        }
    }
}