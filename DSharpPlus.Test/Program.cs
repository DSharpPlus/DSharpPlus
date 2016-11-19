using DSharpPlus;
using DSharpPlus.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Test
{
    class Program
    {
        static void Main(string[] args) => new Program().Run(args).GetAwaiter().GetResult();

        public async Task Run(string[] args)
        {
            if (!File.Exists("token.txt"))
            {
                Console.WriteLine("Please create a \"token.txt\" file with your bot's token inside.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            string token = File.ReadAllLines("token.txt")[0];
            DiscordClient client = new DiscordClient(new DiscordConfig()
            {
                Token = token,
                TokenType = TokenType.Bot,
                DiscordBranch = Branch.Canary,
                LogLevel = LogLevel.Unnecessary,
                UseInternalLogHandler = true,
                AutoReconnect = true
            });

            client.UseCommands(new CommandConfig()
            {
                Prefix = '!',
                SelfBot = false
            });
            client.AddCommand("test", async (x) =>
            {
                await x.Message.Parent.SendMessage("boi");
            });
            client.AddCommand("testerino", async (x) =>
            {
                await client.SendMessage(x.Message.ChannelID, "ye works");
                await client.SendMessage(x.Message.ChannelID, $@"```
Servername: {x.Message.Parent.Parent.Name}
Serverowner: {x.Message.Parent.Parent.OwnerID}
```");
            });

            client.Ready += (sender, e) =>
            {
                Console.WriteLine(client.Me.Username);
            };

            client.ChannelCreated += (sender, e) =>
            {
                Console.WriteLine($"Channel {e.Channel.Name} on {e.Guild.Name} created! [Type: {e.Channel.Type.ToString()}]");
            };

            client.GuildAvailable += (sender, e) =>
            {
                Console.WriteLine($"Guild {e.Guild.Name} by {e.Guild.OwnerID} available");

                //Console.WriteLine(e.Guild.Channels.Find(x => x.ID == 206863986690359296).Parent.Name);
            };

            client.MessageCreated += async (sender, e) =>
            {
                if (e.Message.Content == "!!embed")
                {
                    List<DiscordEmbedField> fields = new List<DiscordEmbedField>();
                    fields.Add(new DiscordEmbedField()
                    {
                        Name = "This is a field",
                        Value = "it works :p",
                        Inline = false
                    });
                    fields.Add(new DiscordEmbedField()
                    {
                        Name = "Multiple fields",
                        Value = "cool",
                        Inline = false
                    });
                    DiscordEmbed embed = new DiscordEmbed
                    {
                        Title = "Testing embed",
                        Description = "It works!",
                        Type = "rich",
                        Url = "https://github.com/NaamloosDT/DSharpPlus",
                        Color = 8257469,
                        Fields = fields,
                        Author = new DiscordEmbedAuthor()
                        {
                            Name = "DSharpPlus team",
                            IconUrl = "https://raw.githubusercontent.com/NaamloosDT/DSharpPlus/master/logo_smaller.png",
                            Url = "https://github.com/NaamloosDT/DSharpPlus"
                        },
                        Footer = new DiscordEmbedFooter()
                        {
                            Text = "I am a footer"
                        },
                        Image = new DiscordEmbedImage()
                        {
                            Url = "https://raw.githubusercontent.com/NaamloosDT/DSharpPlus/master/logo_smaller.png",
                            Height = 50,
                            Width = 50,
                        },
                        Thumbnail = new DiscordEmbedThumbnail()
                        {
                            Url = "https://raw.githubusercontent.com/NaamloosDT/DSharpPlus/master/logo_smaller.png",
                            Height = 10,
                            Width = 10
                        }
                    };
                    await e.Message.Respond("testing embed:", false, embed);
                }

                if (e.Message.Content.StartsWith("!!testerino"))
                {
                    await client.SendMessage(e.Message.ChannelID, "ye works");
                    await client.SendMessage(e.Message.ChannelID, $@"```
Servername: {e.Message.Parent.Parent.Name}
Serverowner: {e.Message.Parent.Parent.OwnerID}
```");

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Mentions ({e.MentionedUsers.Count}/{e.MentionedRoles.Count}/{e.MentionedChannels.Count}):\n```Users:");
                    foreach (DiscordMember member in e.MentionedUsers)
                    {
                        sb.AppendLine($"{member.User.Username}");
                    }
                    sb.AppendLine("\nRoles:");
                    foreach (DiscordRole role in e.MentionedRoles)
                    {
                        sb.AppendLine($"{role.Name}");
                    }
                    sb.AppendLine("\nChannels:");
                    foreach (DiscordChannel channel in e.MentionedChannels)
                    {
                        sb.AppendLine($"{channel.Name}");
                    }
                    sb.AppendLine("```");
                    await client.SendMessage(e.Message.ChannelID, sb.ToString());
                }
            };

            await client.Connect();

            while (true)
                Console.ReadLine();
        }
    }
}
