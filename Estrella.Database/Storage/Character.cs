using System;
using System.Collections.Generic;

namespace Estrella.Database.Storage
{
    public class Character
    {
        public CharacterStats CharacterStats = new CharacterStats();
        public LookInfo LookInfo = new LookInfo();
        public PositionInfo PositionInfo = new PositionInfo();
        public List<DatabaseSkill> SkillList = new List<DatabaseSkill>();

        public int ID { get; set; }
        public int AccountID { get; set; }
        public string Name { get; set; }
        public byte Slot { get; set; }
        public byte CharLevel { get; set; }
        public byte Job { get; set; }
        public int HP { get; set; }
        public int SP { get; set; }
        public int instanzeID { get; set; }
        public short HPStones { get; set; }
        public short SPStones { get; set; }
        public long Exp { get; set; }
        public int Fame { get; set; }
        public long Money { get; set; }
        public byte StatPoints { get; set; }
        public byte UsablePoints { get; set; }
        public byte[] QuickBar { get; set; }
        public byte[] Shortcuts { get; set; }
        public byte[] QuickBarState { get; set; }
        public byte[] GameSettings { get; set; }
        public byte[] ClientSettings { get; set; }
        public int MountID { get; set; }
        public int MountFood { get; set; }
        public int GuildID { get; set; }
        public int AcademyID { get; set; }
        public DateTime MasterJoin { get; set; }
        public long GroupId { get; set; }
        public bool IsGroupMaster { get; set; }
        public long ReviveCoper { get; set; }
    }
}