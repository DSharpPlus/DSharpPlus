using DiscordSharp;
using DiscordSharp.Commands;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot.Modules
{
    class TestServerLog : IModule
    {
        private DiscordServer DiscordSharpTestServer;
        private DiscordChannel LogChannel;

        public TestServerLog(DiscordClient client)
        {
            Name = "discordsharp-logs";
            Description = "nunya";

            DiscordSharpTestServer = client.GetServersList().Find(
                x => x.Name == "DiscordSharp Test Server"
            ); //todo replace with id
            if(DiscordSharpTestServer != null)
            {
                LogChannel = DiscordSharpTestServer.Channels.Find(x => x.Name == "log" && x.Type == ChannelType.Text);
            }
        }

        public override void Install(CommandsManager manager)
        {
            /**
            cool hack
            */
            manager.Client.GuildUpdated += (sender, e) =>
            {
                Console.WriteLine($"[TestServerLog Module] Guild update. Old guild name: {e.OldServer.Name}");
                if(DiscordSharpTestServer != null)
                {
                    if (e.NewServer.ID == DiscordSharpTestServer.ID) //test server
                    {
                        Console.WriteLine($"[TestServerLog Module] Posting comparison.");
                        string msg = $"**Server Update**\n";
                        msg += $"\n**Name: **: {e.OldServer.Name} -> {e.NewServer.Name}";
                        msg += $"\n**Icon:** <{e.OldServer.IconURL}> -> <{e.OldServer.IconURL}>";
                        msg += $"\n**ID:** {e.NewServer.ID}";
                        msg += $"\n**Owner: ** {e.OldServer.Owner.ID} -> {e.NewServer.Owner.ID}";
                        msg += $"\n**Region: ** {e.OldServer.Region} -> {e.NewServer.Region}";
                        msg += $"\n**Users Online: **";
                        foreach (var user in DiscordSharpTestServer.Members)
                        {
                            if (user.Value != null && user.Value.Status == Status.Online)
                                msg += $"{user.Value.Username}, ";
                        }
                        msg += "\n------------------------------";
                        LogChannel.SendMessage(msg);

                        DiscordSharpTestServer = e.NewServer;
                    }
                }
            };
            manager.Client.ChannelUpdated += (sender, e) =>
            {
                if (LogChannel != null && DiscordSharpTestServer != null)
                {
                    if (e.NewChannel.Parent.ID == DiscordSharpTestServer.ID)
                    {
                        string msg = $"**Channel Update**\n";
                        msg += $"\n**Name: ** {e.OldChannel.Name} -> {e.NewChannel.Name}";
                        msg += $"\n**Topic:** {e.OldChannel.Topic} -> {e.NewChannel.Topic}";
                        msg += $"\n**ID:** {e.NewChannel.ID}";
                        msg += $"\n**Users Online: **";
                        foreach (var user in DiscordSharpTestServer.Members)
                        {
                            if (user.Value != null && user.Value.Status == Status.Online)
                                msg += $"{user.Value.Username}, ";
                        }
                        msg += "\n------------------------------";
                        LogChannel.SendMessage(msg);
                    }
                }
            };
        }
    }
}
