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
            Console.WriteLine("Program initialized!");
            Controller.GetInstance().RunWebCrawler("http://www.apple.com", null);
            Console.WriteLine("Program Complete! Hit Enter!");
            Console.ReadLine();
        }
    }
}
