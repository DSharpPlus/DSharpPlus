using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using DSharpPlus.Commands;
//using DSharpPlus.VoiceNext;
using NAudio.Wave;
using DSharpPlus.Interactivity;

namespace DSharpPlus.Test
{
    public sealed class TestBotCommands
    {
        public async Task Test(CommandEventArgs e) =>
            await e.Channel.SendMessage("u w0t m8");

        public async Task Testerino(CommandEventArgs e)
        {
            await e.Discord.SendMessage(e.Channel, "ill bash ur head in i sweak on me fkin mum");
            await e.Discord.SendMessage(e.Message.ChannelID, $@"```
Servername: {e.Guild.Name}
Serverowner: {e.Guild.OwnerID}
```");
        }

        public async Task Cunt(CommandEventArgs e)
        {
            await e.Message.Respond("u wot");
            DiscordMessage m = await TestBot.Discord.GetInteractivityModule().WaitForMessageAsync(xm => xm.Content.ToLower() == "no u", TimeSpan.FromSeconds(30));
            if (m == null)
                await e.Message.Respond("that's what i thought u lil basterd");
            else
                await e.Message.Respond("What the fuck did you just fucking say about me, you little bitch? I’ll have you know I graduated top of my class in the Navy Seals, and I’ve been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and I’m the top sniper in the entire US armed forces. You are nothing to me but just another target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your life. You’re fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that’s just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little “clever” comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldn’t, you didn’t, and now you’re paying the price, you goddamn idiot. I will shit fury all over you and you will drown in it. You’re fucking dead, kiddo.");
        }

        public async Task Poll(CommandEventArgs e)
        {
            var m = await e.Message.Respond("Hey everyone! Add some reactions to this message! you've got 30 seconds!");
            await e.Message.Delete();
            var list = await TestBot.Discord.GetInteractivityModule().CollectReactionsAsync(m, TimeSpan.FromSeconds(30));
            string reactions = "We're done people!\n\nReactions:";
            foreach(var collected in list)
            {
                reactions += "\n" + collected.Key + ": " + collected.Value + "times!";
            }
            await m.Respond(reactions);
        }

        public async Task Kill(CommandEventArgs e)
        {
            await e.Channel.SendMessage("kthxbai 👋");
            e.Discord.Dispose();
            await Task.Delay(-1);
        }

        public async Task Restart(CommandEventArgs e) =>
            await e.Discord.Reconnect();

        public async Task PurgeChannel(CommandEventArgs e)
        {
            var ids = (await e.Channel.GetMessages(before: e.Message.ID, limit: 50))
                .Select(y => y.ID)
                .ToList();
            await e.Channel.BulkDeleteMessages(ids);
            await e.Message.Respond($"Removed `{ids.Count}` messages");
        }

        public async Task Presence(CommandEventArgs e) =>
            await e.Message.Respond(e.Author.Username);

        public async Task Guild(CommandEventArgs e)
        {
            var roles = e.Guild.Roles.Select(xr => xr.Mention);
            var overs = e.Channel.PermissionOverwrites.Select(xo => string.Concat("Principal: ", xo.ID, " (", xo.Type, "), Allow: ", (ulong)xo.Allow, "; Deny: ", (ulong)xo.Deny));

            var embed = new DiscordEmbed
            {
                Title = "Guild info",
                Description = "ye boiii!",
                Type = "rich",
                Color = 0x007FFF,
                Fields = new List<DiscordEmbedField>
                {
                    new DiscordEmbedField
                    {
                        Inline = false,
                        Name = "Roles",
                        Value = string.Join("\n", roles)
                    },
                    new DiscordEmbedField
                    {
                        Inline = false,
                        Name = string.Concat("Overrides for ", e.Channel.Mention),
                        Value = string.Join("\n", overs)
                    }
                }
            };

            await e.Message.Respond("", embed: embed);
        }

        public async Task Embed(CommandEventArgs e)
        {
            List<DiscordEmbedField> fields = new List<DiscordEmbedField>
            {
                new DiscordEmbedField()
                {
                    Name = "This is a field",
                    Value = "it works :p",
                    Inline = false
                },
                new DiscordEmbedField()
                {
                    Name = "Multiple fields",
                    Value = "cool",
                    Inline = false
                }
            };

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
            await e.Message.Respond("testing embed:", embed: embed);
        }

        public async Task AppInfo(CommandEventArgs e)
        {
            var app = await e.Discord.GetCurrentApp();
            var usrn = app.Owner.Username
                .Replace(@"\", @"\\")
                .Replace(@"*", @"\*")
                .Replace(@"_", @"\_")
                .Replace(@"~", @"\~")
                .Replace(@"`", @"\`");

            var embed = new DiscordEmbed
            {
                Title = "Application info",
                Color = 0x007FFF,
                Fields = new List<DiscordEmbedField>()
            };
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Name", Value = app.Name });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Description", Value = string.Concat("```\n", app.Description, "\n```") });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "ID", Value = app.ID.ToString() });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Created", Value = app.CreationDate.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Owner", Value = $"{usrn}#{app.Owner.Discriminator.ToString("0000")} ({app.Owner.ID})" });

            await e.Message.Respond("", embed: embed);
        }

        public async Task Mention(CommandEventArgs e) =>
            await e.Message.Respond($"User {e.Author.Mention} invoked this from {e.Channel.Mention}");

        public async Task ModifyMe(CommandEventArgs e) =>
            await e.Discord.ModifyMember(e.Guild.ID, e.Author.ID, "Tests D#+ instead of going outside");

        public async Task Hang(CommandEventArgs e)
        {
            await e.Message.Respond("Will now hang this thread for 10 minutes.");
            await Task.Delay(TimeSpan.FromMinutes(10));
            await e.Message.Respond("Thread hang completed.");
        }

        /*public async Task VoiceJoin(CommandEventArgs e)
        {
            var vs = e.Guild.VoiceStates.FirstOrDefault(xvs => xvs.UserID == e.Author.ID);
            if (vs == null)
            {
                await e.Message.Respond("You are not in a voice channel");
                return;
            }

            var chn = e.Guild.Channels.FirstOrDefault(xc => xc.ID == vs.ChannelID);
            if (chn == null)
            {
                await e.Message.Respond("Your voice channel was not found");
                return;
            }

            var voice = e.Discord.GetVoiceNextClient();
            if (voice == null)
            {
                await e.Message.Respond("Voice is not activated");
                return;
            }

            await Task.Yield();
            await voice.ConnectAsync(chn);
            await e.Message.Respond($"Tryina join `{chn.Name}` ({chn.ID})");
        }

        public async Task VoiceLeave(CommandEventArgs e)
        {
            var voice = e.Discord.GetVoiceNextClient();
            if (voice == null)
            {
                await e.Message.Respond("Voice is not activated");
                return;
            }

            var vnc = voice.GetConnection(e.Guild);
            if (vnc == null)
            {
                await e.Message.Respond("Voice is not connected in this guild");
                return;
            }

            vnc.Disconnect();
            await e.Message.Respond("Disconnected");
        }

        public async Task VoicePlay(CommandEventArgs e)
        {
            var voice = e.Discord.GetVoiceNextClient();
            if (voice == null)
            {
                await e.Message.Respond("Voice is not activated");
                return;
            }

            var vnc = voice.GetConnection(e.Guild);
            if (vnc == null)
            {
                await e.Message.Respond("Voice is not connected in this guild");
                return;
            }

            var snd = string.Join(" ", e.Arguments);
            if (string.IsNullOrWhiteSpace(snd) || !File.Exists(snd))
            {
                await e.Message.Respond("Invalid file specified");
                return;
            }

            var exc = (Exception)null;
            await e.Message.Respond($"Playing `{snd}`");
            await vnc.SendSpeakingAsync(true);
            try
            {
                var fmt = new WaveFormat(48000, 16, 2);
                using (var wav = new AudioFileReader(snd))
                using (var pcm = new MediaFoundationResampler(wav, fmt))
                {
                    pcm.ResamplerQuality = 60;
                    var bs = fmt.AverageBytesPerSecond / 50;
                    var buff = new byte[bs];
                    int bc = 0;
                    
                    while ((bc = pcm.Read(buff, 0, bs)) > 0)
                    {
                        if (bc < bs)
                            for (var i = 0; i < bs; i++)
                                buff[i] = 0;

                        await vnc.SendAsync(buff, 20, fmt.BitsPerSample);
                    }
                }
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
            }

            if (exc != null)
                throw exc;
        }

        public async Task VoiceSpeak(CommandEventArgs e)
        {
            var voice = e.Discord.GetVoiceNextClient();
            if (voice == null)
            {
                await e.Message.Respond("Voice is not activated");
                return;
            }

            var vnc = voice.GetConnection(e.Guild);
            if (vnc == null)
            {
                await e.Message.Respond("Voice is not connected in this guild");
                return;
            }

            var spk = string.Join(" ", e.Arguments);
            if (string.IsNullOrWhiteSpace(spk))
            {
                await e.Message.Respond("Must say something");
                return;
            }

            await e.Message.Respond($"Speaking");
            await vnc.SendSpeakingAsync(true);
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var sps = new SpeechSynthesizer())
                    {
                        sps.SetOutputToWaveStream(ms);
                        sps.Speak(spk);
                    }

                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);

                    var fmt = new WaveFormat(48000, 16, 2);
                    using (var wav = new WaveFileReader(ms))
                    using (var pcm = new MediaFoundationResampler(wav, fmt))
                    {
                        pcm.ResamplerQuality = 60;
                        var bs = fmt.AverageBytesPerSecond / 50;
                        var buff = new byte[bs];
                        int bc = 0;

                        while ((bc = pcm.Read(buff, 0, bs)) > 0)
                        {
                            if (bc < bs)
                                for (var i = 0; i < bs; i++)
                                    buff[i] = 0;

                            await vnc.SendAsync(buff, 20, fmt.BitsPerSample);
                        }
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                await vnc.SendSpeakingAsync(false);
            }
        }*/
    }
}
