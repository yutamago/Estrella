using System;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Data;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.Zone.Data;
using Estrella.Zone.Game;
using Estrella.Zone.Networking;
using Estrella.Zone.Networking.Security;

namespace Estrella.Zone.Handlers
{
    public sealed class Handler8
    {
        [PacketHandler(CH8Type.ChatNormal)]
        public static void NormalChatHandler(ZoneClient client, Packet packet)
        {
            byte len;
            string text;
            if (!packet.TryReadByte(out len) || !packet.TryReadString(out text, len))
            {
                Log.WriteLine(LogLevel.Warn, "Could not parse normal chat from {0}.", client.Character.Name);
                return;
            }

            if (client.Admin > 0 && (text.StartsWith("&") || text.StartsWith("/")))
            {
                CommandLog.Instance.LogCommand(client.Character.Name, text);
                var status = CommandHandler.Instance.ExecuteCommand(client.Character, text.Split(' '));
                switch (status)
                {
                    case CommandStatus.Error:
                        client.Character.DropMessage("Error executing command.");
                        break;
                    case CommandStatus.GMLevelTooLow:
                        client.Character.DropMessage("You do not have the privileges for this command.");
                        break;
                    case CommandStatus.NotFound:
                        client.Character.DropMessage("Command not found.");
                        break;
                }
            }
            else
            {
                var chatblock = client.Character.ChatCheck();
                if (chatblock == -1)
                {
                    ChatLog.Instance.LogChat(client.Character.Name, text, false);
                    SendNormalChat(client.Character, text, client.Admin > 0 ? (byte) 0x03 : (byte) 0x2a);
                }
                else
                {
                    Handler2.SendChatBlock(client.Character, chatblock);
                }
            }
        }

        [PacketHandler(CH8Type.By)]
        public static void GetByList(ZoneClient client, Packet packet)
        {
            var character = client.Character;

            byte handlerflags;
            packet.TryReadByte(out handlerflags);
            if (handlerflags == (ushort) NpcFlags.Vendor && character.CharacterInTarget != null)
            {
                var npc = character.CharacterInTarget as Npc;
                switch (npc.Point.RoleArg0)
                {
                    case "Weapon":
                        SendItemShopWeapon(npc, packet, character);
                        break;
                    case "Item":
                        SendItemShopItems(npc, packet, character);
                        break;
                    case "Guild":
                        SendItemShopItems(npc, packet, character);
                        break;
                    case "WeaponTitle":
                        SendItemShopItems(npc, packet, character);
                        break;
                    case "Skill":
                        SendItemShopSkill(npc, packet, character);
                        break;
                    case "SoulStone":
                        SendItemShopStones(npc, packet, character);
                        break;
                    default:
                        Log.WriteLine(LogLevel.Error, "Unhandelt Npc VendorType : {0}", npc.Point.RoleArg0);
                        break;
                }
            }
        }

        public static void SendItemShopStones(Npc npc, Packet packet, ZoneCharacter character)
        {
            using (packet = new Packet(SH15Type.HandlerStone))
            {
                packet.WriteInt(character.BaseStats.HPStoneEffectID); //useeffectid
                packet.WriteInt(character.BaseStats.MaxSoulHP); //maxhpstones
                packet.WriteInt(character.BaseStats.PriceHPStone); //hp stines price
                packet.WriteInt(character.BaseStats.SPStoneEffectID); //useeffectid
                packet.WriteInt(character.BaseStats.MaxSoulSP); //sp max stones
                packet.WriteInt(character.BaseStats.PriceSPStone); //spstones price
                character.Client.SendPacket(packet);
            }
        }

        public static void SendItemShopItems(Npc npc, Packet packet, ZoneCharacter character)
        {
            using (packet = new Packet(SH15Type.HandlerTitel))
            {
                if (npc.Point.VendorItems != null)
                {
                    var count = npc.Point.VendorItems.Count;

                    packet.WriteByte((byte) count);
                    packet.WriteInt(0); //unk
                    if (count > 0)
                    {
                        foreach (var items in npc.Point.VendorItems)
                        {
                            packet.WriteUShort(items.ItemID);
                            packet.WriteByte(items.InvSlot);
                        }
                    }
                    else
                    {
                        packet.WriteUShort(0);
                        packet.WriteByte(0); //unk
                    }
                }
                else
                {
                    packet.WriteByte(0);
                    packet.WriteInt(0); //unk
                    packet.WriteUShort(0);
                    packet.WriteByte(0); //unk
                }

                character.Client.SendPacket(packet);
            }
        }

