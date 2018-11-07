using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Estrella.Database.Storage;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.InterLib.Networking;
using Estrella.Util;
using Estrella.World.Data.Group;
using Estrella.World.Data.Guild;
using Estrella.World.Data.Guild.Academy;
using Estrella.World.Data.MasterSystem;
using Estrella.World.Handlers;
using Estrella.World.InterServer;
using Estrella.World.Managers;
using Estrella.World.Networking;

namespace Estrella.World.Data
{
    public class WorldCharacter
    {
        public List<string> BlocketUser = new List<string>();
        private List<Friend> friends;
        private List<Friend> friendsby;
        public Inventory Inventory = new Inventory();
        public List<MasterMember> MasterList = new List<MasterMember>();

        public WorldCharacter(Character ch, WorldClient client)
        {
            Character = ch;
            Client = client;
            ID = Character.ID;
            Equips = new Dictionary<byte, ushort>();
            Inventory.LoadBasic(this);
            LoadEqupippet();
            ;
        }

        public WorldCharacter(Character ch, byte eqpslot, ushort eqpid)
        {
            Character = ch;
            ID = Character.ID;
            Equips = new Dictionary<byte, ushort>();
            Equips.Add(eqpslot, eqpid);
        }

        public Character Character { get; set; }

        public WorldClient Client { get; set; }

        //	public Character Character { get { return _character ?? (_character = LazyLoadMe()); } set { _character = value; } }
        public int ID { get; private set; }
        public Dictionary<byte, ushort> Equips { get; private set; }
        public bool IsDeleted { get; private set; }
        public bool IsIngame { get; set; }
        public bool IsPartyMaster { get; set; }
        public Group.Group Group { get; internal set; }
        public long GroupId { get; internal set; }
        public GroupMember GroupMember { get; internal set; }
        public long RecviveCoperMaster { get; set; }


        public bool IsInGuildAcademy { get; set; }
        public bool IsInGuild { get; set; }
        public Guild.Guild Guild { get; set; }
        public GuildMember GuildMember { get; set; }
        public GuildAcademy GuildAcademy { get; set; }
        public GuildAcademyMember GuildAcademyMember { get; set; }
        public DateTime LastGuildListRefresh { get; set; }

        public bool IsOnline
        {
            get { return ClientManager.Instance.IsOnline(Character.Name); }
        }

        public List<Friend> Friends
        {
            get
            {
                if (friends == null)
                {
                    LoadFriends();
                }

                return friends;
            }
        }

        public event EventHandler GotIngame;

        public void LoadFriends()
        {
            friends = new List<Friend>();
            friendsby = new List<Friend>();
            DataTable frenddata = null;
            DataTable frenddataby = null;

            using (var dbClient = Program.DatabaseManager.GetClient())
            {
                frenddata = dbClient.ReadDataTable("SELECT * FROM friends WHERE CharID='" + Character.ID + "'");
                frenddataby =
                    dbClient.ReadDataTable("SELECT * FROM friends WHERE FriendID='" + Character.ID + "'");
            }

            if (frenddata != null)
            {
                foreach (DataRow row in frenddata.Rows)
                {
                    friends.Add(Friend.LoadFromDatabase(row));
                }
            }

            if (frenddataby != null)
            {
                foreach (DataRow row in frenddataby.Rows)
                {
                    friendsby.Add(Friend.LoadFromDatabase(row));
                }
            }

            foreach (var friend in friends)
            {
                DataTable frendsdata;
                using (var dbClient = Program.DatabaseManager.GetClient())
                {
                    frendsdata = dbClient.ReadDataTable("SELECT * FROM Characters WHERE CharID='" + friend.ID + "'");
                }

                if (frenddata != null)
                {
                    foreach (DataRow row in frendsdata.Rows)
                    {
                        friend.UpdateFromDatabase(row);
                    }
                }
            }

            foreach (var friend in friendsby)
            {
                DataTable frendsdata;
                using (var dbClient = Program.DatabaseManager.GetClient())
                {
                    frendsdata = dbClient.ReadDataTable("SELECT * FROM Characters WHERE CharID='" + friend.ID + "'");
                }

                if (frenddata != null)
                {
                    foreach (DataRow row in frendsdata.Rows)
                    {
                        friend.UpdateFromDatabase(row);
                    }
                }
            }

            UpdateFriendStates();
        }

