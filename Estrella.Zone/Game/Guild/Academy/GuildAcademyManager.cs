/*File for this file Basic Copyright 2012 no0dl */

using System;
using Estrella.InterLib.Networking;
using Estrella.Zone.InterServer;
using Estrella.Zone.Managers;

namespace Estrella.Zone.Game.Guild.Academy
{
    public static class GuildAcademyManager
    {
        [InterPacketHandler(InterHeader.ZoneAcademyMemberJoined)]
        public static void On_WorldClient_AcademyMemberJoined(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            DateTime registerDate;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID)
                || !pPacket.TryReadDateTime(out registerDate))
            {
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                var member = new GuildAcademyMember(guild.Academy, characterID, GuildAcademyRank.Member, registerDate)
                {
                    IsOnline = true
                };
                guild.Academy.Members.Add(member);


                ZoneCharacter character;
                if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                {
                    member.Character = character;

                    character.Guild = guild;
                    character.GuildAcademy = guild.Academy;
                    character.GuildAcademyMember = member;


                    GuildManager.SetGuildBuff(character);
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneAcademyMemberLeft)]
        public static void On_WorldClient_AcademyMemberLeft(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                GuildAcademyMember member;
                if (guild.Academy.GetMember(characterID, out member))
                {
                    guild.Academy.Members.Remove(member);
                    member.Dispose();


                    ZoneCharacter character;
                    if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                    {
                        character.Guild = null;
                        character.GuildAcademy = null;
                        character.GuildAcademyMember = null;


                        GuildManager.RemoveGuildBuff(character);
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneAcademyMemberOnline)]
        public static void On_WorldClient_AcademyMemberOnline(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                GuildAcademyMember member;
                if (guild.Academy.GetMember(characterID, out member))
                {
                    member.IsOnline = true;


                    ZoneCharacter character;
                    if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                    {
                        character.Guild = guild;
                        character.GuildAcademy = guild.Academy;
                        character.GuildAcademyMember = member;

                        member.Character = character;
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneAcademyMemberOffline)]
        public static void On_WorldClient_AcademyMemberOffline(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                GuildAcademyMember member;
                if (guild.Academy.GetMember(characterID, out member))
                {
                    member.IsOnline = false;


                    ZoneCharacter character;
                    if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                    {
                        character.Guild = null;
                        character.GuildAcademy = null;
                        character.GuildAcademyMember = null;

                        member.Character = null;
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZoneAcademyBuffUpdate)]
        public static void On_WorldClient_AcademyBuffUpdate(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID;
            DateTime updateTime;
            double keepTime;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadDateTime(out updateTime)
                || !pPacket.TryReadDouble(out keepTime))
            {
                //Client.Dispose();
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                guild.Academy.GuildBuffUpdateTime = updateTime;
                guild.Academy.GuildBuffKeepTime = TimeSpan.FromSeconds(keepTime);
            }
        }
    }
}