using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Utility;
using DSharpPlus.Commands;
using System.IO;
using DSharpPlus.Events;

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

            client.CommandPrefixes.Add(".");

            client.AddCommand(new Command_Greet());

            client.AddCommand(DiscordCommand.Create("test").Do(async e =>
            {
                await e.Channel.SendMessageAsync($"Test received. param1={e.GetArg(0)}  param2={e.GetArg(1)}  multiWordParam3={e.GetArg(2)}");
            })
            .AddParameter("param1")
            .AddParameter("param2")
            .AddParameter("param3", DiscordCommandParameterType.Multiple)
            .Alias("alsotest")
            );

            //Mayuri
            client.AddAuthorListener("183777969221795841", new EventHandler<DiscordMessageEventArgs>((sender, e) =>
            {
                if (e.Channel.ID == "221338268682158081")
                    e.Channel.DeleteMessage(e.Message);
            }));

            //Death
            client.AddAuthorListener("92838401044140032", new EventHandler<DiscordMessageEventArgs>((sender, e) =>
            {
                if (e.Channel.ID == "221338268682158081")
                    e.Channel.DeleteMessage(e.Message);
            }));

            //Also Death
            client.AddAuthorListener("92838401044140032", new EventHandler<DiscordMessageEventArgs>((sender, e) =>
            {
                e.Channel.SendMessage("Hey Death");
            }));

            Console.ReadLine();

        }
    }
}
