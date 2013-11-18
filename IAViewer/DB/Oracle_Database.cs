using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace IAViewer.DB
{
    public class Oracle_Database : Database, IDatabase
    {

        public static readonly Database.DatabaseTypes _databaseType = Database.DatabaseTypes.ORACLE_DATABASE;
        public static readonly String _databaseName = Database.DatabaseTypes.ORACLE_DATABASE.ToString();
        public static IDatabase _database = new Oracle_Database();

        new public string _ConnectionString { get; private set; }
        new public string _UserName { get; private set; }
        new public string _Password { get; private set; }

        public Oracle_Database(String connectionString, String userName, String password)
            : base(connectionString, userName, password)
        {
            this._ConnectionString = connectionString;
            this._UserName = userName;
            this._Password = password;
        }

        public Oracle_Database()
        {
            DBConfiguration dbConfig = DBConfiguration.GetDBConfiguration();
            _ConnectionString = dbConfig.ConnectionString;
            _UserName = dbConfig.UserName;
            _Password = dbConfig.Password;
        }

        public void CloseConnection()
        {

        }

        public void ExecuteNonQuery(String command)
        {

        }

        public SqlDataReader ExecuteQuery(String command)
        {

            return null;
        }

        public static void Register()
        {
            DBFactory.Register(_databaseName, _database);
        }

        public Func<IDatabase> GetDatabaseGenerator()
        {
            Func<IDatabase> _databaseGenerator = new Func<Oracle_Database>(() => new Oracle_Database());
            return _databaseGenerator;
        }
    }
}
