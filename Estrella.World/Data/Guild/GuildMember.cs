/*File for this file Basic Copyright 2012 no0dl */

using System.Data;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.World.Managers;
using MySql.Data.MySqlClient;

namespace Estrella.World.Data.Guild
{
    public sealed class GuildMember
    {
        private object ThreadLocker;


        public GuildMember(Guild Guild, WorldCharacter Character, GuildRank Rank, ushort Corp)
        {
            this.Guild = Guild;
            this.Character = Character;

            this.Rank = Rank;
            this.Corp = Corp;


            ThreadLocker = new object();
        }

        public Guild Guild { get; private set; }
        public WorldCharacter Character { get; private set; }


        public GuildRank Rank { get; set; }
        public ushort Corp { get; set; }

        public void Dispose()
        {
            Guild = null;
            Character = null;
        }


        public void Save(MySqlConnection con = null)
        {
            lock (ThreadLocker)
            {
                var conCreated = con == null;
                if (conCreated)
                {
                    con = Program.DatabaseManager.GetClient().GetConnection();
                }


                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GuildMember_Save";

                    cmd.Parameters.Add(new MySqlParameter("@pGuildID", Guild.ID));
                    cmd.Parameters.Add(new MySqlParameter("@pCharacterID", Character.ID));

                    cmd.Parameters.Add(new MySqlParameter("@pRank", (byte) Rank));
                    cmd.Parameters.Add(new MySqlParameter("@pCorp", (short) Corp));


                    cmd.ExecuteNonQuery();
                }


                if (conCreated)
                {
                    con.Dispose();
                }
            }
        }


        public void WriteInfo(Packet Packet)
        {
            Packet.WriteString(Character.Character.Name, 16);
            Packet.WriteByte((byte) Rank);
            Packet.WriteInt(0); //unk ?

            Packet.WriteUShort(Corp);
            Packet.WriteByte(0);
            Packet.WriteUShort(0xFFFF); //unk
            Packet.WriteUShort(0xFFFF); //unk
            Packet.WriteByte(0);
            Packet.WriteInt(32);
            Packet.WriteInt(32);
            Packet.Fill(50, 0x00); // unk
            Packet.WriteByte((byte) (Character.IsOnline ? 0xB9 : 0x00));
            Packet.Fill(3, 0x00); // unk
            Packet.WriteByte(Character.Character.Job);
            Packet.WriteByte(Character.Character.CharLevel);
            Packet.WriteByte(0);
            Packet.WriteString(DataProvider.GetMapname(Character.Character.PositionInfo.Map), 12);
        }

        public void BroadcastGuildName()
        {
            var packet = new Packet(SH29Type.GuildNameResult);
            packet.WriteInt(Guild.ID);
            packet.WriteString(Guild.Name, 16);

            BroadcastManager.Instance.BroadcastInRange(Character, packet, false);
        }
    }
}