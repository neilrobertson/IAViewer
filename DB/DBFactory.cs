using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAViewer.DB
{
    class DBFactory
    {
        public static Dictionary<String, IDatabase> storage = new Dictionary<String, IDatabase>();
        public static HashSet<String> checkExists = new HashSet<String>();
        public static Boolean isSetUp = false;

        public static IDatabase GetDatabase(String databaseType)
        {
            if (isSetUp == false)
            {
                SetUp();
                isSetUp = true;
            }

            IDatabase database = (IDatabase)storage[databaseType];
            if (database == null)
                return SQL_Database._database as IDatabase;
            return database;
        }

        public static void Register(String type, IDatabase database)
        {
            if (checkExists.Contains(type) == false)
            {
                checkExists.Add(type);
                storage.Add(type, database);
            }
        }

        private static void SetUp()
        {
            SQL_Database.Register();
            Oracle_Database.Register();
        }
    }
}