        public static void SendItemShopSkill(Npc npc, Packet packet, ZoneCharacter character)
        {
            using (packet = new Packet(SH15Type.HanlderSkill))
            {
                if (npc.Point.VendorItems != null)
                {
                    var count = npc.Point.VendorItems.Count;

                    packet.WriteByte((byte) count);
                    packet.WriteInt(0); //unk
                    foreach (var items in npc.Point.VendorItems)
                    {
                        packet.WriteUShort(items.ItemID);
                        packet.WriteByte(items.InvSlot);
                    }
                }
                else
                {
                    packet.WriteByte(0);
                    packet.WriteInt(0); //unk
                    packet.WriteUShort(0);
                    packet.WriteByte(0); //unk
                }

                character.Client.SendPacket(packet);
            }
        }

        public static void SendItemShopWeapon(Npc npc, Packet packet, ZoneCharacter character)
        {
            using (packet = new Packet(SH15Type.HandlerWeapon))
            {
                if (npc.Point.VendorItems != null)
                {
                    var count = npc.Point.VendorItems.Count;

                    packet.WriteByte((byte) count);
                    packet.WriteInt(0); //unk
                    foreach (var items in npc.Point.VendorItems)
                    {
                        packet.WriteUShort(items.ItemID);
                        packet.WriteByte(items.InvSlot); //unk
                    }
                }
                else
                {
                    packet.WriteByte(0);
                    packet.WriteInt(0); //unk
                    packet.WriteUShort(0);
                    packet.WriteByte(0); //unk
                }

                character.Client.SendPacket(packet);
            }
        }

        [PacketHandler(CH8Type.Shout)]
        public static void ShoutHandler(ZoneClient client, Packet packet)
        {
            var character = client.Character;
            byte len;
            string message;
            if (!packet.TryReadByte(out len) ||
                !packet.TryReadString(out message, len))
            {
                Log.WriteLine(LogLevel.Warn, "Could not read shout from {0}.", character.Name);
                return;
            }

            var shoutcheck = character.ShoutCheck();
            if (shoutcheck > 0)
            {
                Handler2.SendChatBlock(character, shoutcheck);
            }
            else
            {
                ChatLog.Instance.LogChat(client.Character.Name, message, true);
                using (var broad = Shout(character.Name, message))
                {
                    character.Map.Broadcast(broad);
                }
            }
        }

        [PacketHandler(CH8Type.BeginInteraction)]
        public static void BeginInteractionHandler(ZoneClient client, Packet packet)
        {
            ushort entityid;
            if (!packet.TryReadUShort(out entityid))
            {
                Log.WriteLine(LogLevel.Warn, "Error reading interaction attempt.");
                return;
            }

            var character = client.Character;


            MapObject obj;
            if (character.Map.Objects.TryGetValue(entityid, out obj))
            {
                var npc = obj as Npc;
                client.Character.CharacterInTarget = obj;
                if (npc != null)
                {
                    if (npc.Gate != null)
                    {
                        MapInfo mi = null;
                        if (DataProvider.Instance.MapsByName.TryGetValue(npc.Gate.MapServer, out mi))
                        {
                            var q = new Question($"Do you want to move to {mi.FullName} field?",
                                AnswerOnGateQuestion, npc);
                            q.Add("Yes", "No");
                            q.Send(character, 500);
                            character.Question = q;
                        }
                        else
                        {
                            character.DropMessage("You can't travel to this place.");
                        }
                    }
                    else
                    {
                        SendNpcInteraction(client, npc);
                    }
                }
            }
            else Log.WriteLine(LogLevel.Warn, "{0} selected invalid object.", character.Name);
        }


        private static void AnswerOnGateQuestion(ZoneCharacter character, byte answer)
        {
            var npc = character.Question.Object as Npc;
            MapInfo mi = null;
            if (DataProvider.Instance.MapsByName.TryGetValue(npc.Gate.MapServer, out mi))
            {
                switch (answer)
                {
                    case 0:
                        character.ChangeMap(mi.ID, npc.Gate.CoordX, npc.Gate.CoordY);
                        break;

                    case 1: break;
                    default:
                        Log.WriteLine(LogLevel.Warn, "Invalid gate question response.");
                        break;
                }
            }
        }


