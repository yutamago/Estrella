using System;
using Estrella.World.Data;

namespace Estrella.World.Events
{
    public class OnCharacterLogoutArgs : EventArgs
    {
        public OnCharacterLogoutArgs(WorldCharacter pChar)
        {
            PCharacter = pChar;
        }

        public WorldCharacter PCharacter { get; set; }
    }
}