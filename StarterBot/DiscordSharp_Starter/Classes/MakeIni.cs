using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCord_Starter
{
   public static class MakeIni
    {
        public static void Create()
        {
            //Checks to see if Bot_Settings.ini exists and if it doesnt it generates a default file for you to config and then ends the bot.
            if (!File.Exists("Bot_Settings.ini"))
            {
                IniFile MyIni = new IniFile("Bot_Settings.ini");
                MyIni.Write("Bot Token", "N/A");
                MyIni.Write("Bot Name", "Default-chan");
                MyIni.Write("Game its playing", "SharpCord <3");
                MyIni.Write("Prefix", "!");

                Console.WriteLine("You did not have a ini file set up. \nA defualt one has been made with the name \"Bot_Settings.ini\" \nPlease go ahead and configure it and restart the bot.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}
