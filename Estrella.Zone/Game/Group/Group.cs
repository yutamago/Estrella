using System;
using System.Collections.Generic;
using System.Linq;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.InterLib.Networking;
using Estrella.Zone.InterServer;
using Estrella.Zone.Managers;
using MySql.Data.MySqlClient;

namespace Estrella.Zone.Game.Group
{
    public class Group
    {
        public Group()
        {
            Members = new List<GroupMember>();
            LastUpdate = DateTime.Now;
        }

        public long Id { get; private set; }

        public GroupMember Master
        {
            get { return Members.Single(m => m.IsMaster); }
        }

        public IEnumerable<GroupMember> NormalMembers
        {
            get { return Members.Where(m => !m.IsMaster); }
        }

        public DateTime LastUpdate { get; private set; }

        public readonly List<GroupMember> Members;

        public static Group LoadGroupFromDatabaseById(long pId)
        {
            //--------------------------------------------------
            // Queries used in this function
            //--------------------------------------------------
            const string read_group_query =
                "USE `fiesta_world`; " +
                "SELECT `Id`, `Member1`, `Member2`, `Member3`, `Member4`, `Member5` " +
                "FROM `groups` " +
                "WHERE Id = {0}";

            //--------------------------------------------------
            // Reading the group out of the database
            //--------------------------------------------------

            var grp = new Group();
            grp.Id = pId;

            using (var client = Program.DatabaseManager.GetClient())
            {
                var query = string.Format(read_group_query, pId);
                using (var cmd = new MySqlCommand(query, client.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (var i = 1; i < 6; i++)
                        {
                            if (!reader.IsDBNull(i))
                                grp.Members.Add(ReadGroupMemberFromDatabase(reader.GetInt64(i)));
                        }
                    }
                }
            }

            return grp;
        }

        public void AddMember(ZoneCharacter pCharacter, bool pIsMaster)
        {
            var mem = new GroupMember();
            mem.Character = pCharacter;
            mem.Group = this;
            mem.IsMaster = pIsMaster;
            mem.IsOnline = true;
            mem.Name = pCharacter.Name;
            pCharacter.GroupMember = mem;
            pCharacter.LevelUp += OnCharacterLevelUp;
            pCharacter.Group = this;
            pCharacter.GroupMember = mem;
            Members.Add(mem);
        }

        public void AddMember(string pName, bool pIsMaster = false)
        {
            var mem = new GroupMember();
            mem.Character = null;
            mem.Group = this;
            mem.IsMaster = pIsMaster;
            mem.IsOnline = false;
            mem.Name = pName;

            if (ClientManager.Instance.HasClient(pName))
            {
                mem.IsOnline = true;
                var client = ClientManager.Instance.GetClientByCharName(pName);
                mem.Character = client.Character;
                mem.Character.LevelUp += OnCharacterLevelUp;
                mem.Character.Group = this;
                mem.Character.GroupMember = mem;
            }

            Members.Add(mem);
        }

        public void Update()
        {
            /* Note									    *
             * Add more update logic here if needed.	*
             * this will automatically repeated.		*/
            UpdateGroupPositions();
            UpdateGroupStats();

            LastUpdate = DateTime.Now;
        }

        public void UpdateCharacterLevel(ZoneCharacter pChar)
        {
            using (var packet = new Packet(SH14Type.SetMemberStats))
            {
                packet.WriteByte(0x01); // UNK
                packet.WriteString(pChar.Name, 16);
                packet.WriteByte((byte) pChar.Job);
                packet.WriteByte(pChar.Level);
                packet.WriteUInt(pChar.MaxHP);
                packet.WriteUInt(pChar.MaxSP);
                packet.WriteByte(0x01); // UNK

                AnnouncePacket(packet);
            }
        }

        public void UpdateCharacterHpSp(ZoneCharacter pChar)
        {
            using (var packet = new Packet(SH14Type.UpdatePartyMemberStats))
            {
                packet.WriteByte(0x01); // UNK
                packet.WriteString(pChar.Name, 16);
                packet.WriteUInt(pChar.HP);
                packet.WriteUInt(pChar.SP);

                AnnouncePacketToUpdatable(packet);
            }

            using (var packet = new Packet(SH14Type.SetMemberStats))
            {
                packet.WriteByte(1); // UNK
                packet.WriteString(pChar.Name, 16);
                packet.WriteByte((byte) pChar.Job);
                packet.WriteByte(pChar.Level);
                packet.WriteUInt(pChar.MaxHP);
                packet.WriteUInt(pChar.MaxSP);
                packet.WriteByte(0x00); // UNK
            }
        }

