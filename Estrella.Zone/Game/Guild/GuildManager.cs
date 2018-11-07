/*File for this file Basic Copyright 2012 no0dl */

using System;
using System.Collections.Generic;
using Estrella.InterLib.Networking;
using Estrella.Util;
using Estrella.Zone.InterServer;
using Estrella.Zone.Managers;
using MySql.Data.MySqlClient;

namespace Estrella.Zone.Game.Guild
{
    [ServerModule(InitializationStage.Clients)]
    public static class GuildManager
    {
        private static List<Guild> LoadedGuilds;
        private static object ThreadLocker;


        [InitializerMethod]
        public static bool OnAppStart()
        {
            LoadedGuilds = new List<Guild>();
            ThreadLocker = new object();


            CharacterManager.OnCharacterLogin += LoadGuildID;
            return true;
        }

        private static void On_CharacterManager_CharacterLogin(ZoneCharacter Character)
        {
            SetGuildBuff(Character);
        }

        public static void LoadGuildID(ZoneCharacter pChar)
        {
            Guild g;
            using (var dbclient = Program.CharDBManager.GetClient())
            {
                var AcademyID =
                    dbclient.ReadInt32("SELECT GuildID FROM guildacademymembers WHERE CharID=" + pChar.ID + "");
                var GuildID = dbclient.ReadInt32("SELECT GuildID FROM guildmembers WHERE CharID=" + pChar.ID + "");
                if (GuildID > 0)
                {
                    if (!GetGuildByID(GuildID, out g))
                        return;
                    pChar.Guild = g;

                    pChar.GuildMember = g.Members.Find(m => m.CharacterID == pChar.Character.ID);
                    pChar.Character.GuildID = g.ID;
                }
                else if (AcademyID > 0)
                {
                    if (!GetGuildByID(AcademyID, out g))
                        return;
                    pChar.GuildAcademy = g.Academy;
                    pChar.GuildAcademyMember = g.Academy.Members.Find(m => m.Character.ID == pChar.Character.ID);
                    pChar.Character.AcademyID = g.ID;
                    pChar.IsInaAcademy = true;
                }
            }
        }

        public static void RemoveGuildBuff(ZoneCharacter Character)
        {
            /*  Buff buff;
              if (Character.Buffs.GetBuff(b => b.AbStateInfo.ID.Equals(GuildDataProvider.AcademyBuff.ID), out buff))
              {
                  Character.Buffs.Remove(buff);
              }*/
        }

        public static void SetGuildBuff(ZoneCharacter Character)
        {
            //Later
            //check if character needs guild buff
            /* if (Character.IsInGuild
                 || Character.IsInGuildAcademy)
             {
                 var remainingBuffTime = (Character.GuildAcademy.GuildBuffKeepTime - (ZoneService.Instance.Time - Character.GuildAcademy.GuildBuffUpdateTime)).TotalMilliseconds;
                 if (remainingBuffTime > 0)
                 {
                     Character.Buffs.Add(GuildDataProvider.AcademyBuff, GuildDataProvider.AcademyBuffStrength, (uint)remainingBuffTime);
                 }
             }*/
        }

        public static bool GetGuildByID(int GuildID, out Guild Guild)
        {
            lock (ThreadLocker)
            {
                if ((Guild = LoadedGuilds.Find(g => g.ID.Equals(GuildID))) == null)
                {
                    //load from db
                    using (var con = Program.CharDBManager.GetClient().GetConnection())
                    {
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT * FROM Guilds WHERE ID = @pID";

                            cmd.Parameters.Add(new MySqlParameter("@pID", GuildID));


                            using (var reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                    return false;

                                //create new guild
                                Guild = new Guild(reader, con);


                                //add to cache
                                LoadedGuilds.Add(Guild);
                                reader.Close();
                            }
                        }
                    }
                }
            }

            return Guild != null;
        }


