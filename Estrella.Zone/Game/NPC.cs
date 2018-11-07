using System;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.Zone.Data;

namespace Estrella.Zone.Game
{
    public sealed class Npc : MapObject
    {
        //public byte Type { get; private set; } //TODO: load from?
        public ShineNpc Point;

        public Npc(ShineNpc spoint)
        {
            IsAttackable = false;
            Point = spoint;
            LinkTable lt = null;
            if (Point.Role == "Gate" && !DataProvider.Instance.NpcLinkTable.TryGetValue(spoint.RoleArg0, out lt))
            {
                Log.WriteLine(LogLevel.Warn, "Could not load LinkTable for NPC {0} LT {1}", Point.MobName,
                    Point.RoleArg0);
            }

            Gate = lt;

            ID = DataProvider.Instance.GetMobIDFromName(Point.MobName);
            Position = new Vector2(spoint.CoordX, spoint.CoordY);
            if (spoint.Direct < 0)
            {
                Rotation = (byte) ((360 + spoint.Direct) / 2);
            }
            else
            {
                Rotation = (byte) (spoint.Direct / 2);
            }
        }

        public ushort ID { get; private set; }
        public LinkTable Gate { get; private set; }

        public override void Update(DateTime date)
        {
            //just for the fun of it?
        }

        public override Packet Spawn()
        {
            var packet = new Packet(SH7Type.SpawnSingleObject);
            Write(packet);
            return packet;
        }

        public void Write(Packet packet)
        {
            packet.WriteUShort(MapObjectID);
            packet.WriteByte(2); //always 2 (type i bet shown / transparent?) -> test it
            packet.WriteUShort(ID);
            packet.WriteInt(Position.X);
            packet.WriteInt(Position.Y);
            packet.WriteByte(Rotation);
            if (Gate != null)
            {
                packet.WriteByte(1);
                packet.WriteString(Gate.MapClient, 12);
                packet.Fill(43, 0);
            }
            else
            {
                packet.Fill(56, 0); //find out later
            }
        }
    }
}