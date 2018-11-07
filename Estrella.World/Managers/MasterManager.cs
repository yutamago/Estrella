using System;
using System.Collections.Generic;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.World.Data;
using Estrella.World.Data.MasterSystem;
using Estrella.World.InterServer;
using Estrella.World.Networking;

namespace Estrella.World.Managers
{
    [ServerModule(InitializationStage.Clients)]
    public class MasterManager
    {
        public MasterManager()
        {
            _pMasterRequests = new List<MasterRequest>();
        }

        [InitializerMethod]
        public static bool Initialize()
        {
            Instance = new MasterManager();
            return true;
        }

        public static MasterManager Instance { get; private set; }
        private readonly List<MasterRequest> _pMasterRequests;

        public void AddMasterRequest(WorldClient pClient, string target)
        {
            var request = new MasterRequest(target, pClient);
            var response = new MasterRequestResponse(request);

            if (!response.ResponseAnswer) return;
            response.SendMasterRequest();
            _pMasterRequests.Add(request);
        }

        public void RemoveMasterRequest(WorldClient pClient)
        {
            var request = _pMasterRequests.Find(d => Equals(d.InvitedClient, pClient));
            _pMasterRequests.Remove(request);
        }

        public void RemoveMasterMember(WorldClient pClient)
        {
            var pMember = pClient.Character.MasterList.Find(d => d.IsMaster);
            if (pMember != null)
            {
                SendApprenticeRemoveMaster(pMember.pMember, pClient.Character.Character.Name);
                var member =
                    pMember.pMember.Character.MasterList.Find(d => d.pMemberName == pClient.Character.Character.Name);
                pMember.pMember.Character.MasterList.Remove(member);
            }

            pMember.RemoveFromDatabase();
            pMember.RemoveFromDatabase(pMember.MasterID, pClient.Character.Character.Name);
            pClient.Character.MasterList.Remove(pMember);
            pClient.Character.UpdateMasterJoin();
            SendMasterRemoveResponse(pClient);
        }

        public void ApprenticeLevelUp(WorldCharacter pChar)
        {
            var pMember = pChar.MasterList.Find(d => d.IsMaster);

            if (pMember == null) return;

            // TODO: Add Break if the difference is greater than 5
            AddApprenticeReward(pChar);
            MasterMember.UpdateLevel(pChar.Character.CharLevel, pChar.Character.Name);
            if (pMember.pMember.Character.Client != null)
                SendApprenticeLevelUp(pMember.pMember, pChar.Character.Name, pChar.Character.CharLevel);
        }

        public void RemoveMasterMember(WorldCharacter pChar, string name)
        {
            var pMember = pChar.MasterList.Find(d => d.pMemberName == name);
            var pClient = ClientManager.Instance.GetClientByCharname(name);
            if (pClient != null)
            {
                SendApprenticeRemoveMaster(pClient, pMember.pMemberName);
                pClient.Character.MasterList.Remove(pMember);
            }

            pMember.RemoveFromDatabase();
            pMember.RemoveFromDatabase(pChar.Character.ID, pMember.pMemberName);
            pChar.MasterList.Remove(pMember);
            pChar.UpdateMasterJoin();
        }

        public void MasterRequestAccept(string requesterName, string targetName)
        {
            var target = ClientManager.Instance.GetClientByCharname(targetName);
            var requester = ClientManager.Instance.GetClientByCharname(requesterName);
            var response = new MasterRequestResponse(target, requester);
            if (response.ResponseAnswer)
            {
                var reqMember = new MasterMember(requester, target.Character.ID);
                var targetM = new MasterMember(target, requester.Character.ID);
                target.Character.MasterList.Add(reqMember);
                requester.Character.MasterList.Add(targetM);
                reqMember.AddToDatabase();
                targetM.IsMaster = true;
                targetM.AddToDatabase();
                SendMasterRequestAccept(requester, targetName);
            }
            else
            {
                var rRequest = _pMasterRequests.Find(d => Equals(d.InvitedClient, requester));
                _pMasterRequests.Remove(rRequest);
            }
        }

        private void SendMasterRemoveResponse(WorldClient pClient)
        {
            using (var packet = new Packet(SH37Type.SendMasterResponseRemove))
            {
                packet.WriteByte(0);
                packet.WriteUShort(0x1740);
                pClient.SendPacket(packet);
            }
        }

