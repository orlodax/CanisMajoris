namespace CanisDAL
{
    public sealed class DatabaseFactory
    {
        private DatabaseFactory() { }

        public static Database CreateDatabase(string DbType, string server, string port, string databaseName, string username, string password, string apikey)
        {
            switch (DbType)
            {
                case "MySql":
                    return new DBMySQL(server, port, databaseName, username, password, DbType, apikey);

                //case "SqLite":
                //    return new SQLiteDatabase();

                default:
                    return null;
            }
        }

        public static Database CreateDatabase(string connectionString)
        {
            return new DBSQLite(connectionString);
        }

    }
}
