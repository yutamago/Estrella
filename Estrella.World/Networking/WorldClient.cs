using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using Estrella.Database.DataStore;
using Estrella.Database.Storage;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.World.Data;
using Estrella.World.Handlers;
using Estrella.World.Managers;

namespace Estrella.World.Networking
{
    public sealed class WorldClient : Client
    {
        #region .ctor

        public WorldClient(Socket socket)
            : base(socket)
        {
            OnPacket += WorldClient_OnPacket;
            OnDisconnect += WorldClient_OnDisconnect;
        }

        #endregion

        #region Properties

        public bool Authenticated { get; set; }
        public string Username { get; set; }
        public int AccountId { get; set; }
        public byte Admin { get; set; }
        public ushort RandomId { get; set; } //this ID is used to authenticate later on.
        public Dictionary<byte, WorldCharacter> Characters { get; private set; }
        public WorldCharacter Character { get; set; }
        public DateTime LastPing { get; set; }
        public bool Pong { get; set; }

        #endregion

        #region Methods

        private void WorldClient_OnDisconnect(object sender, SessionCloseEventArgs e)
        {
            Log.WriteLine(LogLevel.Debug, "{0} Disconnected.", Host);
            ClientManager.Instance.RemoveClient(this);
        }

        private void WorldClient_OnPacket(object sender, PacketReceivedEventArgs e)
        {
            if (!Authenticated && !(e.Packet.Header == 3 && e.Packet.Type == 15))
                return; //do not handle packets if not authenticated!
            var method = HandlerStore.GetHandler(e.Packet.Header, e.Packet.Type);
            if (method != null)
            {
                var action = HandlerStore.GetCallback(method, this, e.Packet);
                Worker.Instance.AddCallback(action);
            }
            else
            {
                Console.WriteLine(e.Packet.Header);
                Log.WriteLine(LogLevel.Debug, "Unhandled packet: {0}", e.Packet);
            }
        }

