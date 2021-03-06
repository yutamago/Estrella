﻿using System.Collections.Generic;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.Zone.Handlers;

namespace Estrella.Zone.Game
{
    public sealed class Sector
    {
        private Dictionary<ushort, Drop> drops;

        private Dictionary<ushort, MapObject> objects;

        public Sector(int x, int y, Map map)
        {
            X = x;
            Y = y;
            Map = map;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public Map Map { get; private set; }

        public Dictionary<ushort, MapObject> Objects
        {
            get { return objects ?? (objects = new Dictionary<ushort, MapObject>()); }
        }

        public Dictionary<ushort, Drop> Drops
        {
            get { return drops ?? (drops = new Dictionary<ushort, Drop>()); }
        }

        private List<Sector> surroundingSectors { get; set; }

        public List<Sector> SurroundingSectors
        {
            get
            {
                // Lets lazyload this!
                return surroundingSectors ?? (surroundingSectors = Map.GetSurroundingSectors(this));
            }
        }

        public bool Update()
        {
            //TODO: update all logic in here
            return false;
        }

        public void AddDrop(Drop drop)
        {
            Drops.Add(drop.ID, drop);
            using (var spawn = Handler7.ShowDrop(drop))
            {
                Broadcast(spawn);
            }
        }

        public void RemoveDrop(Drop drop)
        {
            if (Drops.Remove(drop.ID))
            {
                using (var removedrop = Handler6.RemoveDrop(drop))
                {
                    Broadcast(removedrop);
                }
            }
        }

        public void RemoveObject(MapObject obj)
        {
            Objects.Remove(obj.MapObjectID);
            obj.MapSector = null;

            using (var remover = Handler7.RemoveObject(obj))
            {
                Broadcast(remover);
            }
        }

        public void Transfer(MapObject obj, Sector to)
        {
            Objects.Remove(obj.MapObjectID);
            to.AddObject(obj);
            // we remove object first to the 'non display region'
            var character = obj as ZoneCharacter;
            var oldsectors = Map.GetSectorsOutOfRange(this, to);

            // The idea of this function
            // 1. Remove obj from map for players that are out of range
            // (IF PLAYER) 2. Remove objects that are out of range for obj
            // (IF PLAYER) 3. Spawn objects that are new in range for obj
            // 4. Spawn obj for all players in new range

            using (var removeObjPacket = Handler7.RemoveObject(obj)
            ) // Make new packet to remove object from map for others
            {
                // Even nederlands: Kijken of we mensen kunnen vinden in range?
                foreach (var victimObject in Map.GetObjectsBySectors(oldsectors))
                {
                    if (victimObject is Npc) continue; // NPC's are for noobs. Can't despawn

                    if (victimObject is ZoneCharacter)
                    {
                        // Remove obj for player
                        var victim = victimObject as ZoneCharacter;
                        victim.Client.SendPacket(removeObjPacket);
                    }

                    if (character != null)
                    {
                        // Despawn victimObject for obj
                        using (var removeVictimPacket = Handler7.RemoveObject(victimObject))
                        {
                            character.Client.SendPacket(removeVictimPacket);
                        }
                    }
                }
            }

            //we remove all the drops out of the character's region
            if (character != null && character.Client != null)
            {
                foreach (var dropoutofrange in Map.GetDropsBySectors(oldsectors))
                {
                    using (var despawndrop = Handler6.RemoveDrop(dropoutofrange))
                    {
                        character.Client.SendPacket(despawndrop);
                    }
                }
            }


            //now we spawn the object to other objects in map

            var newsectors = Map.GetSectorsNewInRange(this, to);
            var objects = Map.GetObjectsBySectors(newsectors);

            using (var packet = obj.Spawn())
            {
                foreach (var mapObject in objects)
                {
                    if (mapObject is Npc) continue; //we don't respawn NPCs

                    if (mapObject is ZoneCharacter)
                    {
                        var rangechar = mapObject as ZoneCharacter;

                        // Send spawn packet of the object (can be both a character and mob) to mapObject (a player)
                        rangechar.Client.SendPacket(packet);
                    }

                    if (character != null)
                    {
                        // Send spawn packet of mapObject to character
                        using (var spawn = mapObject.Spawn())
                        {
                            character.Client.SendPacket(spawn);
                        }
                    }
                }
            }

            if (character != null && character.Client != null)
            {
                using (var spawndrops = Handler7.ShowDrops(Map.GetDropsBySectors(newsectors)))
                {
                    character.Client.SendPacket(spawndrops);
                }
            }
        }

        public void AddObject(MapObject obj, bool spawn = false)
        {
            if (Objects.ContainsKey(obj.MapObjectID))
            {
                Log.WriteLine(LogLevel.Warn, "Duplicate object id in sector {0}:{1}", Y, X);
                Objects.Remove(obj.MapObjectID);
            }

            Objects.Add(obj.MapObjectID, obj);
            obj.MapSector = this;

            if (spawn)
            {
                //broadcast mob to map
                //broadcast other players to map (port code from Map.CharacterEnteredMap to here)

                // If Player: Spawn all Mobs and Players in range to Player, and Whole NPC list
                // If Mob: Spawn Mob to all Players in range
                // If NPC: Lolwut

                if (obj is ZoneCharacter)
                {
                    Map.SendCharacterEnteredMap(obj as ZoneCharacter);
                }
                else if (obj is Mob)
                {
                    using (var spawnpacket = obj.Spawn())
                    {
                        Broadcast(spawnpacket);
                    }
                }
            }
        }

        public void Broadcast(Packet packet, ushort idskip = (ushort) 0)
        {
            foreach (var rcharacter in Map.GetCharactersBySectors(SurroundingSectors))
            {
                if (rcharacter.MapObjectID == idskip) continue;
                rcharacter.Client.SendPacket(packet);
            }
        }
    }
}