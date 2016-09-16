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
            string botToken = "MTk3MDcyNjMwMjY2ODU1NDI1.Cr3eLw.x260SZUx1voLgcq8vKq-lM1aLp0";
            Console.WriteLine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
            DiscordClient client = new DSharpPlus.DiscordClient(botToken, true);

            // voice testing
            DiscordVoiceClient voiceClient = new DiscordVoiceClient(client);

            Console.WriteLine("Connecting...");
            client.Connected += (sender, e) =>
            {
                Console.WriteLine("Connected.");
                client.UpdateCurrentGame("Testing", true);
            };
            client.Connect();

            client.CommandPrefixes.Add(".");

            client.AddCommand(DiscordCommand.Create("summon").Do(async e =>
            {
                await e.Channel.SendMessageAsync("Going to channel " + e.Member.CurrentVoiceChannel.Name);
                DiscordVoiceConfig config = new DiscordVoiceConfig();
                voiceClient = new DiscordVoiceClient(client, config, e.Member.CurrentVoiceChannel);
            }));

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
