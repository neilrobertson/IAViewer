using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace IAViewer.DB
{
    public class SQL_Database : Database, IDatabase
    {

        public static readonly Database.DatabaseTypes _databaseType = Database.DatabaseTypes.SQL_DATABASE;
        private static String _databaseName = Database.DatabaseTypes.SQL_DATABASE.ToString();
        public static IDatabase _database = new SQL_Database();

        new public string _ConnectionString { get; private set; }
        new public string _UserName { get; private set; }
        new public string _Password { get; private set; }

        private SqlConnection sqlConnection;

        public SQL_Database(String connectionString, String userName, String password)
            : base(connectionString, userName, password)
        {
            this._ConnectionString = connectionString;
            this._UserName = userName;
            this._Password = password;

            CreateConnection();
        }

        public SQL_Database()
        {
            DBConfiguration dbConfig = DBConfiguration.GetDBConfiguration();
            _ConnectionString = dbConfig.ConnectionString;
            _UserName = dbConfig.UserName;
            _Password = dbConfig.Password;

            CreateConnection();
        }

        private void CreateConnection()
        {
            sqlConnection = new SqlConnection(_ConnectionString);
            sqlConnection.Open();
        }

        public void CloseConnection()
        {
            sqlConnection.Close();
        }

        public void ExecuteNonQuery(String command)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = command;
            sqlCommand.ExecuteNonQuery();
        }

        public void ExecuteNonQueryWithParameters(String command, Dictionary<String, Object> parameters)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = command;

            foreach (var key in parameters.Keys)
            {
                sqlCommand.Parameters.AddWithValue(key, parameters[key]);
            }
            sqlCommand.ExecuteNonQuery();
        }

        public SqlDataReader ExecuteQuery(String command)
        {
            SqlDataReader sqlDataReader = null;
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = command;
            sqlDataReader = sqlCommand.ExecuteReader();
            return sqlDataReader;
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