        public void LoadMasterList()
        {
            DataTable Masterdata = null;
            using (var dbClient = Program.DatabaseManager.GetClient())
            {
                Masterdata = dbClient.ReadDataTable("SELECT * FROM Masters WHERE CharID='" + ID + "'");
            }

            if (Masterdata != null)
            {
                foreach (DataRow row in Masterdata.Rows)
                {
                    var DBMember = MasterMember.LoadFromDatabase(row);
                    MasterList.Add(DBMember);
                    if (DBMember.IsOnline)
                    {
                        DBMember.SetMemberStatus(true, Client.Character.Character.Name);
                    }
                }
            }
        }

        public void BroucastPacket(Packet pPacket)
        {
            InterHandler.SendGetCharacterBroaucast(this, pPacket);
        }

        public void ChangeFrendMap(string mapname)
        {
            foreach (var friend in friends)
            {
                var client = ClientManager.Instance.GetClientByCharname(friend.Name);
                if (client == null) return;
                using (var packet = new Packet(SH21Type.FriendChangeMap))
                {
                    packet.WriteString(Character.Name, 16);
                    packet.WriteString(mapname, 12);
                    client.SendPacket(packet);
                }
            }
        }

        public void ChangeMap(int oldmap)
        {
            InterHandler.SendChangeMap(this, oldmap);
        }

        public void WriteBlockList()
        {
            if (BlocketUser.Count > 0)
            {
                using (var packet = new Packet(SH42Type.BlockList))
                {
                    packet.WriteUShort((ushort) BlocketUser.Count);
                    foreach (var charname in BlocketUser)
                    {
                        packet.WriteString(charname, 16);
                    }

                    Client.SendPacket(packet);
                }
            }
        }

        public void LoadGroup()
        {
            Group = GroupManager.Instance.GetGroupById(Character.GroupId);
            if (Group != null)
            {
                GroupMember = Group[Character.Name];
                UpdateGroupStatus();
            }
        }

        public void LoadEqupippet()
        {
            foreach (var eqp in Inventory.EquippedItems.Where(eq => eq.Slot < 0))
            {
                var realslot = (byte) (eqp.Slot * -1);
                if (Equips.ContainsKey(realslot))
                {
                    Log.WriteLine(LogLevel.Warn, "{0} has duplicate equip in slot {1}", eqp.EquipId, realslot);
                    Equips.Remove(realslot);
                }

                Equips.Add(realslot, eqp.EquipId);
            }
        }

        public void UpdateMasterJoin()
        {
            Character.MasterJoin = DateTime.Now;
            Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE characters SET MasterJoin='" +
                                                             DateTime.Now.ToString("yyyy-MM-dd hh:mm") +
                                                             "' WHERE CharID='" + ID + "'");
        }

        public void SendPacketToAllOnlineMasters(Packet packet)
        {
            foreach (var pMember in MasterList)
            {
                pMember.pMember.SendPacket(packet);
            }
        }

        public Friend AddFriend(WorldCharacter pChar)
        {
            var pFrend = pChar.friends.Find(f => f.Name == pChar.Character.Name);
            var pFrendby = pChar.friendsby.Find(f => f.Name == pChar.Character.Name);
            var friend = Friend.Create(pChar);
            if (pFrend != null)
            {
                Program.DatabaseManager.GetClient()
                    .ExecuteQuery("INSERT INTO Friends (CharID,FriendID,Pending) VALUES ('" + pChar.Character.ID +
                                  "','" + Character.ID + "','1')");
            }

            if (pFrendby == null)
            {
                friendsby.Add(friend);
            }

            Program.DatabaseManager.GetClient().ExecuteQuery("INSERT INTO Friends (CharID,FriendID) VALUES ('" +
                                                             Character.ID + "','" + pChar.Character.ID + "')");
            friends.Add(friend);

            return friend;
        }