        public static void SendNpcInteraction(ZoneClient client, Npc npc)
        {
            switch (npc.Point.RoleArg0)
            {
                case "Guild":
                    if (npc.Point.Role == "NPCMenu")
                    {
                        using (var packet = new Packet(SH15Type.GuildNpcReqest))
                        {
                            client.SendPacket(packet);
                        }
                    }
                    else
                    {
                        using (var packet = new Packet(SH8Type.Interaction))
                        {
                            packet.WriteUShort(npc.ID);
                            client.SendPacket(packet);
                        }
                    }

                    break;
                case "Quest":
                    //:TODO Quest Proggresion
                    using (var packet = new Packet(SH8Type.Interaction))
                    {
                        packet.WriteUShort(npc.ID);
                        client.SendPacket(packet);
                    }

                    Console.WriteLine(npc.Point.RoleArg0);
                    break;
                default:
                    using (var packet = new Packet(SH8Type.Interaction))
                    {
                        packet.WriteUShort(npc.ID);
                        client.SendPacket(packet);
                    }

                    break;
            }
        }

        [PacketHandler(CH8Type.BeginRest)]
        public static void BeginRestHandler(ZoneClient client, Packet packet)
        {
            client.Character.Rest(true);
        }

        [PacketHandler(CH8Type.EndRest)]
        public static void EndRestHandler(ZoneClient client, Packet packet)
        {
            client.Character.Rest(false);
        }

        public static void SendEndRestResponse(ZoneClient client)
        {
            using (var packet = new Packet(SH8Type.EndRest))
            {
                packet.WriteUShort(0x0a81);
                client.SendPacket(packet);
            }
        }

        public static void SendBeginRestResponse(ZoneClient client, ushort value)
        {
            /*  0x0A81 - OK to rest
                   0x0A82 - Can't rest on mount
                   0x0A83 - Too close to NPC*/
            using (var packet = new Packet(SH8Type.BeginRest))
            {
                packet.WriteUShort(value);
                client.SendPacket(packet);
            }
        }

        public static Packet BeginDisplayRest(ZoneCharacter character)
        {
            var packet = new Packet(SH8Type.BeginDisplayRest);
            packet.WriteUShort(character.MapObjectID);
            packet.WriteUShort(character.House.ItemID);
            packet.Fill(10, 0xff);
            return packet;
        }

        public static Packet EndDisplayRest(ZoneCharacter character)
        {
            var packet = new Packet(SH8Type.EndDisplayRest);
            packet.WriteUShort(character.MapObjectID);
            character.WriteLook(packet);
            character.WriteEquipment(packet);
            character.WriteRefinement(packet);
            return packet;
        }

        [PacketHandler(CH8Type.Emote)]
        public static void EmoteHandler(ZoneClient client, Packet packet)
        {
            var character = client.Character;
            byte action;
            if (!packet.TryReadByte(out action))
            {
                Log.WriteLine(LogLevel.Warn, "{0} did empty emote.", character.Name);
                return;
            }

            if (action > 74)
            {
                character.CheatTracker.AddCheat(CheatTypes.Emote, 500);
                return;
            }

            using (var broad = Animation(character, action))
            {
                character.Broadcast(broad, true);
            }
        }

        public static Packet Animation(ZoneCharacter character, byte id)
        {
            var packet = new Packet(SH8Type.Emote);
            packet.WriteUShort(character.MapObjectID);
            packet.WriteByte(id);
            return packet;
        }

        public static Packet Shout(string charname, string text)
        {
            var packet = new Packet(SH8Type.Shout);
            packet.WriteString(charname, 16);
            packet.WriteByte(0); //color
            packet.WriteByte((byte) text.Length);
            packet.WriteString(text, text.Length);
            return packet;
        }

        [PacketHandler(CH8Type.Jump)]
        public static void JumpHandler(ZoneClient client, Packet packet)
        {
            var character = client.Character;
            if (character.State == PlayerState.Normal || character.State == PlayerState.Mount)
            {
                using (var broad = Jump(character))
                {
                    character.Broadcast(broad);
                }
            }
            else character.CheatTracker.AddCheat(CheatTypes.InvalidMove, 50);
        }

        public static Packet Jump(ZoneCharacter character)
        {
            var packet = new Packet(SH8Type.Jump);
            packet.WriteUShort(character.MapObjectID);
            return packet;
        }

        [PacketHandler(CH8Type.Run)]
        public static void RunHandler(ZoneClient client, Packet packet)
        {
            HandleMovement(client.Character, packet, true);
        }

        [PacketHandler(CH8Type.Stop)]
        public static void StopHandler(ZoneClient client, Packet packet)
        {
            HandleMovement(client.Character, packet, true, true);
        }

        [PacketHandler(CH8Type.Walk)]
        public static void WalkHandler(ZoneClient client, Packet packet)
        {
            HandleMovement(client.Character, packet, false);
        }

