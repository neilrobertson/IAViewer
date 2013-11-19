using System;
using System.Data.SqlClient;
using System.Text;
using System.Collections.Generic;

namespace IAViewer.DB
{
    public interface IDatabase
    {
        void CloseConnection();
        Func<IDatabase> GetDatabaseGenerator();
        string _ConnectionString { get; }
        string _UserName { get; }
        string _Password { get; }
        void ExecuteNonQuery(String command);
        void ExecuteNonQueryWithParameters(String command, Dictionary<String, Object> parameters);
        SqlDataReader ExecuteQuery(String command);
    }
}
