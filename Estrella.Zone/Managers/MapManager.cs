﻿using System.Collections.Generic;
using Estrella.FiestaLib.Data;
using Estrella.Util;
using Estrella.Zone.Data;
using Estrella.Zone.Game;

namespace Estrella.Zone.Managers
{
    [ServerModule(InitializationStage.DataStore)]
    public sealed class MapManager
    {
        public MapManager()
        {
            Maps = new Dictionary<MapInfo, List<Map>>();
        }

        public static MapManager Instance { get; private set; }
        public Dictionary<MapInfo, List<Map>> Maps { get; private set; }

        public Map GetMap(MapInfo info, short instance = (short) 0)
        {
            //lazy loading
            if (!Maps.ContainsKey(info))
            {
                Maps.Add(info, new List<Map>());
            }

            BlockInfo block;
            DataProvider.Instance.Blocks.TryGetValue(info.ID, out block);
            Map toret;
            var maps = Maps[info];
            if (maps.Count == 0)
            {
                //we load the first map instance
                maps.Add(toret = new Map(info, block, (short) maps.Count));
            }
            else
            {
                if (maps.Count < instance) // Check if instance exists, else, add another
                {
                    if (maps.Count - 1 < instance)
                    {
                        // ohnoes
                        Log.WriteLine(LogLevel.Info, "Couldn't find instance for map {0}", info.ID);
                        instance = 0;
                    }
                    else
                    {
                        // Add another instance of map
                        maps.Add(toret = new Map(info, block, (short) maps.Count));
                    }
                }

                toret = Maps[info][instance];
            }

            return toret;
        }

        [InitializerMethod]
        public static bool Load()
        {
            Instance = new MapManager();
            return true;
        }
    }
}