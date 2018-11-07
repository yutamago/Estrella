using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Estrella.Database.DataStore;
using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.Zone.Data;
using Estrella.Zone.Handlers;

namespace Estrella.Zone.Game
{
    public sealed class Map
    {
        public const int SectorCount = 16;
        private readonly Queue<ushort> availableDropKeys = new Queue<ushort>();
        private readonly Queue<ushort> availableLifeKeys = new Queue<ushort>();

        private readonly Sector[,] sectors = new Sector[SectorCount, SectorCount];
        private ushort dropIndexer = 1;
        private ushort lifeIndexer = 1;
        private int sectorHeight = 800;
        private int sectorWidth = 800; //default

        public Map(MapInfo info, BlockInfo block, short instanceID)
        {
            MapInfo = info;
            Block = block;
            InstanceID = instanceID;
            Objects = new ConcurrentDictionary<ushort, MapObject>();
            Drops = new ConcurrentDictionary<ushort, Drop>();
            Load();
        }

        public MapInfo MapInfo { get; private set; }

        public ushort MapID
        {
            get { return MapInfo.ID; }
        }

        public short InstanceID { get; private set; }
        public ConcurrentDictionary<ushort, MapObject> Objects { get; private set; }
        public ConcurrentDictionary<ushort, Drop> Drops { get; private set; }
        public BlockInfo Block { get; private set; }
        public List<MobBreedLocation> MobBreeds { get; private set; }

        private void Load()
        {
            LoadSectors();
            LoadNpc();
            LoadMobBreeds();
        }

        private void LoadNpc()
        {
            //NPC's are shown always, they don't dissapear from sectors
            foreach (var spawn in MapInfo.NPCs)
            {
                var npc = new Npc(spawn);
                if (npc == null)
                    Log.WriteLine(LogLevel.Warn, "NULL value for {0} {1}:{2} of mob {3}", spawn.Map, spawn.CoordX,
                        spawn.CoordY, spawn.MobName);
                else
                    FullAddObject(npc);
            }
        }

        private void LoadMobBreeds()
        {
            MobBreeds = new List<MobBreedLocation>();

            DataTable data = null;
            using (var dbClient = Program.DatabaseManager.GetClient())
            {
                data = dbClient.ReadDataTable("SELECT  *FROM `" + Settings.Instance.zoneMysqlDatabase +
                                              "`.`Mobspawn` WHERE MapID='" + MapInfo.ID + "'");
            }

            if (data != null)
            {
                if (data.Rows.Count > 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        var locationInfo = new MobBreedLocation
                        {
                            MapID = GetDataTypes.GetUshort(row["MapID"]),
                            MobID = GetDataTypes.GetUshort(row["MobID"]),
                            //InstanceID = GetDataTypes.Getshort(row["InstanceID"]),
                            //NOTE from db throws;
                            InstanceID = 0,
                            Position = new Vector2()
                        };
                        locationInfo.Position.X = GetDataTypes.GetInt(row["PosX"]);
                        locationInfo.Position.Y = GetDataTypes.GetInt(row["PosY"]);
                        MobBreeds.Add(locationInfo);
                    }
                }
                else
                {
                    MobBreeds = new List<MobBreedLocation>();
                }
            }
        }

        public void SaveMobBreeds()
        {
            //this can dam long....
            foreach (var mobspawn in MobBreeds)
            {
                Program.DatabaseManager.GetClient().ExecuteQuery(
                    "INSERT INTO `" + Settings.Instance.zoneMysqlDatabase +
                    "`.`Mobspawn` (MobID,MapID,PosX,PosY,InstanceID) VALUES ('" + mobspawn.MobID + "','" +
                    mobspawn.MapID + "','" + mobspawn.Position.X + "','" + mobspawn.Position.Y + "','" +
                    mobspawn.InstanceID + "')");
            }
        }

        public void AddDrop(Drop drop)
        {
            ushort key = 0;
            if (availableDropKeys.Count > 0)
            {
                key = availableDropKeys.Dequeue();
            }
            else
            {
                if (dropIndexer == ushort.MaxValue)
                {
                    Log.WriteLine(LogLevel.Warn, "Drop buffer overflow at map {0}.", MapInfo.ShortName);
                    return;
                }

                key = dropIndexer++;
            }

            if (Drops.TryAdd(key, drop))
            {
                drop.ID = key;
                drop.MapSector = GetSectorByPos(drop.Position);
                drop.MapSector.AddDrop(drop);
            }
            else Log.WriteLine(LogLevel.Warn, "Failed to add drop at map {0}.", MapInfo.ShortName);
        }

