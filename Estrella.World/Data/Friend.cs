using System.Data;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.World.Managers;
using Estrella.World.Networking;

namespace Estrella.World.Data
{
    public class Friend
    {
        public uint UniqueID { get; set; }
        public int ID { get; private set; }
        public string Name { get; private set; }
        public byte Level { get; private set; }
        public byte Job { get; private set; }
        public string Map { get; private set; }
        public bool IsOnline { get; set; }
        public byte Month { get; private set; }
        public byte Day { get; private set; }
        public WorldClient client { get; private set; }

        public static Friend Create(WorldCharacter pCharacter)
        {
            var friend = new Friend
            {
                ID = pCharacter.Character.ID,
                Name = pCharacter.Character.Name,
                Level = pCharacter.Character.CharLevel,
                Job = pCharacter.Character.Job,
                Map = GetMapname(pCharacter.Character.PositionInfo.Map),
                UniqueID = (uint) pCharacter.Character.AccountID,
                IsOnline = pCharacter.IsIngame,
                client = pCharacter.Client
            };

            return friend;
        }

        private static string GetMapname(ushort mapid)
        {
            MapInfo mapinfo;
            if (DataProvider.Instance.Maps.TryGetValue(mapid, out mapinfo))
            {
                return mapinfo.ShortName;
            }

            return "";
        }

        public static Friend LoadFromDatabase(DataRow row)
        {
            var friend = new Friend
            {
                UniqueID = uint.Parse(row["CharID"].ToString()),
                ID = int.Parse(row["FriendID"].ToString()),
                Day = byte.Parse(row["LastConnectDay"].ToString()),
                Month = byte.Parse(row["LastConnectMonth"].ToString())
            };
            return friend;
        }

        /// <summary>
        ///     Updates friend status with input from characters table
        /// </summary>
        /// <param name="pReader"></param>
        public void UpdateFromDatabase(DataRow row)
        {
            //this.UniqueID = uint.Parse(row["CharID"].ToString());
            Name = row["Name"].ToString();
            Job = byte.Parse(row["Job"].ToString());
            Level = byte.Parse(row["Level"].ToString());
            Map = GetMapname(ushort.Parse(row["Map"].ToString()));
        }

        /// <summary>
        ///     Updates friend status using a <see cref="WorldCharacter" /> object.
        /// </summary>
        /// <param name="pCharacter">The WorldCharacter object with the new data.</param>
        public void Update(WorldCharacter pCharacter)
        {
            Map = GetMapname(pCharacter.Character.PositionInfo.Map);
            Job = pCharacter.Character.Job;
            Level = pCharacter.Character.CharLevel;
            IsOnline = ClientManager.Instance.IsOnline(pCharacter.Character.Name);
        }

        public void Offline(WorldClient pClient, string name)
        {
            using (var packet = new Packet(SH21Type.FriendOffline))
            {
                packet.WriteString(name, 16);
                pClient.SendPacket(packet);
            }
        }

        public void Online(WorldClient client, WorldClient target)
        {
            using (var packet = new Packet(SH21Type.FriendOnline))
            {
                packet.WriteString(target.Character.Character.Name, 16);
                packet.WriteString(DataProvider.GetMapname(target.Character.Character.PositionInfo.Map), 12);
                client.SendPacket(packet);
            }
        }

        public void WritePacket(Packet pPacket)
        {
            pPacket.WriteBool(IsOnline); // Logged In
            pPacket.WriteByte(Month); // Last connect Month << 4 (TODO)
            pPacket.WriteByte(Day); // Last connect Day (TODO)
            pPacket.WriteByte(0); // Unknown (TODO)
            pPacket.WriteString(Name, 16);
            pPacket.WriteByte(Job);
            pPacket.WriteByte(Level);
            pPacket.WriteByte(0); // In Party (TODO)
            pPacket.WriteByte(0); // Unkown (TODO)
            pPacket.WriteString(Map, 12);
            pPacket.Fill(32, 0);
        }
    }
}