        private void SendApprenticeRemoveMaster(WorldClient pClient, string name)
        {
            using (var packet = new Packet(SH37Type.SendApprenticeRemoveMaster))
            {
                packet.WriteString(name, 16);
                packet.WriteByte(0); //isonline?
                pClient.SendPacket(packet);
            }
        }

        private void SendApprenticeLevelUp(WorldClient pClient, string charname, byte level)
        {
            using (var packet = new Packet(SH37Type.SendApprenticeLevelUp))
            {
                packet.WriteString(charname, 16);
                packet.WriteByte(level);
                pClient.SendPacket(packet);
            }
        }

        private void SendMasterRequestAccept(WorldClient pClient, string targetName)
        {
            using (var packet = new Packet(SH37Type.SendMasterRequestAccept))
            {
                packet.WriteString(targetName, 16);
                pClient.SendPacket(packet);
            }
        }

        private void AddApprenticeReward(WorldCharacter pChar)
        {
            var rewards = DataProvider.Instance.MasterRewards.FindAll(d =>
                (byte) d.Job == pChar.Character.Job && d.Level == pChar.Character.CharLevel);
            var rr = new MasterRewardItem
            {
                ItemID = 250,
                Count = 1
            };
            var conn = Program.GetZoneByMap(pChar.Character.PositionInfo.Map);
            if (conn == null)
                return;

            using (var packet = new Packet(SH37Type.SendApprenticeReward))
            {
                packet.WriteByte((byte) rewards.Count); //count
                foreach (var pReward in rewards)
                {
                    packet.WriteUShort(pReward.ItemID);
                    packet.WriteByte(pReward.Count);
                    packet.WriteByte(0); //unk
                    InterHandler.SendAddReward(conn, pReward.ItemID, pReward.Count, pChar.Character.Name);
                }

                pChar.Client.SendPacket(packet);
            }
        }

        public void SendMasterList(WorldClient pClient)
        {
            if (pClient.Character.MasterList.Count == 0)
                return;

            using (var packet = new Packet(SH37Type.SendMasterList))
            {
                var master = pClient.Character.MasterList.Find(d => d.IsMaster);
                if (master != null)
                {
                    var nowyear = (master.RegisterDate.Year - 1920 << 1) | Convert.ToByte(master.IsOnline);
                    var nowmonth = (master.RegisterDate.Month << 4) | 0x0F;
                    packet.WriteString(master.pMemberName, 16);
                    packet.WriteByte((byte) nowyear);
                    packet.WriteByte((byte) nowmonth);
                    packet.WriteByte((byte) DateTime.Now.Day);
                    packet.WriteByte(0x01); //unk
                    packet.WriteByte(master.Level);
                    packet.WriteByte(0); //unk
                    packet.WriteByte(0x03); //unk
                    var count = pClient.Character.MasterList.Count - 1;
                    packet.WriteUShort((ushort) count);
                }
                else
                {
                    var now = DateTime.Now;
                    var nowyear = (now.Year - 1920 << 1) | 1;
                    var nowmonth = (now.Month << 4) | 0x0F;
                    packet.WriteString("", 16);
                    packet.WriteByte((byte) nowyear);
                    packet.WriteByte((byte) nowmonth);
                    packet.WriteByte((byte) now.Day);
                    packet.WriteByte(0x01); //unk
                    packet.WriteByte(1);
                    packet.WriteByte(0); //unk
                    packet.WriteByte(0x03); //unk
                    packet.WriteUShort((ushort) pClient.Character.MasterList.Count);
                    //tODO when master null
                }

                foreach (var member in pClient.Character.MasterList)
                {
                    packet.WriteString(member.pMemberName, 16);
                    var year = (member.RegisterDate.Year - 1920 << 1) | Convert.ToUInt16(member.IsOnline);
                    var month = (member.RegisterDate.Month << 4) | 0x0F;
                    packet.WriteByte((byte) year);
                    packet.WriteByte((byte) month);
                    packet.WriteByte(0xB9);
                    packet.WriteByte(0x11); //unk
                    packet.WriteByte(member.Level);
                    packet.WriteByte(0); //unk
                }

                pClient.SendPacket(packet);
            }
        }
    }
}