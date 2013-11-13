using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
