using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// 

        
        static mainForm form;
        static Server server;
        


        [STAThread]
        static void Main()
        {
            

            
            




            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new mainForm();
            
         //   Thread serverThread = new Thread(new ThreadStart(Server.StartServer));
            Application.Run(form);


            
        }
    }
    
}
