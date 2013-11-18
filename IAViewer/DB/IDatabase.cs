using System;
using System.Data.SqlClient;
using System.Text;

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
        SqlDataReader ExecuteQuery(String command);
    }
}
