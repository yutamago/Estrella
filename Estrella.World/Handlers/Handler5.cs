﻿using System;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.World.Data;
using Estrella.World.Networking;
using Estrella.World.Security;

namespace Estrella.World.Handlers
{
    public sealed class Handler5
    {
        [PacketHandler(CH5Type.ChangeCharacterName)]
        public static void ChangeCharNameHandler(WorldClient client, Packet packet)
        {
            byte Character_slot;
            string charname;
            if (!packet.TryReadByte(out Character_slot) || !packet.TryReadString(out charname, 16))
                return;
            using (var pack = new Packet(SH5Type.SendCharacterChangeNewName))
            {
                pack.WriteByte(Character_slot);
                pack.WriteString(charname, 16);
                pack.WriteUShort(208); //Responsecode?
            }

            //Todo ChangeinDatabase
        }

        [PacketHandler(CH5Type.CreateCharacter)]
        public static void CreateCharHandler(WorldClient client, Packet packet)
        {
            string name;
            byte slot, jobGender, hair, color, style;
            if (!packet.TryReadByte(out slot) || !packet.TryReadString(out name, 20) ||
                !packet.TryReadByte(out jobGender) || !packet.TryReadByte(out hair) ||
                !packet.TryReadByte(out color) || !packet.TryReadByte(out style))
            {
                Log.WriteLine(LogLevel.Warn, "Error reading create char for {0}", client.Username);
                return;
            }

            if (DatabaseChecks.IsCharNameUsed(name))
            {
                SendCharCreationError(client, CreateCharError.NameTaken);
                return;
            }

            if (DataProvider.Instance.IsBadName(name))
            {
                SendCharCreationError(client, CreateCharError.NameInUse);
                return;
            }

            var isMaleByte = (byte) ((jobGender >> 7) & 0x01);
            var classIDByte = (byte) ((jobGender >> 2) & 0x1F);
            var job = (Job) classIDByte;
            switch (job)
            {
                case Job.Archer:
                case Job.Cleric:
                case Job.Fighter:
                case Job.Mage:
                case Job.Trickster:
                    //create character here
                    try
                    {
                        var wchar = client.CreateCharacter(name, slot, hair, color, style, job,
                            Convert.ToBoolean(isMaleByte));
                        SendCharOKResponse(client, wchar);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(LogLevel.Exception, "Error creating character for {0}: {1}", client.Username,
                            ex.InnerException.ToString());
                        SendCharCreationError(client, CreateCharError.FailedToCreate);
                    }

                    break;
                default:
                    SendCharCreationError(client, CreateCharError.WrongClass);
                    Log.WriteLine(LogLevel.Warn, "Invalid job ID at char creation from {0}", client.Username);
                    break;
            }
        }

        [PacketHandler(CH5Type.DeleteCharacter)]
        public static void DeleteCharacterHandler(WorldClient client, Packet packet)
        {
            byte slot;
            if (!packet.TryReadByte(out slot) || slot > 10 || !client.Characters.ContainsKey(slot))
            {
                Log.WriteLine(LogLevel.Warn, "{0} tried to delete character out of range.", client.Username);
                return;
            }

            var todelete = client.Characters[slot];

            if (todelete.Delete())
            {
                client.Characters.Remove(slot);
                SendCharDeleteOKResponse(client, slot);
            }
            else
            {
                Handler3.SendError(client, ServerError.DatabaseError);
            }
        }

        private static void SendCharDeleteOKResponse(WorldClient client, byte slot)
        {
            using (var packet = new Packet(SH5Type.CharDeleteOK))
            {
                packet.WriteByte(slot);
                client.SendPacket(packet);
            }
        }

        private static void SendCharOKResponse(WorldClient client, WorldCharacter character)
        {
            using (var packet = new Packet(SH5Type.CharCreationOK))
            {
                packet.WriteByte(1);
                PacketHelper.WriteBasicCharInfo(character, packet);
                client.SendPacket(packet);
            }
        }

        private static void SendCharCreationError(WorldClient client, CreateCharError error)
        {
            using (var packet = new Packet(SH5Type.CharCreationError))
            {
                packet.WriteUShort((ushort) error);
                client.SendPacket(packet);
            }
        }
    }
}