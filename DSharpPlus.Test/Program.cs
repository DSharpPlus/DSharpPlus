using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Voice;
using NAudio.Wave;
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

        public Task Run(string[] args)
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
                VoiceSettings = VoiceSettings.Both
            });

            client.UseCommands(new CommandConfig()
            {
                Prefix = "-",
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

            client.AddCommand("pres", async (x) =>
            {
                await x.Message.Respond(x.Message.Author.Presence.User.Username);
            });

            client.AddCommand("pers", async (x) =>
            {
                string roles = "roles: ";
                string os = "overwrites: ";

                foreach (DiscordRole r in x.Message.Parent.Parent.Roles)
                {
                    roles += $"\n{r.Name.Replace("@", "[@]")} : {r.ID}";
                }

                foreach (DiscordOverwrite o in client.Guilds[x.Message.Parent.GuildID].Channels.Find(a => a.ID == x.Message.ChannelID).PermissionOverwrites)
                {
                    os += $"\n{o.Type} : {o.ID}";
                }

                await x.Message.Respond(roles + "\n\n" + os);
            });

            client.AddCommand("permission", async (x) =>
            {
                DiscordOverwrite o = x.Message.Parent.PermissionOverwrites.Find(a => a.ID == 281521779116343298);
                foreach(Permission p in Enum.GetValues(typeof(Permission)))
                {
                    o.UndenyPermission(p);
                    o.UnallowPermission(p);
                }
                await x.Message.Parent.UpdateOverwrite(o);
            });

            client.AddCommand("testvoice", async (x) =>
            {
                DiscordChannel vc = await client.GetChannel(148524575586648064);
                await vc.ConnectToVoice();
                await Task.Delay(2000);
                var channelCount = 2; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                using (var MP3Reader = new Mp3FileReader("song.mp3")) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    uint blockSize = (uint)OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while ((byteCount = resampler.Read(buffer, 0, (int)blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
#pragma warning disable CS0618
                        await client.VoiceClient.SendAsync(buffer, blockSize);
#pragma warning restore
                    }
                }

            });

            client.Ready += (sender, e) =>
            {
                Console.WriteLine($"Connected as {client.Me.Username}#{client.Me.Discriminator}\nOn {client.GatewayUrl} (v{client.GatewayVersion})");
            };

            client.MessageCreated += async (sender, e) =>
                    {
                        if (e.Message.Content == "!!appinfo")
                        {
                            DiscordApplication App = await client.GetCurrentApp();

                            string appinfo = "**App info:**\n```";
                            appinfo += "\nApp Name: " + App.Name;
                            appinfo += "\nApp Description: " + App.Description;
                            appinfo += "\nApp ID: " + App.ID;
                            appinfo += "\nApp Creation Date: " + App.CreationDate.ToString();
                            appinfo += "\nApp Owner ID: " + App.Owner.ID;
                            appinfo += "\nApp Owner Username: " + App.Owner.Username + "#" + App.Owner.Discriminator;
                            appinfo += "\nApp Owner Join Date: " + App.Owner.CreationDate.ToString();
                            appinfo += "\n```";

                            await e.Message.Respond(appinfo);
                        }

                        if(e.Message.Content == "!!heck")
                        {
                            DiscordGuild g = await client.GetGuild(0);
                        }

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

                        if (e.Message.Content.StartsWith("!!mention"))
                        {
                            await e.Channel.SendMessage($"{e.Message.Author.Mention} and {e.Channel.Mention}");
                        }

                        if (e.Message.Content.StartsWith("!!restart"))
                        {
                            await client.Reconnect();
                        }

                        if (e.Message.Content == "!!mypresence")
                        {
                            DiscordPresence p = client.GetUserPresence(e.Message.Author.ID);

                            string info = "Your presence: ";
                            info += "\nYour ID: " + p.UserID;
                            info += "\nGame: " + p.Game;
                            info += "\nStatus: " + p.Status;

                            await e.Message.Respond(info);
                        }
                        if (e.Message.Content == "!!exception")
                        {
                            try
                            {
                                await e.Message.Respond("");
                            }
                            catch (BadRequestException ex)
                            {
                                Console.WriteLine(ex.WebResponse.ResponseCode + ": " + ex.JsonMessage);
                            }
                        }
                        if (e.Message.Content == "!!heck")
                        {
                    /*List<DiscordMember> mems = await client.InternalListGuildMembers(e.Guild.ID, 1000, 0);
                    string memes = "";
                    foreach(DiscordMember m in mems)
                    {
                        memes += $"{m.User.Username}#{m.User.Discriminator}: (nick: {m.Nickname})\n";
                    }
                    await e.Message.Respond(memes);*/
                        }
                        if (e.Message.Content == "!!testmodify")
                        {
                            await client.ModifyMember(e.Guild.ID, e.Message.Author.ID, "yoyoyoy");
                        }
                    };

            // This was an example I made for someone, but might be nice to keep this in for people who sniff out test code instead of docs :^)
            client.GuildBanAdd += async (sender, e) =>
            {
                List<DiscordChannel> c = e.Guild.Channels.FindAll(x => x.Name.Contains("logs"));
                if (c.Count > 0)
                    await c[0].SendMessage($"User Banned: {e.User.Username}#{e.User.Discriminator}");
            };

            client.GuildAvailable += (sender, e) =>
            {
                Console.WriteLine(e.Guild.Name);
            };

            client.MessageReactionAdd += async (sender, e) =>
            {
                await e.Message.DeleteAllReactions();
            };

            client.MessageReactionRemoveAll += (sender, e) =>
            {
                client.DebugLogger.LogMessage(LogLevel.Debug, "Client", $"All reactions got removed for message id: {e.MessageID} in channel: {e.ChannelID}", DateTime.Now);
            };

            client.PresenceUpdate += (sender, e) =>
            {
                if (e.User != null)
                    Console.WriteLine($"[{e.UserID}]{e.User.Username}#{e.User.Discriminator}: {e.Status} playing " + e.Game);
            };

            client.Connect().GetAwaiter().GetResult();

            while (true)
                Console.ReadLine();
        }
    }
}
