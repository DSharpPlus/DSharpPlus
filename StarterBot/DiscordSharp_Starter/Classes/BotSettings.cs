using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCord_Starter
{
    public static class BotSettings
    {
        static IniFile MyIni = new IniFile("Bot_Settings.ini");

        static string BotToken  = MyIni.Read("Bot Token");
        static string BotName   = MyIni.Read("Bot Name");
        static string BotStatus = MyIni.Read("Game its playing");
        static string BotPrefix = MyIni.Read("Prefix");


        static public string botToken
        {
            get
            {
                return BotToken;
            }
        }
        static public string botName
        {
            get
            {
                return BotName;
            }
        }
        static public string botStatus
        {
            get
            {
                return BotStatus;
            }
        }
        static public string botPrefix
        {
            get
            {
                return BotPrefix;
            }
        }

    }
}
