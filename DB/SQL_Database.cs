using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAViewer.DB
{
    public class SQL_Database : Database, IDatabase
    {

        public static readonly Database.DatabaseTypes _databaseType = Database.DatabaseTypes.SQL_DATABASE;
        private static String _databaseName = Database.DatabaseTypes.SQL_DATABASE.ToString();
        public static IDatabase _database = new SQL_Database();

        public string _ConnectionString { get; private set; }
        public string _UserName { get; private set; }
        public string _Password { get; private set; }

        public SQL_Database(String connectionString, String userName, String password)
            : base(connectionString, userName, password)
        {
            this._ConnectionString = connectionString;
            this._UserName = userName;
            this._Password = password;
        }

        public SQL_Database()
        {
            DBConfiguration dbConfig = DBConfiguration.GetDBConfiguration();
            _ConnectionString = dbConfig.ConnectionString;
            _UserName = dbConfig.UserName;
            _Password = dbConfig.Password;
        }

        public void CloseConnection()
        {
            
        }

        public static void Register()
        {
            DBFactory.Register(_databaseName, _database);
        }

        public Func<IDatabase> GetDatabaseGenerator()
        {
            Func<IDatabase> _databaseGenerator = new Func<SQL_Database>(() => new SQL_Database());
            return _databaseGenerator;
        }
    }
}
