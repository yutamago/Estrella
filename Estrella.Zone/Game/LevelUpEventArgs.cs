using System;

namespace Estrella.Zone.Game
{
    public class LevelUpEventArgs : EventArgs
    {
        public LevelUpEventArgs(int oldLevel, int newLevel, ushort mobId)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
            MobId = mobId;
        }

        public int OldLevel { get; private set; }
        public int NewLevel { get; private set; }
        public ushort MobId { get; set; }
    }
}