        public void RemoveDrop(Drop drop)
        {
            if (drop.MapSector == null)
            {
                Log.WriteLine(LogLevel.Warn, "Tried to remove drop where sectors wasn't assigned.");
                return;
            }

            Drop test;
            if (Drops.TryRemove(drop.ID, out test) && test == drop)
            {
                availableDropKeys.Enqueue(drop.ID);
                drop.MapSector.RemoveDrop(drop);
                drop.MapSector = null;
            }
        }

        private void LoadSectors()
        {
            try
            {
                var sectorcount = SectorCount;
                if (Block != null)
                {
                    while (Block.Width / sectorcount < MapInfo.ViewRange && sectorcount != 1)
                    {
                        sectorcount--;
                    }

                    sectorWidth = Block.Width / sectorcount;
                    if (sectorWidth < MapInfo.ViewRange)
                    {
                        sectorWidth = MapInfo.ViewRange;
                    }

                    sectorHeight = Block.Height / sectorcount;
                    if (sectorHeight < MapInfo.ViewRange)
                    {
                        sectorHeight = MapInfo.ViewRange;
                    }
                }

                for (var y = 0; y < sectorcount; y++)
                {
                    for (var x = 0; x < sectorcount; x++)
                    {
                        sectors[y, x] = new Sector(x, y, this);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, ex.ToString());
            }
        }

        public Sector GetSectorByPos(int x, int y)
        {
            var ymatrix = y / sectorHeight;
            var xmatrix = x / sectorWidth;
            return sectors[ymatrix, xmatrix];
        }

        public List<Sector> GetSurroundingSectors(Sector sector)
        {
            var x = sector.X;
            var y = sector.Y;
            var toret = new List<Sector>();
            for (var column = x - 1; column <= x + 1; ++column)
            {
                for (var row = y - 1; row <= y + 1; ++row)
                {
                    if (column < 0 || row < 0 || column >= SectorCount || row >= SectorCount)
                    {
                        continue;
                    }

                    toret.Add(sectors[row, column]);
                }
            }

            return toret;
        }

        public List<Sector> GetSectorsNewInRange(Sector oldsector, Sector newsector)
        {
            return new List<Sector>(newsector.SurroundingSectors.Except(oldsector.SurroundingSectors));
        }

        public List<Sector> GetSectorsOutOfRange(Sector oldsector, Sector newsector)
        {
            return new List<Sector>(oldsector.SurroundingSectors.Except(newsector.SurroundingSectors));
        }

        public List<ZoneCharacter> GetCharactersInRegion(Sector sector)
        {
            return GetCharactersBySectors(GetSurroundingSectors(sector));
        }

        public List<ZoneCharacter> GetCharactersBySectors(List<Sector> input)
        {
            var toret = new List<ZoneCharacter>();
            foreach (var sursector in input)
            {
                toret.AddRange(sursector.Objects.Values.Where(o => o is ZoneCharacter).Cast<ZoneCharacter>());
            }

            return toret;
        }

        public List<Drop> GetDropsBySectors(List<Sector> input)
        {
            var toret = new List<Drop>();
            foreach (var sect in input)
            {
                toret.AddRange(sect.Drops.Values);
            }

            return toret;
        }

        public List<MapObject> GetObjectsBySectors(List<Sector> input)
        {
            var toret = new List<MapObject>();
            foreach (var sursector in input)
            {
                toret.AddRange(sursector.Objects.Values.Where(o => !(o is Npc)));
            }

            return toret;
        }

        public void SendCharacterLeftMap(ZoneCharacter character, bool toplayer = true)
        {
            using (var removeObjPacket = Handler7.RemoveObject(character)
            ) // Make new packet to remove object from map for others
            {
                foreach (var victimObject in character.MapSector.Objects.Values)
                {
                    if (victimObject == character) continue; // ...
                    switch (victimObject)
                    {
                        case Npc _:
                            continue; // NPC's are for noobs. Can't despawn
                        case ZoneCharacter _:
                        {
                            // Remove obj for player
                            var victim = victimObject as ZoneCharacter;
                            victim.Client.SendPacket(removeObjPacket);
                            break;
                        }
                    }

                    if (!toplayer) continue;
                    // Despawn victimObject for obj
                    using (var removeVictimPacket = Handler7.RemoveObject(victimObject))
                    {
                        character.Client.SendPacket(removeVictimPacket);
                    }
                }
            }
        }

        public void SendCharacterEnteredMap(ZoneCharacter character)
        {
            //we send all players in region to character
            var characters = GetCharactersInRegion(character.MapSector);
            using (var packet = Handler7.SpawnMultiPlayer(characters, character))
            {
                character.Client.SendPacket(packet);
                if (character.Mount != null)
                {
                    character.Mounting(character.Mount.Handle, true);
                }
            }

            //we send character to all players in region
            using (var packet = Handler7.SpawnSinglePlayer(character))
            {
                foreach (var charinmap in characters)
                {
                    if (charinmap == character) continue;
                    charinmap.Client.SendPacket(packet);
                }
            }

            //we send moblist and NPC to local character
            var npcs = Objects.Values.Where(o => o is Npc);
            var monsters =
                GetObjectsBySectors(character.MapSector.SurroundingSectors).Where(o => o is Mob);

            var obj = new List<MapObject>(npcs);
            obj.AddRange(monsters);
            if (obj.Count > 0)
            {
                for (var i = 0; i < obj.Count; i += 255)
                {
                    using (var packet = Handler7.MultiObjectList(obj, i, i + Math.Min(255, obj.Count - i)))
                    {
                        character.Client.SendPacket(packet);
                    }
                }
            }

            //we send all drops to the character
            using (var spawndrops = Handler7.ShowDrops(GetDropsBySectors(character.MapSector.SurroundingSectors)))
            {
                character.Client.SendPacket(spawndrops);
            }
        }

        public Sector GetSectorByPos(Vector2 position)
        {
            return GetSectorByPos(position.X, position.Y);
        }

        public bool FinalizeAdd(MapObject obj)
        {
            var sector = GetSectorByPos(obj.Position);
            sector.AddObject(obj, true);
            return Objects.TryAdd(obj.MapObjectID, obj);
        }

        public bool AssignObjectID(MapObject obj)
        {
            var result = false;
            lock (availableLifeKeys)
            {
                if (availableLifeKeys.Count == 0)
                {
                    if (lifeIndexer == ushort.MaxValue)
                    {
                        Log.WriteLine(LogLevel.Warn,
                            "Map is having map object id overflow (cannot handler more than {0})", ushort.MaxValue);
                        result = false;
                    }
                    else
                    {
                        var key = lifeIndexer;
                        ++lifeIndexer;
                        obj.MapObjectID = key;
                        result = true;
                    }
                }
                else
                {
                    var key = availableLifeKeys.Dequeue();
                    obj.MapObjectID = key;
                    result = true;
                }

                if (result)
                    obj.Map = this;
                return result;
            }
        }


        public bool FullAddObject(MapObject obj)
        {
            Log.WriteLine(LogLevel.Debug, "Added {0} to the map.", obj.GetType().ToString());
            if (AssignObjectID(obj))
            {
                return FinalizeAdd(obj);
            }

            return false;
        }

        public bool RemoveObject(ushort mapobjid)
        {
            MapObject obj;
            if (Objects.TryRemove(mapobjid, out obj))
            {
                Log.WriteLine(LogLevel.Debug, "Removed {0} (type: {1}) from the map.", mapobjid,
                    obj.GetType().ToString());
                obj.MapSector.RemoveObject(obj);
                obj.MapObjectID = 0;
                obj.Map = null;
                lock (availableLifeKeys)
                {
                    availableLifeKeys.Enqueue(mapobjid);
                }

                return true;
            }

            return false;
        }

        public void Broadcast(Packet packet)
        {
            foreach (var kvp in Objects.Where(o => o.Value is ZoneCharacter))
            {
                var victim = (ZoneCharacter) kvp.Value;
                victim.Client?.SendPacket(packet);
            }
        }

        public void Update(DateTime date)
        {
            foreach (var kvp in Objects)
            {
                kvp.Value.Update(date);
            }

            foreach (var mb in MobBreeds)
            {
                mb.Update(date);
            }

            foreach (var drop in Drops.Values)
            {
                if (drop.IsExpired(date))
                {
                    drop.CanTake = false;
                    RemoveDrop(drop);
                }
            }
        }
    }
}