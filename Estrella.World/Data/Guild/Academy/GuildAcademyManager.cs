/*File for this file Basic Copyright 2012 no0dl */

using System;
using System.Collections.Generic;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.InterLib.Networking;
using Estrella.Util;
using Estrella.World.Handlers;
using Estrella.World.Managers;
using Estrella.World.Networking;

/*using Fiesta.Core.Networking;
using Fiesta.World.Data.Characters;
using Fiesta.World.Data.Guilds;
using Fiesta.World.Game.Characters;
using Fiesta.World.Game.Buffs;
using Fiesta.World.Game.Zones;
using Fiesta.World.Networking;
using Fiesta.World.Networking.Helpers;*/

namespace Estrella.World.Data.Guild.Academy
{
    [ServerModule(InitializationStage.Clients)]
    public static class GuildAcademyManager
    {
        [InitializerMethod]
        public static bool OnAppStart()
        {
            CharacterManager.OnCharacterLogin += On_CharacterManager_CharacterLogin;
            CharacterManager.OnCharacterLogout += On_CharacterManager_CharacterLogout;
            CharacterManager.OnCharacterLevelUp += On_CharacterManager_CharacterLevelUp;
            return true;
        }

        private static void On_CharacterManager_CharacterLogin(WorldCharacter character)
        {
            if (character.IsInGuildAcademy)
            {
                using (var packet = new Packet(SH38Type.AcademyMemberLoggedIn))
                {
                    packet.WriteString(character.Character.Name, 16);


                    character.Guild.Broadcast(packet);
                    character.GuildAcademy.Broadcast(packet);
                }

                using (var packet = new InterPacket(InterHeader.ZoneAcademyMemberOnline))
                {
                    packet.WriteInt(character.Guild.ID);
                    packet.WriteInt(character.ID);


                    ZoneManager.Broadcast(packet);
                }
            }
        }

        private static void On_CharacterManager_CharacterLogout(WorldCharacter character)
        {
            if (character.IsInGuildAcademy)
            {
                using (var packet = new Packet(SH38Type.AcademyMemberLoggedOut))
                {
                    packet.WriteString(character.Character.Name, 16);


                    character.Guild.Broadcast(packet);
                    character.GuildAcademy.Broadcast(packet);
                }

                using (var packet = new InterPacket(InterHeader.ZoneAcademyMemberOffline))
                {
                    packet.WriteInt(character.Guild.ID);
                    packet.WriteInt(character.ID);


                    ZoneManager.Broadcast(packet);
                }
            }
        }

        private static void On_CharacterManager_CharacterLevelUp(WorldCharacter character)
        {
            //fix later
            if (character.IsInGuildAcademy)
            {
                using (var packet = new Packet(SH38Type.AcademyMemberLevelUp))
                {
                    packet.WriteString(character.Character.Name, 16);
                    packet.WriteByte(character.Character.CharLevel);


                    character.Guild.Broadcast(packet);
                    character.GuildAcademy.Broadcast(packet);
                }


                lock (character.Guild.ThreadLocker)
                {
                    uint points;
                    if (GuildDataProvider.Instance.AcademyLevelUpPoints.TryGetValue(character.Character.CharLevel,
                        out points))
                    {
                        character.GuildAcademy.Points += (ushort) points;
                    }


                    //add time to guild buff
                    var time = Program.CurrentTime;
                    //var newTime = Math.Min(CharacterDataProvider.ChrCommon.GuildBuffMaxTime.TotalSeconds, (CharacterDataProvider.ChrCommon.GuildBuffAddTime.TotalSeconds + Character.GuildAcademy.GuildBuffKeepTime.TotalSeconds));
                    //Character.GuildAcademy.GuildBuffKeepTime = TimeSpan.FromSeconds(newTime);

                    //update guild buff to all guild/aka members
                    var toUpdate = new List<WorldCharacter>();
                    foreach (var member in character.GuildAcademy.Members)
                    {
                        if (member.Character.IsOnline)
                        {
                            toUpdate.Add(member.Character);
                        }
                    }

                    foreach (var member in character.Guild.Members)
                    {
                        if (member.Character.IsOnline
                            && !toUpdate.Contains(member.Character))
                        {
                            toUpdate.Add(member.Character);
                        }
                    }

                    //BuffManager.SetBuff(GuildDataProvider.AcademyBuff, GuildDataProvider.AcademyBuffStrength, (uint)(newTime * 1000), toUpdate.ToArray());

                    toUpdate.Clear();
                    toUpdate = null;

                    //update guild buff to zones
                    using (var packet = new InterPacket(InterHeader.ZoneAcademyBuffUpdate))
                    {
                        packet.WriteInt(character.Guild.ID);
                        packet.WriteDateTime(time);
                        packet.WriteDouble(900); //fix later


                        ZoneManager.Broadcast(packet);
                    }


                    //broadcast info and save guild
                    character.GuildAcademy.BroadcastInfo();
                    character.GuildAcademy.Save();
                }
            }
        }


        #region Game Client Handlers