        private static void HandleMovement(ZoneCharacter character, Packet packet, bool run, bool stop = false)
        {
            if (character.State == PlayerState.Dead || character.State == PlayerState.Resting ||
                character.State == PlayerState.Vendor)
            {
                character.CheatTracker.AddCheat(CheatTypes.InvalidMove, 50);
                return;
            }

            int newX, oldX, newY, oldY;
            if (!stop)
            {
                if (!packet.TryReadInt(out oldX) || !packet.TryReadInt(out oldY) ||
                    !packet.TryReadInt(out newX) || !packet.TryReadInt(out newY))
                {
                    Log.WriteLine(LogLevel.Warn, "Invalid movement packet detected.");
                    return;
                }
            }
            else
            {
                if (!packet.TryReadInt(out newX) || !packet.TryReadInt(out newY))
                {
                    Log.WriteLine(LogLevel.Warn, "Invalid stop packet detected.");
                    return;
                }

                oldX = character.Position.X;
                oldY = character.Position.Y;
            }

            if (character.Map.Block != null)
            {
                if (!character.Map.Block.CanWalk(newX, newY))
                {
                    Log.WriteLine(LogLevel.Debug, "Blocking walk at {0}:{1}.", newX, newY);
                    SendPositionBlock(character, newX, newY);
                    SendTeleportCharacter(character, oldX, oldY);
                    return;
                }
            }

            var distance = Vector2.Distance(newX, oldX, newY, oldY);
            if (run && distance > 500d || !run && distance > 400d) //TODO: mounts don't check with these speeds
            {
                character.CheatTracker.AddCheat(CheatTypes.Speedwalk, 50);
                return;
            }

            if (!stop)
            {
                var deltaY = newY - character.Position.Y;
                var deltaX = newX - character.Position.X;
                var radians = Math.Atan((double) deltaY / deltaX);
                var angle = radians * (180 / Math.PI);
                character.Rotation = (byte) (angle / 2);
            }

            foreach (var member in character.Party)
            {
                if (member.Key != character.Name)
                {
                    using (var ppacket = new Packet(SH14Type.UpdatePartyMemberLoc))
                    {
                        ppacket.WriteByte(1); //unk
                        ppacket.WriteString(character.Name, 16);
                        ppacket.WriteInt(character.Position.X);
                        ppacket.WriteInt(character.Position.Y);
                        member.Value.SendPacket(ppacket);
                    }
                }
            }

            character.Move(oldX, oldY, newX, newY, !run, stop); // hehe
        }

        public static Packet MoveObject(MapObject obj, int oldx, int oldy, bool walk, ushort speed = (ushort) 115)
        {
            var packet = new Packet(walk ? SH8Type.Walk : SH8Type.Move);
            packet.WriteUShort(obj.MapObjectID);
            packet.WriteInt(oldx);
            packet.WriteInt(oldy);
            packet.WriteInt(obj.Position.X);
            packet.WriteInt(obj.Position.Y);
            packet.WriteUShort(speed);
            return packet;
        }

        public static Packet StopObject(MapObject obj)
        {
            var packet = new Packet(SH8Type.StopTele);
            packet.WriteUShort(obj.MapObjectID);
            packet.WriteInt(obj.Position.X);
            packet.WriteInt(obj.Position.Y);
            return packet;
        }

        public static void SendAdminNotice(ZoneClient client, string text)
        {
            using (var packet = new Packet(SH8Type.GmNotice))
            {
                packet.WriteByte((byte) text.Length);
                packet.WriteString(text, text.Length);
                client.SendPacket(packet);
            }
        }

        public static void SendPositionBlock(ZoneCharacter character, int x, int y)
        {
            using (var packet = new Packet(SH8Type.BlockWalk))
            {
                packet.WriteInt(x);
                packet.WriteInt(y);
                character.Client.SendPacket(packet);
            }
        }

        public static void SendTeleportCharacter(ZoneCharacter character, int x, int y)
        {
            using (var packet = new Packet(SH8Type.Teleport))
            {
                packet.WriteInt(x);
                packet.WriteInt(y);
                character.Client.SendPacket(packet);
            }
        }

        public static void SendNormalChat(ZoneCharacter character, string chat, byte color = (byte) 0x2a)
        {
            using (var packet = new Packet(SH8Type.ChatNormal))
            {
                packet.WriteUShort(character.MapObjectID);
                packet.WriteByte((byte) chat.Length);
                packet.WriteByte(color);
                packet.WriteString(chat, chat.Length);
                character.Broadcast(packet, true);
            }
        }
    }
}