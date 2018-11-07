using Estrella.World.Data.Group;

namespace Estrella.World.Data
{
    public static class DatabaseHelper
    {
        public const string RemoveCharacterGroupQuery = "UPDATE `characters` SET GroupID = NULL WHERE Name = \'{0}\'";

        public const string UpdateCharacterGroupQuery =
            "UPDATE `characters` SET GroupID = '{0}' , IsGroupMaster = '{1}' WHERE Name = \'{2}\'";

        public static void RemoveCharacterGroup(string pName)
        {
            using (var con = Program.DatabaseManager.GetClient())
            {
                var q = string.Format(RemoveCharacterGroupQuery, pName);
                con.ExecuteQuery(q);
            }
        }

        public static void UpdateCharacterGroup(GroupMember pMember)
        {
            using (var client = Program.DatabaseManager.GetClient())
            {
                var q = string.Format(UpdateCharacterGroupQuery,
                    pMember.Group.Id,
                    pMember.Role == GroupRole.Master,
                    pMember.Character.ID);
                client.ExecuteQuery(q);
            }
        }
    }
}