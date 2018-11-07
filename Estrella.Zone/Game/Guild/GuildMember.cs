/*File for this file Basic Copyright 2012 no0dl */

using MySql.Data.MySqlClient;

namespace Estrella.Zone.Game.Guild
{
    public sealed class GuildMember
    {
        public GuildMember(Guild Guild, MySqlDataReader reader)
        {
            this.Guild = Guild;

            CharacterID = reader.GetInt32(1);
            Rank = (GuildRank) reader.GetByte(2);
            Corp = (ushort) reader.GetInt16(3);
        }

        public GuildMember(Guild Guild, int CharacterID, GuildRank Rank, ushort Corp)
        {
            this.Guild = Guild;
            this.CharacterID = CharacterID;
            this.Rank = Rank;
            this.Corp = Corp;
        }

        public Guild Guild { get; private set; }
        public int CharacterID { get; private set; }

        public GuildRank Rank { get; set; }
        public ushort Corp { get; set; }


        public bool IsOnline { get; set; }

        public bool IsOnThisZone
        {
            get { return Character != null; }
        }

        public ZoneCharacter Character { get; set; }

        public void Dispose()
        {
            Guild = null;
            Character = null;
        }
    }
}