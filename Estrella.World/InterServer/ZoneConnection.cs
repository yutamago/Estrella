using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Estrella.FiestaLib.Data;
using Estrella.InterLib.Networking;
using Estrella.Util;
using Estrella.World.Data;

namespace Estrella.World.InterServer
{
    public sealed class ZoneConnection : InterClient
    {
        public ZoneConnection(Socket sock) : base(sock)
        {
            IsAZone = false;
            OnPacket += WorldConnection_OnPacket;
            OnDisconnect += WorldConnection_OnDisconnect;
        }

        public bool IsAZone { get; set; }
        public int Load { get; private set; }

        public byte ID { get; set; }
        public ushort Port { get; set; }
        public string IP { get; set; }
        public List<MapInfo> Maps { get; set; }

        void WorldConnection_OnDisconnect(object sender, SessionCloseEventArgs e)
        {
            if (IsAZone)
            {
                OnPacket -= WorldConnection_OnPacket;
                OnDisconnect -= WorldConnection_OnDisconnect;

                ZoneConnection derp;
                if (Program.Zones.TryRemove(ID, out derp))
                {
                    Log.WriteLine(LogLevel.Info, "Zone {0} disconnected.", ID);
                    InterHandler.SendZoneStopped(ID);
                }
                else
                {
                    Log.WriteLine(LogLevel.Info, "Could not remove zone {0}!?", ID);
                }
            }
        }

        void WorldConnection_OnPacket(object sender, InterPacketReceivedEventArgs e)
        {
#if DEBUG
            // so the startup works
            Thread.Sleep(TimeSpan.FromSeconds(3));
#endif

            if (e.Client.Assigned == false)
            {
                if (Program.Zones.Count >= 3)
                {
                    Log.WriteLine(LogLevel.Warn, "We can't load more than 3 zones atm.");
                    e.Client.Disconnect();
                    return;
                }

                if (e.Packet.OpCode == InterHeader.Auth)
                {
                    string pass;
                    if (!e.Packet.TryReadString(out pass))
                    {
                        e.Client.Disconnect();
                        return;
                    }

                    if (!pass.Equals(Settings.Instance.InterPassword))
                    {
                        e.Client.Disconnect();
                    }
                    else
                    {
                        try
                        {
                            e.Client.Assigned = true;

                            ID = Program.GetFreeZoneId();
                            Port = (ushort) (Settings.Instance.ZoneBasePort + ID);

                            var l = DataProvider.Instance.GetMapsForZone(ID);
                            Maps = new List<MapInfo>();
                            foreach (var mapid in l)
                            {
                                MapInfo map;
                                if (DataProvider.Instance.Maps.TryGetValue(mapid, out map))
                                {
                                    Maps.Add(map);
                                }
                                else
                                    Log.WriteLine(LogLevel.Warn, "Zone is loading map {0} which could not be found.",
                                        mapid);
                            }

                            if (Program.Zones.TryAdd(ID, this))
                            {
                                IsAZone = true;
                                SendData();
                                Log.WriteLine(LogLevel.Info, "Added zone {0} with {1} maps.", ID, Maps.Count);
                            }
                            else
                            {
                                Log.WriteLine(LogLevel.Error, "Failed to add zone. Terminating connection.");
                                Disconnect();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(LogLevel.Exception, ex.ToString());
                            Disconnect();
                        }
                    }
                }
                else
                {
                    Log.WriteLine(LogLevel.Info, "Not authenticated and no auth packet first.");
                    e.Client.Disconnect();
                }
            }
            else
            {
                var method = InterHandlerStore.GetHandler(e.Packet.OpCode);
                if (method != null)
                {
                    var action = InterHandlerStore.GetCallback(method, this, e.Packet);
                    if (Worker.Instance == null)
                    {
                        action();
                    }
                    else
                    {
                        Worker.Instance.AddCallback(action);
                    }
                }
                else
                {
                    Log.WriteLine(LogLevel.Debug, "Unhandled interpacket: {0}", e.Packet);
                }
            }
        }

        public void SendTransferClientFromWorld(int accountID, string userName, byte admin, string hostIP, string hash)
        {
            using (var packet = new InterPacket(InterHeader.Clienttransfer))
            {
                packet.WriteByte(0);
                packet.WriteInt(accountID);
                packet.WriteStringLen(userName);
                packet.WriteStringLen(hash);
                packet.WriteByte(admin);
                packet.WriteStringLen(hostIP);
                SendPacket(packet);
            }
        }

        public void SendTransferClientFromZone(int accountID, string userName, string charName, int CharID,
            ushort randid, byte admin, string hostIP)
        {
            using (var packet = new InterPacket(InterHeader.Clienttransfer))
            {
                packet.WriteByte(1);
                packet.WriteInt(accountID);
                packet.WriteStringLen(userName);
                packet.WriteStringLen(charName);
                packet.WriteInt(CharID);
                packet.WriteUShort(randid);
                packet.WriteByte(admin);
                packet.WriteStringLen(hostIP);
                SendPacket(packet);
            }
        }

        public void SendData()
        {
            using (var packet = new InterPacket(InterHeader.Assigned))
            {
                packet.WriteByte(ID);
                packet.WriteStringLen(string.Format("{0}-{1}", Settings.Instance.GameServiceUri, ID));
                packet.WriteUShort((ushort) (Settings.Instance.ZoneBasePort + ID));

                packet.WriteInt(Maps.Count);
                foreach (var m in Maps)
                {
                    packet.WriteUShort(m.ID);
                    packet.WriteStringLen(m.ShortName);
                    packet.WriteStringLen(m.FullName);
                    packet.WriteInt(m.RegenX);
                    packet.WriteInt(m.RegenY);
                    packet.WriteByte(m.Kingdom);
                    packet.WriteUShort(m.ViewRange);
                }

                SendPacket(packet);
            }
        }
    }
}