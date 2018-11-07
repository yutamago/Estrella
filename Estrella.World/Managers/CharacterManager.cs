using System;
using Estrella.Database.DataStore;
using Estrella.Util;
using Estrella.World.Data;
using Estrella.World.Data.Guild;

namespace Estrella.World.Managers
{
    public delegate void CharacterEvent(WorldCharacter Character);

    [ServerModule(InitializationStage.CharacterManager)]
    public class CharacterManager
    {
        public static CharacterManager Instance { get; set; }
        public static event CharacterEvent CharacterLogin; //this wehen change map or another
        public static event CharacterEvent OnCharacterLogin;
        public static event CharacterEvent OnCharacterLogout;
        public static event CharacterEvent OnCharacterLevelUp;
        public static event CharacterEvent OnCharacterChangeMap;

        [InitializerMethod]
        public static bool init()
        {
            Instance = new CharacterManager();

            OnCharacterChangeMap += OnetestmapChange;
            return true;
        }

        public static void invokeLevelUp(WorldCharacter pChar)
        {
            OnCharacterLevelUp.Invoke(pChar);
        }

        public static void invokeLoggetInEvent(WorldCharacter pChar)
        {
            OnCharacterLogin.Invoke(pChar);
        }

        public static void InvokdeIngame(WorldCharacter pChar)
        {
            CharacterLogin.Invoke(pChar);
        }

        public static void InvokeLoggetOutInEvent(WorldCharacter pChar)
        {
            OnCharacterLogout.Invoke(pChar);
        }

        public static void InvokeChangeMapEvent(WorldCharacter pChar)
        {
            OnCharacterChangeMap.Invoke(pChar);
        }

        public static void OnetestmapChange(WorldCharacter pChar)
        {
            Console.WriteLine(pChar.Character.PositionInfo.Map);
            Console.WriteLine(pChar.Character.PositionInfo.XPos);
            Console.WriteLine(pChar.Character.PositionInfo.YPos);
        }

        public static void OneLoadGuildInCharacter(WorldCharacter pChar)
        {
            var dbClient = Program.DatabaseManager.GetClient();
            var GuildID = dbClient.ReadInt32("SELECT GuildID FROM guildmembers WHERE CharID='" + pChar.ID + "'");
            var AcademyID =
                dbClient.ReadInt32("SELECT GuildID FROM guildacademymembers WHERE CharID='" + pChar.ID + "'");
            if (AcademyID > 0 && GuildID == 0)
            {
                Guild g;
                if (!GuildManager.GetGuildByID(AcademyID, out g))
                    return;
                pChar.GuildAcademy = g.Academy;
                pChar.IsInGuildAcademy = true;
            }
            else if (GuildID > 0 && AcademyID == 0)
            {
                Guild g;
                if (!GuildManager.GetGuildByID(GuildID, out g))
                    return;
                pChar.Guild = g;
                pChar.GuildAcademy = g.Academy;
                pChar.IsInGuild = true;
                var GuildMember = g.Members.Find(m => m.Character.Character.Name == pChar.Character.Name);
                GuildMember.Character.Client = pChar.Client;
            }
        }

        public static bool GetLoggedInCharacter(string Name, out WorldCharacter pChar)
        {
            var pClient = ClientManager.Instance.GetClientByCharname(Name);
            pChar = null;
            if (pClient != null)
            {
                pChar = pClient.Character;
                return true;
            }

            return false;
        }

        public bool GetCharacterByID(int ID, out WorldCharacter pChar)
        {
            var pclient = ClientManager.Instance.GetClientByCharId(ID);
            if (pclient != null)
            {
                pChar = pclient.Character;
                return true;
            }

            pChar = null;
            var DBpChar = ReadMethods.ReadCharObjectByIDFromDatabase(ID, Program.DatabaseManager);
            var ReaderChar = new WorldCharacter(DBpChar, null);
            pChar = ReaderChar;
            if (DBpChar == null)
            {
                return false;
            }

            return true;
        }
    }
}