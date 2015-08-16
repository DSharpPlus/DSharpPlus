using DiscordSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscordGUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static DiscordClient MainDiscordClient = new DiscordClient();
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoginForm login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK)
                Application.Run(new View());
            else
                Application.Exit();
            
        }
    }
}
