﻿/*File for this file Basic Copyright 2012 no0dl */

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Estrella.FiestaLib.Networking;
using Estrella.Zone.Game.Guild.Academy;
using MySql.Data.MySqlClient;

namespace Estrella.Zone.Game.Guild
{
    public sealed class Guild
    {
        private byte[] _Password;


        public Guild(MySqlDataReader reader, MySqlConnection con)
        {
            ID = reader.GetInt32("ID");
            Name = reader.GetString("GuildName");
            // _Password = (byte[])reader.GetValue("Password");
            _Password = new byte[12];
            AllowGuildWar = reader.GetBoolean("AllowGuildWar");
            Message = reader.GetString("GuildMessage");
            MessageCreateTime = reader.GetDateTime(8);
            MessageCreaterID = reader.GetInt32("CreaterID");
            CreateTime = DateTime.Now; //read later
            GuildMoney = reader.GetInt64("GuildMoney");

            Members = new List<GuildMember>();
            ThreadLocker = new object();
            GuildStore = new GuildStorage(this);
            Load();
        }

        public int ID { get; private set; }
        public string Name { get; set; }
        public long GuildMoney { get; set; }
        public GuildStorage GuildStore { get; set; }

        public string Password
        {
            get
            {
                var data = _Password;
                // InterCrypto.DecryptData(ref data, 0, data.Length);

                return Encoding.UTF8.GetString(data);
            }
            set
            {
                var data = Encoding.UTF8.GetBytes(value);
                //  InterCrypto.EncryptData(ref data, 0, data.Length);

                _Password = data;
            }
        }

        public bool AllowGuildWar { get; set; }
        public string Message { get; set; }
        public int MessageCreaterID { get; set; }
        public DateTime MessageCreateTime { get; set; }
        public DateTime CreateTime { get; private set; }


        public List<GuildMember> Members { get; private set; }

        public GuildAcademy Academy { get; private set; }


        public object ThreadLocker { get; private set; }

        public void GuildMoneySave()
        {
            var con = Program.CharDBManager.GetClient().GetConnection();
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
                    cmd.CommandText = "GuildMoney_Save";
                    cmd.Parameters.Add(new MySqlParameter("@pGuildID", ID));
                    cmd.Parameters.Add(new MySqlParameter("@pGuildMoney", GuildMoney));
                    cmd.ExecuteNonQuery();
                }

                if (conCreated)
                {
                    con.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Name = null;
            _Password = null;

            Message = null;


            Members.ForEach(m => m.Dispose());
            Members.Clear();
            Members = null;

            Academy.Dispose();
            Academy = null;

            ThreadLocker = null;
        }


        private void Load()
        {
            //members
            using (var cmd = Program.CharDBManager.GetClient().GetConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM GuildMembers WHERE GuildID = @pGuildID";

                cmd.Parameters.Add(new MySqlParameter("@pGuildID", ID));


                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var member = new GuildMember(this, reader);

                        Members.Add(member);
                    }
                }
            }

            //academy
            Academy = new GuildAcademy(this);
        }


        public void Broadcast(Packet Packet, GuildMember Exclude = null)
        {
            lock (ThreadLocker)
            {
                foreach (var member in Members)
                {
                    if (Exclude != null
                        && member == Exclude)
                        continue;


                    if (member.IsOnline)
                    {
                        try
                        {
                            member.Character.Client.SendPacket(Packet);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        public bool GetMember(int CharacterID, out GuildMember Member)
        {
            lock (ThreadLocker)
            {
                Member = Members.Find(m => m.CharacterID.Equals(CharacterID));
            }

            return Member != null;
        }
    }
}