using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAViewer.DB
{
    public class Database
    {

        public string _ConnectionString { get; private set; }
        public string _UserName { get; private set; }
        public string _Password { get; private set; }

        public Database(String connectionString, String userName, String password)
        {
            this._ConnectionString = connectionString;
            this._UserName = userName;
            this._Password = password;
        }

        public Database() { }

        public enum DatabaseTypes
        {
            SQL_DATABASE = 1,
            ORACLE_DATABASE = 2
        }
    }
}
