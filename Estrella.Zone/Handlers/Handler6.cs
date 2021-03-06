﻿using System;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.Zone.Data;
using Estrella.Zone.Game;
using Estrella.Zone.Managers;
using Estrella.Zone.Networking;

namespace Estrella.Zone.Handlers
{
    public sealed class Handler6
    {
        [PacketHandler(CH6Type.Teleporter)]
        public static void UseTeleporter(ZoneClient client, Packet packet)
        {
            byte anwser;
            if (packet.TryReadByte(out anwser))
            {
                using (var Packet = new Packet(SH6Type.TelePorter))
                {
                    Packet.WriteShort(6593); //code for normal teleport
                    client.SendPacket(Packet);
                }

                switch (anwser)
                {
                    case 0:
                        client.Character.ChangeMap(0, 4199, 4769); //Roumen

                        break;
                    case 1:
                        client.Character.ChangeMap(9, 11802, 10466); //Eldrine

                        break;
                    case 2:
                        client.Character.ChangeMap(75, 9069, 9312); //EldGbl02
                        break;
                    case 3:
                        client.Character.ChangeMap(5, 13658, 7812); //RouVal01

                        break;
                    default:
                        Log.WriteLine(LogLevel.Warn, "Unkown Teleport Answer {1}", anwser);
                        break;
                }
            }
        }

        [PacketHandler(CH6Type.TransferKey)]
        public static void TransferKeyHandler(ZoneClient client, Packet packet)
        {
            ushort randomID;
            string characterName, checksums; //TODO: check in securityclient
            if (!packet.TryReadUShort(out randomID) || !packet.TryReadString(out characterName, 16) ||
                !packet.TryReadString(out checksums, 832))
            {
                Log.WriteLine(LogLevel.Warn, "Invalid game transfer.");
                return;
            }

            var transfer = ClientManager.Instance.GetTransfer(characterName);
            if (transfer == null || transfer.HostIP != client.Host || transfer.RandID != randomID)
            {
                Log.WriteLine(LogLevel.Warn, "{0} tried to login without a valid client transfer.", client.Host);
                //Handler3.SendError(client, ServerError.INVALID_CREDENTIALS);
                Handler4.SendConnectError(client, ConnectErrors.RequestedCharacterIDNotMatching);
                return;
            }

            try
            {
                ClientManager.Instance.RemoveTransfer(characterName);

                var zonecharacter = new ZoneCharacter(transfer.CharID);
                if (zonecharacter.Character.AccountID != transfer.AccountID)
                {
                    Log.WriteLine(LogLevel.Warn, "Character is logging in with wrong account ID.");
                    Handler4.SendConnectError(client, ConnectErrors.RequestedCharacterIDNotMatching);
                    //Handler3.SendError(client, ServerError.INVALID_CREDENTIALS);
                    return;
                }

                client.Authenticated = true;
                client.Admin = transfer.Admin;
                client.AccountID = transfer.AccountID;
                client.Username = transfer.Username;
                client.Character = zonecharacter;
                zonecharacter.Client = client;
                //Zonecharacter.Client. = ;


                if (ClientManager.Instance.AddClient(client))
                {
                    zonecharacter.SendGetIngameChunk(); //TODO: interserver packet?
                    Log.WriteLine(LogLevel.Debug, "{0} logged in successfully!", zonecharacter.Name);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error loading character {0}: {1} - {2}", characterName,
                    ex.ToString(), ex.StackTrace);
                Handler4.SendConnectError(client, ConnectErrors.ErrorInCharacterInfo);
            }
        }

        [PacketHandler(CH6Type.ClientReady)]
        public static void ClientReadyHandler(ZoneClient client, Packet packet)
        {
            if (client.Admin > 0)
            {
                client.Character.DropMessage("AdminLevel = {0}; ClientLoad = {1};", client.Admin,
                    ClientManager.Instance.ZoneLoad);
            }

            Handler4.SendUsablePoints(client);

            if (!client.Character.IsDead)
            {
                // Just logged on.
                client.Character.Map.FinalizeAdd(client.Character);
            }
            else
            {
                // Reviving, not readding for this one!
                MapInfo mi;
                if (DataProvider.Instance.MapsByID.TryGetValue(client.Character.MapID, out mi))
                {
                    client.Character.State = PlayerState.Normal;
                    client.Character.Map.SendCharacterLeftMap(client.Character, false);
                    client.Character.Position.X = mi.RegenX;
                    client.Character.Position.Y = mi.RegenY;
                    client.Character.Map.SendCharacterEnteredMap(client.Character);
                }

                client.Character.SetHP(client.Character.MaxHP / 4);
            }
        }

        public static Packet RemoveDrop(Drop drop)
        {
            var packet = new Packet(SH6Type.RemoveDrop);
            packet.WriteUShort(drop.ID);
            return packet;
        }

        public static void SendDetailedCharacterInfo(ZoneCharacter character)
        {
            using (var packet = new Packet(SH6Type.DetailedCharacterInfo))
            {
                character.WriteDetailedInfoExtra(packet);
                character.Client.SendPacket(packet);
            }
        }

        public static void SendChangeMap(ZoneCharacter character, ushort mapid, int x, int y)
        {
            using (var packet = new Packet(SH6Type.ChangeMap))
            {
                packet.WriteUShort(mapid);
                packet.WriteInt(x);
                packet.WriteInt(y);
                character.Client.SendPacket(packet);
            }
        }

        public static void SendChangeZone(ZoneCharacter character, ushort mapid, int x, int y, string ip, ushort port,
            ushort randomid)
        {
            using (var packet = new Packet(SH6Type.ChangeZone))
            {
                packet.WriteUShort(mapid);
                packet.WriteInt(x);
                packet.WriteInt(y);
                packet.WriteString(Settings.Instance.IP, 16);
                packet.WriteUShort(port);
                packet.WriteUShort(randomid);
                character.Client.SendPacket(packet);
            }
        }

        public static void SendError(ZoneCharacter character)
        {
            using (var packet = new Packet(SH6Type.Error))
            {
                character.Client.SendPacket(packet);
            }
        }
    }
}