        [InterPacketHandler(InterHeader.ZoneGuildCreated)]
        public static void On_InterClient_GuildCreated(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }


            Guild guild;
            if (GetGuildByID(guildID, out guild))
            {
                //check if character is on local zone, if so assign guild to him
                ZoneCharacter character;
                if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                {
                    character.Guild = guild;
                    character.GuildAcademy = guild.Academy;


                    GuildMember member;
                    if (guild.GetMember(characterID, out member))
                    {
                        member.Character = character;
                        character.GuildMember = member;
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneGuildMemberLogin)]
        public static void On_InterClient_GuildMemberLogin(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }

            Guild guild;
            if (GetGuildByID(guildID, out guild))
            {
                GuildMember member;
                if (guild.GetMember(characterID, out member))
                {
                    member.IsOnline = true;


                    ZoneCharacter character;
                    if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                    {
                        character.Guild = guild;
                        character.GuildAcademy = guild.Academy;
                        character.GuildMember = member;
                        member.Character = character;
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneGuildMemberLogout)]
        public static void On_InterClient_GuildMemberLogout(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }

            Guild guild;
            if (GetGuildByID(guildID, out guild))
            {
                GuildMember member;
                if (guild.GetMember(characterID, out member))
                {
                    member.Character = null;
                    member.IsOnline = false;
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneGuildMessageUpdate)]
        public static void On_InterClient_GuildMessageUpdate(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            DateTime createTime;
            ushort length;
            string message;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID)
                || !pPacket.TryReadDateTime(out createTime)
                || !pPacket.TryReadUShort(out length)
                || !pPacket.TryReadString(out message, length))
            {
                return;
            }


            Guild guild;
            if (GetGuildByID(guildID, out guild))
            {
                //update guild
                guild.Message = message;
                guild.MessageCreateTime = createTime;
                guild.MessageCreaterID = characterID;
            }
        }

        [InterPacketHandler(InterHeader.ZoneGuildMemberAdd)]
        public static void On_InterClient_GuildMemberAdd(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            byte rank;
            ushort corp;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID)
                || !pPacket.TryReadByte(out rank)
                || !pPacket.TryReadUShort(out corp))
            {
                return;
            }

            Guild guild;
            if (GetGuildByID(guildID, out guild))
            {
                lock (guild.ThreadLocker)
                {
                    //create member
                    var member = new GuildMember(guild, characterID, (GuildRank) rank, corp)
                    {
                        IsOnline = true
                    };

                    guild.Members.Add(member);


                    //check if member is on this zone, if so assign guild to him
                    ZoneCharacter character;
                    if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                    {
                        character.Guild = guild;
                        character.GuildAcademy = guild.Academy;
                        character.GuildMember = member;

                        member.Character = character;


                        SetGuildBuff(character);
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneGuildMemberRemove)]
        public static void On_InterClient_GuildMemberRemove(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }


            Guild guild;
            if (GetGuildByID(guildID, out guild))
            {
                lock (guild.ThreadLocker)
                {
                    GuildMember member;
                    if (guild.GetMember(characterID, out member))
                    {
                        //remove member and clean up
                        guild.Members.Remove(member);

                        member.Dispose();


                        //check if member is on this zone
                        ZoneCharacter character;
                        if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                        {
                            character.Guild = null;
                            character.GuildAcademy = null;
                            character.GuildMember = null;


                            RemoveGuildBuff(character);
                        }
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneGuildMemberRankUpdate)]
        public static void On_InterClient_GuildMemberRankUpdate(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            byte newRank;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID)
                || !pPacket.TryReadByte(out newRank))
            {
                return;
            }


            Guild guild;
            if (GetGuildByID(guildID, out guild))
            {
                lock (guild.ThreadLocker)
                {
                    GuildMember member;
                    if (guild.GetMember(characterID, out member))
                    {
                        member.Rank = (GuildRank) newRank;
                    }
                }
            }
        }
    }
}