        public bool DeleteFriend(string pName)
        {
            var friend = friends.Find(f => f.Name == pName);
            var friendby = friendsby.Find(f => f.Name == pName);
            if (friend != null)
            {
                var result = friends.Remove(friend);
                if (result)
                {
                    if (friendsby != null)
                    {
                        Program.DatabaseManager.GetClient()
                            .ExecuteQuery("DELETE FROM friends WHERE CharID=" + friend.ID + " AND FriendID=" + ID);
                        friendsby.Remove(friendby);
                    }

                    Program.DatabaseManager.GetClient()
                        .ExecuteQuery("DELETE FROM friends WHERE CharID=" + ID + " AND FriendID=" + friend.ID);
                }

                UpdateFriendStates();
                return result;
            }

            return false;
        }

        public void LevelUp(byte level)
        {
            CharacterManager.invokeLevelUp(this);
        }

        public void SendReciveMasterCoper()
        {
            if (Character.ReviveCoper > 0)

                using (var packet = new Packet(SH37Type.SendRecivveCopper))
                {
                    packet.WriteLong(Character.ReviveCoper);
                    Client.SendPacket(packet);
                }
        }

        public void UpdateFriendsStatus(bool state, WorldClient sender)
        {
            if (friendsby == null)
                return;

            foreach (var frend in friendsby)
            {
                var client = ClientManager.Instance.GetClientByCharId((int) frend.UniqueID);

                if (client != null)
                {
                    if (state)
                    {
                        if (client != sender)
                            frend.IsOnline = true;
                        frend.Online(client, sender);
                    }
                    else
                    {
                        frend.IsOnline = false;
                        frend.Offline(client, Character.Name);
                    }
                }
            }
        }

        public void UpdateRecviveCoper()
        {
            var Master = MasterList.Find(m => m.IsMaster);
            if (Master != null)
            {
                Program.DatabaseManager.GetClient()
                    .ExecuteQuery("UPDATE character SET ReviveCoper=" + RecviveCoperMaster + " WHERE CharID =" +
                                  Master.CharID + "");
            }
        }

        public void UpdateFriendStates()
        {
            var unknowns = new List<Friend>();
            foreach (var friend in Friends)
            {
                if (friend.Name == null)
                {
                    unknowns.Add(friend);
                    continue;
                }

                var friendCharacter = ClientManager.Instance.GetClientByCharname(friend.Name);
                if (friendCharacter != null)
                {
                    friend.Update(friendCharacter.Character);
                }
                else
                {
                    friend.IsOnline = false;
                }
            }

            foreach (var friend in unknowns)
            {
                Friends.Remove(friend);
            }

            unknowns.Clear();
        }

        public void WriteFriendData(Packet pPacket)
        {
            foreach (var friend in Friends)
            {
                friend.WritePacket(pPacket);
            }
        }

        public void SetMasterOffline()
        {
            foreach (var Member in MasterList)
            {
                if (Member.pMember != null)
                {
                    Member.SetMemberStatus(false, Client.Character.Character.Name);
                }
            }
        }

        /*  public void SetGuildMemberStatusOffline()
        {
            try
            {
              
               if (this.Guild != null)
                {
                    GuildMember mMember = this.Guild.GuildMembers.Find(m => m.CharID == this.ID);
                    mMember.isOnline = false;
                    mMember.pClient = null;
                    foreach (var pMember in this.Guild.GuildMembers)
                    {
                        if (pMember.isOnline)
                        {
                            pMember.SendMemberStatus(false, this.Character.Name);
                        }
                    }
                }
                if(this.Academy != null)
                {
                    AcademyMember mMember = this.Academy.AcademyMembers.Find(m => m.CharID == this.ID);
                    mMember.isOnline = false;
                    mMember.pClient = null;
                    foreach (var pMember in this.Academy.AcademyMembers)
                    {
                        if (pMember.isOnline)
                        {
                            pMember.SendMemberStatus(false, this.Character.Name);
                        }
                    }
                    foreach (var pMember in  this.Academy.Guild.GuildMembers)
                    {
                        if (pMember.isOnline)
                        {
                            AcademyMember.SetOffline(this.Character.Name, pMember.pClient);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Error, "Failed Load Guild {0} {1}", this.ID, ex.Message.ToString());
            }
        }*/
        public void Loggeout(WorldClient pChar)
        {
            /*this.IsIngame = false;
            this.UpdateRecviveCoper();
			this.UpdateFriendsStatus(false,pChar);
			this.UpdateFriendStates();
            this.SetGuildMemberStatusOffline();*/
        }

