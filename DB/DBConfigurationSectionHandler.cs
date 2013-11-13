using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace IAViewer.DB
{
    public class DBConfigurationSectionHandler : ConfigurationSection
    {
        public DBConfigurationSectionHandler()
        {

        }

        [ConfigurationProperty("databaseBehavior")]
        public DatabaseBehaviorElement DatabaseBehavior
        {
            get { return (DatabaseBehaviorElement)this["databaseBehavior"]; }
        }

        public DBConfiguration Convert()
        {
            AutoMapper.Mapper.CreateMap<DatabaseBehaviorElement, DBConfiguration>();

            DBConfiguration config = DBConfiguration.GetDBConfiguration();
            AutoMapper.Mapper.Map<DatabaseBehaviorElement, DBConfiguration>(DatabaseBehavior, config);

            return config;
        }

        public static DBConfigurationSectionHandler LoadFromXml()
        {
            return ((DBConfigurationSectionHandler)System.Configuration.ConfigurationManager.GetSection("DB"));
        }
    }

    public class DatabaseBehaviorElement : ConfigurationElement
    {
        [ConfigurationProperty("databaseType", IsRequired = true)]
        public string DatabaseType
        {
            get { return (string)this["databaseType"]; }
        }

        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString
        {
            get { return (string)this["connectionString"]; }
        }

        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName
        {
            get { return (string)this["userName"]; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return (string)this["password"]; }
        }
    }
}
