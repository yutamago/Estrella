﻿/*File for this file Basic Copyright 2012 no0dl */

using System;
using MySql.Data.MySqlClient;

namespace Estrella.Zone.Game.Guild.Academy
{
    public sealed class GuildAcademyMember
    {
        public GuildAcademyMember(GuildAcademy Academy, int CharacterID, GuildAcademyRank Rank, DateTime RegisterTime)
        {
            this.Academy = Academy;
            this.CharacterID = CharacterID;
            this.Rank = Rank;
            RegisterDate = RegisterDate;
        }

        public GuildAcademyMember(GuildAcademy Academy, MySqlDataReader reader)
        {
            this.Academy = Academy;


            Load(reader);
        }

        public GuildAcademy Academy { get; private set; }

        public int CharacterID { get; private set; }
        public ZoneCharacter Character { get; set; }
        public bool IsOnline { get; set; }

        public bool IsOnThisZone
        {
            get { return Character != null; }
        }

        public GuildAcademyRank Rank { get; set; }

        public DateTime RegisterDate { get; private set; }
        public bool IsChatBlocked { get; set; }

        public void Dispose()
        {
            Academy = null;
            Character = null;
        }


        private void Load(MySqlDataReader reader)
        {
            CharacterID = reader.GetInt32("ID");
            RegisterDate = reader.GetDateTime("RegisterDate");
            IsChatBlocked = reader.GetBoolean("ChatBlock");
            Rank = (GuildAcademyRank) reader.GetByte("Rank");
        }
    }
}