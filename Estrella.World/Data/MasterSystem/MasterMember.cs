using System;
using System.Data;
using System.Globalization;
using Estrella.Database.DataStore;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.World.Managers;
using Estrella.World.Networking;

namespace Estrella.World.Data.MasterSystem
{
    public class MasterMember
    {
        public WorldClient pMember { get; private set; }
        public string pMemberName { get; private set; }
        public DateTime RegisterDate { get; private set; }

        public bool IsOnline { get; private set; }
        public bool IsMaster { get; set; }
        public int CharID { get; set; }
        public int MasterID { get; set; }
        public byte Level { get; private set; }

        public MasterMember()
        {
        }

        public MasterMember(WorldClient pClient, int MasterCharID)
        {
            MasterID = MasterCharID;
            IsOnline = true;
            CharID = pClient.Character.ID;
            Level = pClient.Character.Character.CharLevel;
            RegisterDate = DateTime.Now;
            pMemberName = pClient.Character.Character.Name;
            pMember = pClient;
        }

        public static MasterMember LoadFromDatabase(DataRow row)
        {
            var Member = new MasterMember
            {
                pMemberName = row["MemberName"].ToString(),
                CharID = GetDataTypes.GetInt(row["CharID"]),
                Level = GetDataTypes.GetByte(row["Level"]),
                IsMaster = GetDataTypes.GetBool(row["isMaster"]),
                MasterID = GetDataTypes.GetInt(row["MasterID"]),
                RegisterDate = DateTime.ParseExact(row["RegisterDate"].ToString(), "dd.MM.yyyy HH:mm:ss",
                    CultureInfo.InvariantCulture)
            };
            Member.pMember = ClientManager.Instance.GetClientByCharname(Member.pMemberName);
            Member.IsOnline = ClientManager.Instance.IsOnline(Member.pMemberName);
            return Member;
        }

        public void AddToDatabase()
        {
            Program.DatabaseManager.GetClient().ExecuteQuery(
                "INSERT INTO Masters (CharID,MasterID,MemberName,Level,RegisterDate,isMaster) VALUES ('" +
                MasterID + "','" + CharID + "','" + pMemberName + "','" + Level + "','" +
                RegisterDate.ToString("yyyy-MM-dd hh:mm") + "','" + Convert.ToByte(IsMaster) + "')");
        }

        public void RemoveFromDatabase()
        {
            Program.DatabaseManager.GetClient()
                .ExecuteQuery("DELETE FROM Masters WHERE CharID ='" + CharID + "' AND MasterID ='" +
                              MasterID + "'");
        }

        public void RemoveFromDatabase(int MasterID, string Charname)
        {
            Program.DatabaseManager.GetClient()
                .ExecuteQuery(
                    "DELETE FROM Masters WHERE CharID ='" + MasterID + "' AND MasterID ='" + CharID + "'");
        }

        public static void UpdateLevel(byte level, string charame)
        {
            Program.DatabaseManager.GetClient()
                .ExecuteQuery("UPDATE  Masters SET Level='" + level + "'WHERE binary `MemberName` ='" + charame + "'");
        }

        public void SetMemberStatus(bool Status, string name)
        {
            if (Status)
            {
                SetOnline(name);
            }
            else
            {
                SetOffline(name);
            }
        }

        private void SetOffline(string name)
        {
            IsOnline = false;

            using (var packet = new Packet(SH37Type.SendMasterMemberOffline))
            {
                packet.WriteString(name, 16);
                pMember.SendPacket(packet);
            }
        }

        private void SetOnline(string name)
        {
            IsOnline = true;

            using (var packet = new Packet(SH37Type.SendMasterMemberOnline))
            {
                packet.WriteString(name, 16);
                pMember.SendPacket(packet);
            }
        }
    }
}