        [PacketHandler(CH38Type.GetAcademyList)]
        public static void On_GameClient_GetAcademyList(WorldClient client, Packet pPacket)
        {
            if (client.Character == null)
            {
                return;
            }


            const int guildsPerPacket = 54;
            lock (GuildManager.ThreadLocker)
            {
                using (var con = Program.DatabaseManager.GetClient().GetConnection())
                {
                    //get guild count
                    int guildCount;
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Guilds";


                        guildCount = Convert.ToInt32(cmd.ExecuteScalar());
                    }


                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ID FROM Guilds";


                        Packet listPacket = null;
                        var count = 0;
                        var globalCount = 0;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (listPacket == null)
                                {
                                    listPacket = new Packet(SH38Type.SendAcademyList);
                                    listPacket.WriteUShort(6312);
                                    listPacket.WriteByte(1);
                                    listPacket.WriteUShort((ushort) guildCount);
                                    listPacket.WriteUShort(0);
                                    listPacket.WriteUShort((ushort) Math.Min(guildsPerPacket,
                                        guildCount - globalCount));
                                    listPacket.WriteUShort(0);
                                }


                                Guild guild;
                                if (GuildManager.GetGuildByID(reader.GetInt32("ID"), out guild))
                                {
                                    //write packet
                                    listPacket.WriteString(guild.Name, 16);
                                    listPacket.WriteString(guild.Master.Character.Character.Name, 16);
                                    listPacket.WriteUShort((ushort) guild.Members.Count);
                                    listPacket.WriteUShort((ushort) guild.Academy.Members.Count);
                                    listPacket.WriteUShort(guild.Academy.Points); // Graduates
                                }
                                else
                                {
                                    pPacket.Fill(38, 0); // guild get error
                                }


                                globalCount++;
                                count++;
                                if (count >= Math.Min(guildsPerPacket, guildCount - globalCount))
                                {
                                    //send packet
                                    client.SendPacket(listPacket);

                                    listPacket.Dispose();
                                    listPacket = null;


                                    //reset
                                    count = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        [PacketHandler(CH38Type.GetAcademyMemberList)]
        public static void On_GameClient_GetAcademyMemberList(WorldClient client, Packet packet)
        {
            if (client.Character == null)
            {
                return;
            }


            if (client.Character.IsInGuildAcademy)
            {
                client.Character.GuildAcademy.SendMemberList(client);
            }
            else if (client.Character.IsInGuild)
            {
                client.Character.Guild.Academy.SendMemberList(client);
            }
        }

        [PacketHandler(CH38Type.JoinAcademy)]
        public static void On_GameClient_JoinAcademy(WorldClient client, Packet packet)
        {
            string guildName;
            if (!packet.TryReadString(out guildName, 16))
            {
                return;
            }


            Guild guild;
            if (!GuildManager.GetGuildByName(guildName, out guild))
            {
                Handler38.SendAcademyResponse(client, guildName, GuildAcademyResponse.AcademyNotFound);
                return;
            }

            guild.Academy.AddMember(client.Character, GuildAcademyRank.Member);
        }

        [PacketHandler(CH38Type.BlockAcademyChat)]
        public static void GuildAcademyChatBlock(WorldClient client, Packet packet)
        {
            if (!client.Character.IsInGuildAcademy)
                return;
            string blockname;
            if (!packet.TryReadString(out blockname, 16))
                return;
            var pMember =
                client.Character.GuildAcademy.Members.Find(m => m.Character.Character.Name == blockname);
            if (pMember == null)
                return;
            pMember.IsChatBlocked = true;
            pMember.Save(Program.DatabaseManager.GetClient().GetConnection());
            using (var pack = new Packet(SH38Type.AcademyChatBlockResponse))
            {
                pack.WriteString(client.Character.Character.Name, 16);
                pack.WriteString(blockname, 16);
                client.Character.GuildAcademy.Guild.Broadcast(pack);
                client.Character.Guild.Broadcast(pack);
            }
        }

        [PacketHandler(CH38Type.ChangeRequestToGuild)]
        public static void ChangeFromGuildAcademyToResponse(WorldClient client, Packet packet)
        {
            string guildName;
            string requestName;
            bool answer;
            if (!packet.TryReadString(out guildName, 16) || !packet.TryReadString(out requestName, 16) || !packet.TryReadBool(out answer) || !client.Character.IsInGuildAcademy)
                return;
            if (answer)
            {
                var pMember =
                    client.Character.GuildAcademy.Members.Find(m => m.Character.Character.Name == requestName);
                if (pMember == null)
                    return;
                pMember.Character.IsInGuildAcademy = false;
                pMember.Academy.RemoveMember(pMember);
                pMember.Character.GuildAcademy.Guild.AddMember(pMember.Character, GuildRank.Member,
                    Program.DatabaseManager.GetClient().GetConnection(), true, true);

                pMember.Character.Guild = pMember.Character.GuildAcademy.Guild;
                pMember.Character.IsInGuild = true;
                using (var pack = new Packet(SH38Type.SendJoinGuildFromAcademy))
                {
                    //this packet remove character from academy List and added to GuildList
                    pack.WriteString(requestName, 16);
                    pack.WriteString(client.Character.Character.Name, 16);
                    pack.WriteByte(6); //rank
                    pack.WriteInt(0); //unk
                    pack.WriteUShort(0); //korp
                    pack.Fill(64, 0x00); //unk
                    pack.WriteByte(true ? (byte) 0x95 : (byte) 0x00); // (this.isOnline ? (byte)0x95 : (byte)0x00);
                    pack.Fill(3, 0x00); //unk
                    pack.WriteByte(pMember.Character.Character.Job);
                    pack.WriteByte(pMember.Character.Character.CharLevel);
                    pack.Fill(13, 0x00); //unk
                    client.Character.GuildAcademy.Guild.Broadcast(pack);
                    client.Character.GuildAcademy.Broadcast(pack);
                }
            }

            using (var p2 = new Packet(SH4Type.CharacterGuildinfo))
            {
                client.Character.Guild.WriteGuildInfo(packet);
                client.SendPacket(p2);
            }

            using (var pack = new Packet(SH29Type.GuildMemberJoined))
            {
                pack.WriteString(client.Character.Character.Name, 16);
                client.Character.GuildAcademy.Guild.Broadcast(pack);
                client.Character.GuildAcademy.Broadcast(pack);
            }

            using (var pack = new Packet(SH29Type.ChangeResponse))
            {
                pack.WriteUShort(3137); //unk
                pack.WriteByte(3);
                pack.Fill(2, 0x00); //unk
                client.SendPacket(pack);
            }
        }

        [PacketHandler(CH38Type.JumpToMember)]
        public static void JumpToMember(WorldClient client, Packet packet)
        {
            string pMemberName;
            if (!packet.TryReadString(out pMemberName, 16))
                return;

            if (!client.Character.IsInGuildAcademy)
                return;
            var pMember =
                client.Character.GuildAcademy.Members.Find(m => m.Character.Character.Name == pMemberName);
            if (pMember != null)
            {
                int oldmap = client.Character.Character.PositionInfo.Map;
                client.Character.Character.PositionInfo.Map = pMember.Character.Character.PositionInfo.Map;
                client.Character.Character.PositionInfo.XPos = pMember.Character.Character.PositionInfo.XPos;
                client.Character.Character.PositionInfo.YPos = pMember.Character.Character.PositionInfo.YPos;
                client.Character.ChangeMap(oldmap);
            }
        }

        [PacketHandler(CH38Type.UpdateDetails)]
        public static void On_GameClient_UpdateDetails(WorldClient client, Packet packet)
        {
            ushort lenght;
            string message;
            if (!packet.TryReadUShort(out lenght))
                return;

            if (!packet.TryReadString(out message, lenght))
                return;
            using (var pack = new Packet(SH38Type.SendChangeDetailsResponse))
            {
                pack.WriteUShort(6016); //code for ok
                client.SendPacket(pack);
            }

            if (client.Character.Guild != null)
            {
                client.Character.Guild.Academy.Message = message;
                client.Character.Guild.Academy.Save();
                using (var pack = new Packet(SH38Type.SendChangeDetails))
                {
                    pack.WriteUShort(lenght);
                    pack.WriteString(message, message.Length);
                    client.Character.Guild.Broadcast(pack);
                    client.Character.Guild.Academy.Broadcast(pack);
                }
            }
            else if (client.Character.GuildAcademy != null)
            {
                client.Character.GuildAcademy.Message = message;
                client.Character.GuildAcademy.Save();
                using (var pack = new Packet(SH38Type.SendChangeDetails))
                {
                    pack.WriteUShort(lenght);
                    pack.WriteString(message, message.Length);
                    client.Character.GuildAcademy.Broadcast(pack);
                }
            }
        }

        [PacketHandler(CH38Type.LeaveAcademy)]
        public static void On_GameClient_LeaveAcademy(WorldClient client, Packet packet)
        {
            if (client.Character == null)
            {
                return;
            }


            if (client.Character.IsInGuildAcademy)
            {
                client.Character.GuildAcademy.RemoveMember(client.Character.GuildAcademyMember);
            }
        }


        [PacketHandler(CH38Type.AcademyChat)]
        public static void On_GameClient_AcademyChat(WorldClient client, Packet packet)
        {
            if (!packet.TryReadByte(out var len)
                || !packet.TryReadString(out var msg, len))
            {
                return;
            }


            if (client.Character.IsInGuildAcademy
                || client.Character.IsInGuild)
            {
                if (client.Character.IsInGuildAcademy
                    && client.Character.GuildAcademyMember.IsChatBlocked)
                {
                    using (var packet1 = new Packet(SH38Type.AcademyChatBlocked))
                    {
                        packet1.WriteUShort(6140);
                        client.SendPacket(packet1);
                    }

                    return;
                }


                using (var packet2 = new Packet(SH38Type.AcademyChat))
                {
                    packet2.WriteInt(client.Character.Guild.ID);
                    packet2.WriteString(client.Character.Character.Name, 16);
                    packet2.WriteByte(len);
                    packet2.WriteString(msg, len);

                    client.Character.Guild.Broadcast(packet2);
                    client.Character.GuildAcademy.Broadcast(packet2);
                }
            }
        }

        #endregion
    }
}