        public bool LoadCharacters()
        {
            if (!Authenticated) return false;
            Characters = new Dictionary<byte, WorldCharacter>();
            try
            {
                DataTable charData;
                using (var dbClient = Program.DatabaseManager.GetClient())
                {
                    charData = dbClient.ReadDataTable("SELECT * FROM Characters WHERE AccountID='" + AccountId +
                                                      "'");
                }

                if (charData != null)
                {
                    foreach (DataRow row in charData.Rows)
                    {
                        var ch = new Character();
                        ch.PositionInfo.ReadFromDatabase(row);
                        ch.LookInfo.ReadFromDatabase(row);
                        ch.CharacterStats.ReadFromDatabase(row);
                        ch.Slot = (byte) row["Slot"];
                        ch.CharLevel = (byte) row["Level"];
                        ch.AccountID = AccountId;
                        ch.Name = (string) row["Name"];
                        ch.ID = GetDataTypes.GetInt(row["CharID"]);
                        ch.Job = (byte) row["Job"];
                        ch.Money = GetDataTypes.GetLong(row["Money"].ToString());
                        ch.Exp = long.Parse(row["Exp"].ToString());
                        ch.HP = int.Parse(row["CurHP"].ToString());
                        ch.HPStones = 10;
                        ch.MasterJoin = DateTime.Parse(row["MasterJoin"].ToString());
                        ch.SP = int.Parse(row["CurSP"].ToString());
                        ch.SPStones = 10;
                        ch.StatPoints = (byte) row["StatPoints"];
                        ch.UsablePoints = (byte) row["UsablePoints"];
                        ch.Fame = 0; // TODO
                        ch.GameSettings = ReadMethods.GetGameSettings(ch.ID, Program.DatabaseManager);
                        ch.ClientSettings = ReadMethods.GetClientSettings(ch.ID, Program.DatabaseManager);
                        ch.Shortcuts = ReadMethods.GetShortcuts(ch.ID, Program.DatabaseManager);
                        ch.QuickBar = ReadMethods.GetQuickBar(ch.ID, Program.DatabaseManager);
                        ch.QuickBarState = ReadMethods.GetQuickBarState(ch.ID, Program.DatabaseManager);
                        ch.ReviveCoper = GetDataTypes.GetLong(row["MasterReciveMoney"]);
                        if (row.IsNull("GroupID"))
                            ch.GroupId = -1;
                        else
                            ch.GroupId = long.Parse(row["GroupID"].ToString());

                        if (ch.GroupId == -1 || row.IsNull("IsGroupMaster"))
                            ch.IsGroupMaster = false;
                        else
                            ch.IsGroupMaster = ReadMethods.EnumToBool(row["IsGroupMaster"].ToString());

                        Characters.Add(ch.Slot, new WorldCharacter(ch, this));
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Log.WriteLine(LogLevel.Exception, "Error loading characters from {0}: {1}", Username,
                        ex.InnerException.ToString());
                return false;
            }

            return true;
        }

        public ClientTransfer GenerateTransfer(byte slot)
        {
            if (!Characters.ContainsKey(slot))
            {
                Log.WriteLine(LogLevel.Warn, "Generating transfer for slot {0} which {1} doesn't own.", slot, Username);
                return null;
            }

            if (Characters.TryGetValue(slot, out var character))
            {
                return new ClientTransfer(AccountId, Username, character.Character.Name, character.Character.ID,
                    RandomId, Admin, Host);
            }

            return null;
        }

        public WorldCharacter CreateCharacter(string name, byte slot, byte hair, byte color, byte face, Job job,
            bool isMale)
        {
            if (Characters.ContainsKey(slot) || slot > 5)
                return null;
            //TODO: check if hair etc are actual beginner ones! (premium hack)
            //NOTE: Check the SHN's for this -> Moved to database
            var stats = DataProvider.Instance.JobBasestats[job];
            if (stats == null)
            {
                //NOTE be serious.. please
                // Log.WriteLine(LogLevel.Warn, "Houston, we have a problem! JobStats not found for job {0}", job.ToString()); 
                Log.WriteLine(LogLevel.Error, "JobStats not found for job {0}", job.ToString());
                return null;
            }

            var newLook = new LookInfo
            {
                Face = face,
                Hair = hair,
                HairColor = color,
                Male = isMale
            };

            var newPos = new PositionInfo
            {
                XPos = 7636,
                YPos = 4610
            };

            var newChar = new Character
            {
                AccountID = AccountId,
                CharLevel = 1,
                Name = name,
                Job = (byte) job,
                Slot = slot,
                HP = (short) stats.MaxHP,
                SP = (short) stats.MaxSP,
                HPStones = (short) stats.MaxHPStones,
                SPStones = (short) stats.MaxSPStones,
                LookInfo = newLook,
                PositionInfo = newPos
            };
            var charId = newChar.ID;
            var client = Program.DatabaseManager.GetClient();

            var query =
                "INSERT INTO `characters` " +
                "(`AccountID`,`Name`,`MasterJoin`,`Slot`,`Job`,`Male`,`Hair`,`HairColor`,`Face`," +
                " `QuickBar`, `QuickBarState`, `ShortCuts`, `GameSettings`, `ClientSettings`) " +
                "VALUES " +
                "('" + newChar.AccountID +
                "', '" + newChar.Name +
                "', '" + DateTime.Now.ToDBString() +
                "', " + newChar.Slot +
                ", " + newChar.Job +
                ", " + Convert.ToByte(newChar.LookInfo.Male) +
                ", " + newChar.LookInfo.Hair +
                ", " + newChar.LookInfo.HairColor +
                ", " + newChar.LookInfo.Face +
                ", " + "0" +
                ", " + "0" +
                ", " + "0" +
                ", " + "0" +
                ", " + "0" +
                ")";
            client.ExecuteQuery(query);

            var worldCharacter = new WorldCharacter(newChar, this);
            var beginEqp = GetBeginnerEquip(job);

            if (beginEqp > 0)
            {
                var eqpSlot =
                    (sbyte) (job == Job.Archer ? -10 : -12); //, (job == Job.Archer) ? (byte)12 : (byte)10, begineqp)
                var eqp = new Equip((uint) newChar.ID, beginEqp, eqpSlot);
                worldCharacter.Inventory.AddToEquipped(eqp);
                client.ExecuteQuery("INSERT INTO equips (owner,slot,EquipID) VALUES ('" + worldCharacter.ID + "','" + eqpSlot +
                                    "','" + eqp.EquipId + "')");
            }

            Characters.Add(slot, worldCharacter);
            return worldCharacter;
        }

        //TODO: move to helper class?
        //NOTE: DO IT.
        private ushort GetBeginnerEquip(Job job)
        {
            ushort equipId;
            switch (job)
            {
                case Job.Archer:
                    equipId = 1250;
                    break;
                case Job.Fighter:
                    equipId = 250;
                    break;
                case Job.Cleric:
                    equipId = 750;
                    break;
                case Job.Mage:
                    equipId = 1750;
                    break;
                case Job.Trickster:
                    equipId = 57363;
                    break;
                default:
                    Log.WriteLine(LogLevel.Exception, "{0} is creating a wrong job (somehow)", Username);
                    return 0;
            }

            return equipId;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WorldClient))
                return false;
            var other = (WorldClient) obj;
            return other.AccountId == AccountId;
        }

        public override int GetHashCode()
        {
            return AccountId;
        }

        #endregion
    }
}