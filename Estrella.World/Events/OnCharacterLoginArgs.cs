using System;
using Estrella.World.Data;

namespace Estrella.World.Events
{
    public class OnCharacterLoginArgs : EventArgs
    {
        public OnCharacterLoginArgs(WorldCharacter pChar, OnCharacterLoginArgs args)
        {
        }
    }
}