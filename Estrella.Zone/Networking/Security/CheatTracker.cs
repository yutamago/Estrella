﻿using System.Collections.Generic;
using Estrella.Util;
using Estrella.Zone.Game;

namespace Estrella.Zone.Networking.Security
{
    public sealed class CheatTracker
    {
        public const ushort MaxPoints = 500;
        private readonly ZoneCharacter character;

        private Dictionary<CheatTypes, ushort> cheats;

        public CheatTracker(ZoneCharacter character)
        {
            this.character = character;
        }

        public ushort CheatPoints { get; private set; }

        private Dictionary<CheatTypes, ushort> Cheats
        {
            get { return cheats ?? (cheats = new Dictionary<CheatTypes, ushort>()); }
        }

        public void AddCheat(CheatTypes type, ushort points)
        {
            Log.WriteLine(LogLevel.Debug, "Detecting cheat from {0}: {1}", character.Name, type.ToString());
            if (!Cheats.ContainsKey(type))
            {
                Cheats.Add(type, 0);
            }

            Cheats[type]++;
            if (character.Client.Admin == 0)
            {
                CheatPoints += points;
                CheckBan();
            }
        }

        private void CheckBan()
        {
            if (CheatPoints >= MaxPoints)
            {
                Log.WriteLine(LogLevel.Info, "CheatTracker auto banned {0}.", character.Name);
                character.Ban();
            }
        }
    }
}