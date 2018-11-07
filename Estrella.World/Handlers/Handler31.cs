﻿using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.World.Networking;

namespace Estrella.World.Handlers
{
    public sealed class Handler31
    {
        [PacketHandler(CH31Type.GetUnknown)]
        public static void UnknownRequest(WorldClient client, Packet packet)
        {
            if (client.Character == null)
            {
                Log.WriteLine(LogLevel.Warn, "Getting unknown block request from unauthorized host: {0}.", client.Host);
                return;
            }

            SendUnknown(client);
        }

        public static void SendUnknown(WorldClient client)
        {
            using (var packet = new Packet(SH31Type.LoadUnkown))
            {
                packet.WriteInt(0xbd1); //lolwut?!  charid or sumtin'
                client.SendPacket(packet);
            }
        }
    }
}