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
            TruncateDatabase();
            String rootURL = "http://www.apple.com";
            String[] uriContents = {"apple.com"};
            Controller.GetInstance().RunWebCrawler(rootURL, uriContents);
            Console.WriteLine("Program Complete! Hit Enter!");
            Console.Read();
        }

        public static void TruncateDatabase()
        {
            DBConfiguration dbConfig = DBConfiguration.GetDBConfiguration();
            dbConfig = DBConfigurationSectionHandler.LoadFromXml().Convert();
            IDatabase database = DBFactory.GetDatabase(dbConfig.DatabaseType);
            String command = "DELETE FROM dbo.CrawledPage; DELETE FROM dbo.PageContent; DELETE FROM dbo.Project";
            database.ExecuteNonQuery(command);
            database.CloseConnection();
        }
    }
}
