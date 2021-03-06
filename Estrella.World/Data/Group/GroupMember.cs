﻿using Estrella.World.Managers;
using Estrella.World.Networking;
using MySql.Data.MySqlClient;

namespace Estrella.World.Data.Group
{
    public class GroupMember
    {
        private GroupMember()
        {
        }

        public GroupMember(WorldClient client, GroupRole role)
        {
            Client = client;
            Character = client.Character;
            CharId = client.Character.ID;
            Role = role;
            Name = client.Character.Character.Name;
            IsOnline = true;
        }

        public WorldCharacter Character { get; private set; }
        public string Name { get; set; }
        public Group Group { get; internal set; }
        public GroupRole Role { get; internal set; }
        public WorldClient Client { get; private set; }
        public int CharId { get; private set; }
        public bool IsOnline { get; set; }

        public override int GetHashCode()
        {
            return CharId;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GroupMember))
                return false;
            return ((GroupMember) obj).Name == Name;
        }

        public static GroupMember LoadFromDatabase(ushort pCharId)
        {
            const string query = "SELECT * FROM `characters` WHERE CharId = @cid";
            var member = new GroupMember();

            using (var con = Program.DatabaseManager.GetClient())
            using (var cmd = new MySqlCommand(query, con.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@cid", pCharId);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        member.Name = rdr.GetString("Name");
                        member.IsOnline = ClientManager.Instance.IsOnline(member.Name);
                        member.Role = rdr.GetBoolean("IsGroupMaster")
                            ? GroupRole.Master
                            : GroupRole.Member;
                        if (member.IsOnline)
                        {
                            member.Client = ClientManager.Instance.GetClientByCharname(member.Name);
                            member.Character = member.Client.Character;
                        }

                        return member;
                    }
                }
            }

            return member;
        }
    }
}