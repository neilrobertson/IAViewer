using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAViewer.DB
{
    public class DBConfiguration
    {
        public static DBConfiguration uniqueInstance;
        public static object syncLocker = new object();

        private DBConfiguration()
        {
            DatabaseType = "";
            ConnectionString = "";
            UserName = "";
            Password = "";
        }

        public static DBConfiguration GetDBConfiguration()
        {
            lock (syncLocker)
            {
                if (uniqueInstance == null)
                    uniqueInstance = new DBConfiguration();
                return uniqueInstance;
            }
        }

        public string DatabaseType { get; set; }
               
        public string ConnectionString { get; set; }
               
        public string UserName { get; set; }
               
        public string Password { get; set; }

    }
}
