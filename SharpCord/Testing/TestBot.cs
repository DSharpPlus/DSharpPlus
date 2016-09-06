using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCord;
using SharpCord.Utility;
using SharpCord.Commands;
using System.IO;

namespace SharpCord.Testing
{
    class TestBot
    {
        public static bool isBot = true;

        static void Main(string[] args)
        {
            string botToken = FileIO.LoadString(
                Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName
                + "bot_token.txt");
            DiscordClient client = new SharpCord.DiscordClient(botToken);
        }
    }
}
