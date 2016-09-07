using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Utility;
using DSharpPlus.Commands;
using System.IO;

namespace DSharpPlus.Testing
{
    class TestBot
    {
        static void Main(string[] args)
        {
            string botToken = FileIO.LoadString(
                Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName
                + "\\bot_token.txt");
            Console.WriteLine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
            DiscordClient client = new DSharpPlus.DiscordClient(botToken, true);

            Console.WriteLine("Connecting...");
            client.Connected += (sender, e) =>
            {
                Console.WriteLine("Connected.");
                client.UpdateCurrentGame("Testing", true);
            };
            client.Connect();

            client.CommandPrefix = ".";

            client.AddCommand(DiscordCommand.Create("test").Do(async e =>
            {
                await e.Channel.SendMessageAsync($"Test received. param1={e.GetArg(0)}  param2={e.GetArg(1)}  multiWordParam3={e.GetArg(2)}");
            })
            .AddParameter("param1")
            .AddParameter("param2")
            .AddParameter("param3", DiscordCommandParameterType.Multiple)
            .AddAlias("alsotest")
            );

            client.MessageReceived += (a, b) =>
            {
                if(b.Author.ID == "183777969221795841" && b.Channel.ID == "221338268682158081")
                    b.Channel.DeleteMessage(b.Message);
            };
            Console.ReadLine();

        }
    }
}
