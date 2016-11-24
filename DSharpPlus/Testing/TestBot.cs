using System;
using DSharpPlus.Commands;
using System.IO;
using DSharpPlus.Events;

namespace DSharpPlus.Testing
{
    class TestBot
    {
        static void Main(string[] args)
        {
            string botToken = "no";
            Console.WriteLine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
            DiscordClient client = new DSharpPlus.DiscordClient(botToken, true);

            // voice testing
            DiscordVoiceClient voiceClient;

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
                Console.WriteLine(e.Member.CurrentVoiceChannel.Name);
                try
                {
                    voiceClient = new DiscordVoiceClient(client, config, e.Member.CurrentVoiceChannel);
                } catch
                {
                    e.Channel.SendMessage("Failed");
                }
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
