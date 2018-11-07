using System;
using Estrella.World.Data;

namespace Estrella.World.Events
{
    public class OnCharacterLevelUpArgs : EventArgs
    {
        public delegate void DelegatetType(WorldCharacter pChar);

        public OnCharacterLevelUpArgs(WorldCharacter pChar = null)
        {
            PCharacter = pChar;
        }

        public OnCharacterLevelUpArgs()
        {
        }

        public WorldCharacter PCharacter { get; set; }
    }
}