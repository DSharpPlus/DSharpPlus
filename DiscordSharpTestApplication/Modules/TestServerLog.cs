using DiscordSharp;
using DiscordSharp.Commands;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharpTestApplication.Modules
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
                x => x.name == "DiscordSharp Test Server"
            ); //todo replace with id
            if(DiscordSharpTestServer != null)
            {
                LogChannel = DiscordSharpTestServer.channels.Find(x => x.Name == "log" && x.Type == ChannelType.Text);
            }
        }

        public override void Install(CommandsManager manager)
        {
            /**
            cool hack
            */
            manager.Client.GuildUpdated += (sender, e) =>
            {
                Console.WriteLine($"[TestServerLog Module] Guild update. Old guild name: {e.OldServer.name}");
                if(DiscordSharpTestServer != null)
                {
                    if (e.NewServer.id == DiscordSharpTestServer.id) //test server
                    {
                        Console.WriteLine($"[TestServerLog Module] Posting comparison.");
                        string msg = $"**Server Update**\n";
                        msg += $"\n**Name: **: {e.OldServer.name} -> {e.NewServer.name}";
                        msg += $"\n**Icon:** <{e.OldServer.IconURL}> -> <{e.OldServer.IconURL}>";
                        msg += $"\n**ID:** {e.NewServer.id}";
                        msg += $"\n**Owner: ** {e.OldServer.owner.ID} -> {e.NewServer.owner.ID}";
                        msg += $"\n**Region: ** {e.OldServer.region} -> {e.NewServer.region}";
                        msg += $"\n**Users Online: **";
                        foreach (var user in DiscordSharpTestServer.members)
                        {
                            if (user != null && user.Status == Status.Online)
                                msg += $"{user.Username}, ";
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
                    if (e.NewChannel.parent.id == DiscordSharpTestServer.id)
                    {
                        string msg = $"**Channel Update**\n";
                        msg += $"\n**Name: ** {e.OldChannel.Name} -> {e.NewChannel.Name}";
                        msg += $"\n**Topic:** {e.OldChannel.Topic} -> {e.NewChannel.Topic}";
                        msg += $"\n**ID:** {e.NewChannel.ID}";
                        msg += $"\n**Users Online: **";
                        foreach (var user in DiscordSharpTestServer.members)
                        {
                            if (user != null && user.Status == Status.Online)
                                msg += $"{user.Username}, ";
                        }
                        msg += "\n------------------------------";
                        LogChannel.SendMessage(msg);
                    }
                }
            };
        }
    }
}
