namespace Estrella.World.Security
{
    public static class DatabaseChecks
    {
        public static bool IsCharNameUsed(string name)
        {
            using (var dbClient = Program.DatabaseManager.GetClient())
            {
                var data = dbClient.ReadDataTable("Select CharID from characters  WHERE binary Name='" + name + "'");
                if (data == null) return true;

                switch (data.Rows.Count)
                {
                    case 0:
                        return false;
                    default:
                        return true;
                }
            }
        }
    }
}