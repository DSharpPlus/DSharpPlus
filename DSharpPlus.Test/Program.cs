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
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
                AutoReconnect = true,
                VoiceSettings = VoiceSettings.None
            });

            client.UseCommands(new CommandConfig()
            {
                Prefix = "!",
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
            client.AddCommand("kill", async (x) =>
            {
                await x.Message.Respond($"Cya o/");

                client.Dispose();
                await Task.Delay(-1);
            });

            client.AddCommand("clearChannel", async (x) =>
            {
                List<ulong> ids = (await x.Message.Parent.GetMessages(before: x.Message.ID, limit: 50)).Select(y => y.ID).ToList();
                await x.Message.Parent.BulkDeleteMessages(ids);
                await x.Message.Respond($"Removed ``{ids.Count}`` messages");
            });

            client.Ready += (sender, e) =>
            {
                Console.WriteLine($"Connected as {client.Me.Username}#{client.Me.Discriminator}\nOn {client.GatewayUrl} (v{client.GatewayVersion})");
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

            client.MessageReactionAdd += async (sender, e) =>
            {
                await e.Message.DeleteAllReactions();
            };

            client.MessageReactionRemoveAll += (sender, e) =>
            {
                client.DebugLogger.LogMessage(LogLevel.Debug, "Client", "All reactions got removed for message id: " + e.MessageID + " in channel: " + e.ChannelID, DateTime.Now);
            };

            client.UserSpeaking += async (sender, e) =>
            {
                await Task.Run(() =>
                {
                    Console.WriteLine($"{e.UserID} [Speaking: {e.Speaking} | SSRC: {e.ssrc}]");
                });
            };

            await client.Connect();

            while (true)
                Console.ReadLine();
        }
    }
}