        public void UpdateGroupStats()
        {
            foreach (var m in Members.Where(m => m.Character != null).Where(m => m.IsReadyForUpdates)
                .Select(m => m.Character))
            {
                UpdateCharacterHpSp(m);
            }
        }

        public void UpdateGroupPositions()
        {
            foreach (var m in Members.Where(mem => mem.IsOnline).Where(m => m.IsReadyForUpdates))
            {
                UpdateMemberPosition(m);
            }
        }

        public static GroupMember ReadGroupMemberFromDatabase(long pCharId)
        {
            //--------------------------------------------------
            // Quries used in this function
            //--------------------------------------------------
            const string get_groupmem_query =
                "SELECT `Name`, `IsGroupMaster` " +
                "FROM `fiesta_world`.`characters` " +
                "WHERE `CharID` = '{0}'";

            //--------------------------------------------------
            // Read member from database.
            //--------------------------------------------------
            var name = "";
            var isOnline = false;
            var isMaster = false;

            using (var client = Program.DatabaseManager.GetClient())
            using (var cmd = new MySqlCommand(string.Format(get_groupmem_query, pCharId), client.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    name = reader.GetString("Name");
                    if (reader.IsDBNull(reader.GetOrdinal("IsGroupMaster")))
                        isMaster = false;
                    else
                        isMaster = reader.GetBoolean("IsGroupMaster");
                }
            }

            var member = new GroupMember(name, isMaster, isOnline);
            if (ClientManager.Instance.HasClient(name))
            {
                var client = ClientManager.Instance.GetClientByCharName(name);
                member.IsOnline = true;
                member.Character = client.Character;
            }
            else
                member.IsOnline = (bool) InterFunctionCallbackProvider.Instance.QueuePacket(id =>
                {
                    var packet = new InterPacket(InterHeader.FunctionCharIsOnline);
                    packet.WriteLong(id);
                    packet.WriteString(name, 16);
                    return packet;
                }, packet =>
                {
                    var value = false;
                    packet.TryReadBool(out value);
                    return value;
                });

            return member;
        }

        internal void RemoveMember(string name)
        {
            var client = ClientManager.Instance.GetClientByCharName(name);
            var chara = client.Character;

            chara.Group = null;
            Members.Remove(chara.GroupMember);
            chara.GroupMember = null;

            // Forced update.
            Update();
        }

        internal void CharacterMoved(GroupMember groupMember, int oldx, int oldy, int newx, int newy)
        {
            using (var packet = new Packet(SH14Type.UpdatePartyMemberLoc))
            {
                packet.WriteByte(1); // 		unk
                packet.WriteString(groupMember.Name, 16);
                packet.WriteInt(newx);
                packet.WriteInt(newy);
                AnnouncePacket(packet);
            }
        }

        private void AnnouncePacket(Packet pPacket)
        {
            foreach (var mem in Members)
            {
                mem.Character.Client.SendPacket(pPacket);
            }
        }

        private void AnnouncePacketToUpdatable(Packet pPacket)
        {
            foreach (var member in Members.Where(m => m.IsOnline && m.IsReadyForUpdates))
            {
                member.Character.Client.SendPacket(pPacket);
            }
        }

        private void UpdateMemberPosition(GroupMember member)
        {
            if (!member.IsOnline)
                return;
            using (var packet = new Packet(SH14Type.UpdatePartyMemberLoc))
            {
                packet.WriteString(member.Name, 0x10);
                packet.WriteInt(member.Character.Position.X);
                packet.WriteInt(member.Character.Position.Y);

                AnnouncePacketToUpdatable(packet);
            }
        }

        private void OnCharacterLevelUp(object sender, LevelUpEventArgs args)
        {
            UpdateCharacterLevel((ZoneCharacter) sender);
            UpdateCharacterHpSp((ZoneCharacter) sender);
        }
    }
}