        public void RemoveGroup()
        {
            Group = null;
            GroupMember = null;
            var query = $"UPDATE `characters` SET GroupID = 'NULL' WHERE CharID =  '{ID}'";
            Program.DatabaseManager.GetClient().ExecuteQuery(query);
        }

        public bool Delete()
        {
            if (IsDeleted) return false;
            try
            {
                Program.DatabaseManager.GetClient()
                    .ExecuteQuery("DELETE FROM characters WHERE CharID='" + Character.ID + "'");

                IsDeleted = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error deleting character: {0}", ex.ToString());
                return false;
            }
        }

        public ushort GetEquipBySlot(ItemSlot slot)
        {
            if (Equips.ContainsKey((byte) slot))
            {
                return Equips[(byte) slot];
            }

            return ushort.MaxValue;
        }

        public static string ByteArrayToStringForBlobSave(byte[] ba)
        {
            var hex = BitConverter.ToString(ba);
            return hex.Replace("-", ",");
        }

        public void SetQuickBarData(byte[] pData)
        {
            Character.QuickBar = pData;
            var data = ByteArrayToStringForBlobSave(Character.QuickBar) ??
                          ByteArrayToStringForBlobSave(new byte[1024]);
            Program.DatabaseManager.GetClient()
                .ExecuteQuery("UPDATE Characters SET QuickBar='" + data + "' WHERE CharID='" + Character.ID + "';");
        }

        public void SetQuickBarStateData(byte[] pData)
        {
            Character.QuickBarState = pData;
            var data = ByteArrayToStringForBlobSave(Character.QuickBarState) ??
                          ByteArrayToStringForBlobSave(new byte[24]);
            Program.DatabaseManager.GetClient()
                .ExecuteQuery("UPDATE Characters SET QuickBarState='" + data + "' WHERE CharID='" + Character.ID + "'");
        }

        public void SetGameSettingsData(byte[] pData)
        {
            Character.GameSettings = pData;
            var data = ByteArrayToStringForBlobSave(Character.GameSettings) ??
                          ByteArrayToStringForBlobSave(new byte[64]);
            Program.DatabaseManager.GetClient()
                .ExecuteQuery("UPDATE Characters SET GameSettings='" + data + "' WHERE CharID='" + Character.ID + "';");
        }

        public void SetClientSettingsData(byte[] pData)
        {
            Character.ClientSettings = pData;
            var data = ByteArrayToStringForBlobSave(Character.ClientSettings) ??
                          ByteArrayToStringForBlobSave(new byte[392]);
            Program.DatabaseManager.GetClient()
                .ExecuteQuery(
                    "UPDATE Characters SET ClientSettings='" + data + "' WHERE CharID='" + Character.ID + "';");
        }

        public void SetShortcutsData(byte[] pData)
        {
            Character.Shortcuts = pData;
            var data = ByteArrayToStringForBlobSave(Character.Shortcuts) ??
                          ByteArrayToStringForBlobSave(new byte[308]);
            Program.DatabaseManager.GetClient()
                .ExecuteQuery("UPDATE Characters SET Shortcuts='" + data + "' WHERE CharID='" + Character.ID + "';");
        }

        internal void OnGotIngame()
        {
            LoadGroup();

            if (GotIngame != null)
                GotIngame(this, new EventArgs());
        }

        public void OneIngameLoginLoad()
        {
            UpdateFriendsStatus(true, Client); //Write Later As Event
            WriteBlockList();
            LoadMasterList();
            SendReciveMasterCoper();
            /* 
             LoadGuild();*/
            Handler2.SendClientTime(Client, DateTime.Now);
        }

        public void ChangeMoney(long NewMoney)
        {
            Character.Money = NewMoney;
            using (var packet = new InterPacket(InterHeader.UpdateMoneyFromWorld))
            {
                packet.WriteInt(Character.ID);
                packet.WriteLong(NewMoney);
            }
        }

        private void UpdateGroupStatus()
        {
            GroupMember.IsOnline = IsIngame;
            Group.AnnouncePartyList();
        }
    }
}