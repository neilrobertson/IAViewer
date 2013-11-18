using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IAViewer.DB;
using System.Threading.Tasks;

namespace IAViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //TruncateDatabase();
            Controller.GetInstance().RunWebCrawler("http://www.apple.com", null);
            Console.WriteLine("Program Complete! Hit Enter!");
            Console.ReadLine();
        }

        public static void TruncateDatabase()
        {
            DBConfiguration dbConfig = DBConfiguration.GetDBConfiguration();
            dbConfig = DBConfigurationSectionHandler.LoadFromXml().Convert();
            IDatabase database = DBFactory.GetDatabase(dbConfig.DatabaseType);
            String command = "TRUNCATE TABLE CrawledPage; TRUNCATE TABLE PageContent; TRUNCATE TABLE Project";
            database.ExecuteNonQuery(command);
            database.CloseConnection();
        }
    }
}
