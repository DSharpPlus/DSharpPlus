using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;

namespace DSharpPlus.Test
{
    public sealed class TestBotCommands
    {
        /*[Command("testreason")]
        public async Task TestReason(CommandContext e, DiscordMember m)
        {
            e.Client.WithAuditReason("testing");
            string oldnick = m.Nickname;
            await m.ModifyAsync("test");
            await Task.Delay(1000);
            await m.ModifyAsync(oldnick);
        }*/

        [Command("namecolor")]
        public async Task NameColor(CommandContext e, DiscordMember m)
        {
            DiscordEmbed embed = new DiscordEmbed()
            {
                Color = m.Color,
                Title = "Color on the left m8"
            };
            await e.RespondAsync("", embed: embed);
        }

        [Command("uploadembed")]
        public async Task UplEm(CommandContext e)
        {
            await e.Channel.SendFileAsync(new FileStream("file.png", FileMode.Open), "file.png", "test upload file to embed", false, new DiscordEmbed()
            {
                Title = "lil test",
                Image = new DiscordEmbedImage()
                {
                    Url = "attachment://file.png"
                },
                Timestamp = DateTime.Now
            });
        }

        [Command("test")]
        public async Task Test(CommandContext e) =>
            await e.Channel.SendMessageAsync("u w0t m8");

        [Command("testerino")]
        public async Task Testerino(CommandContext e)
        {
            await e.Client.SendMessageAsync(e.Channel, "ill bash ur head in i sweak on me fkin mum");
            await e.Client.SendMessageAsync(e.Message.Channel, $@"```
Servername: {e.Guild.Name}
Serverowner: {e.Guild.Owner.DisplayName}
```");
        }

        [Command("cunt")]
        public async Task Cunt(CommandContext e)
        {
            await e.Message.RespondAsync("u wot");
            DiscordMessage m = await e.Client.GetInteractivityModule().WaitForMessageAsync(xm => xm.Content.ToLower() == "no u", TimeSpan.FromSeconds(30));
            if (m == null)
                await e.Message.RespondAsync("that's what i thought u lil basterd");
            else
                await e.Message.RespondAsync("What the fuck did you just fucking say about me, you little bitch? I’ll have you know I graduated top of my class in the Navy Seals, and I’ve been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and I’m the top sniper in the entire US armed forces. You are nothing to me but just another target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your life. You’re fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that’s just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little “clever” comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldn’t, you didn’t, and now you’re paying the price, you goddamn idiot. I will shit fury all over you and you will drown in it. You’re fucking dead, kiddo.");
        }

        [Command("pageembed")]
        public async Task Page(CommandContext e)
        {
            List<Page> pages = new List<Page>()
            {
                new Page()
                {
                    Content = "test 0"
                },
                new Page()
                {
                    Content = "test 1"
                },
                new Page()
                {
                    Content = "test 2"
                },
                new Page()
                {
                    Content = "test 3",
                    Embed = new DiscordEmbed()
                    {
                        Title = "yes!",
                        Description = "this has embeds!!"
                    }
                },
                new Page()
                {
                    Content = "test 4"
                }
            };
            await e.Client.GetInteractivityModule().SendPaginatedMessage(e.Channel, e.User,
                e.Client.GetInteractivityModule().GeneratePagesInEmbeds(/*Dont mind this, i've broken it in parts*/"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quae mihi ipsi, qui volo et esse et haberi gratus, grata non essent, nisi eum perspicerem mea causa mihi amicum fuisse, non sua, nisi hoc dicis sua, quod interest omnium recte facere. Omnes, qui non sint sapientes, aeque miseros esse, sapientes omnes summe beatos, recte facta omnia aequalia, omnia peccata paria; Vives, inquit Aristo, magnifice atque praeclare, quod erit cumque visum ages, numquam angere, numquam cupies, numquam timebis. Illud vero minime consectarium, sed in primis hebes, illorum scilicet, non tuum, gloriatione dignam esse beatam vitam, quod non possit sine honestate contingere, ut iure quisquam glorietur. Superiores tres erant, quae esse possent, quarum est una sola defensa, eaque vehementer. Duo Reges: constructio interrete. Ex ea quae sint apta, ea bonesta, ea pulchra, ea laudabilia, illa autem superiora naturale nominantur, quae coniuncta cum honestis vitam beatam perficiunt et absolvunt. Quantum Aristoxeni ingenium consumptum videmus in musicis? Ita, quod certissimum est, pro vera certaque iustitia simulationem nobis iustitiae traditis praecipitisque quodam modo ut nostram stabilem conscientiam contemnamus, aliorum errantem opinionem aucupemur. Etenim nec iustitia nec amicitia esse omnino poterunt, nisi ipsae per se expetuntur. Aufidio, praetorio, erudito homine, oculis capto, saepe audiebam, cum se lucis magis quam utilitatis desiderio moveri diceret. Ex quo intellegitur nec intemperantiam propter se esse fugiendam temperantiamque expetendam, non quia voluptates fugiat, sed quia maiores consequatur. Sin autem est in ea, quod quidam volunt, nihil impedit hanc nostram comprehensionem summi boni. Totius enim quaestionis eius, quae habetur de finibus bonorum et malorum, cum quaeritur, in his quid sít extremum et ultimum, fons reperiendus est, in quo sint prima invitamenta naturae; Quod idem cum vestri faciant, non satis magnam tribuunt inventoribus gratiam. Quaeque de virtutibus dicta sunt, quem ad modum eae semper voluptatibus inhaererent, eadem de amicitia dicenda sunt. Quo modo autem optimum, si bonum praeterea nullum est? Incommoda autem et commoda-ita enim estmata et dustmata appello-communia esse voluerunt, paria noluerunt. Cum autem assumpta ratío est, tanto in dominatu locatur, ut omnia illa prima naturae hulus tutelae subiciantur. Omne enim animal, simul et ortum est, se ipsum et omnes partes suas diligit duasque, quae maximae sunt, in primis amplectitur, animum et corpus, deinde utriusque partes. Pomponius Luciusque Cicero, frater noster cognatione patruelis, amore germanus, constituimus inter nos ut ambulationem postmeridianam conficeremus in Academia, maxime quod is locus ab omni turba id temporis vacuus esset. Atque his tribus generibus honestorum notatis quartum sequitur et in eadem pulchritudine et aptum ex illis tribus, in quo inest ordo et moderatio. Is hoc melior, quam Pyrrho, quod aliquod genus appetendi dedit, deterior quam ceteri, quod penitus a natura recessit. Ergo infelix una molestia, fellx rursus, cum is ipse anulus in praecordiis piscis inventus est? Quae cum ita sint, effectum est nihil esse malum, quod turpe non sit. Sed virtutem ipsam inchoavit, nihil amplius. In homine autem summa omnis animi est et in animo rationis, ex qua virtus est, quae rationis absolutio definitur, quam etiam atque etiam explicandam putant. Alii rursum isdem a principiis omne officium referent aut ad voluptatem aut ad non dolendum aut ad prima illa secundum naturam optinenda. Quodsi esset in voluptate summum bonum, ut dicitis, optabile esset maxima in voluptate nullo intervallo interiecto dies noctesque versari, cum omnes sensus dulcedine omni quasi perfusi moverentur. Quantam rem agas, ut Circeis qui habitet totum hunc mundum suum municipium esse existimet? Quod quam magnum sit fictae veterum fabulae declarant, in quibus tam multis tamque variis ab ultima antiquitate repetitis tria vix amicorum paria reperiuntur, ut ad Orestem pervenias profectus a Theseo. Et tamen vide, ne, si ego non intellegam quid Epicurus loquatur, cum Graece, ut videor, luculenter sciam, sit aliqua culpa eius, qui ita loquatur, ut non intellegatur. Non potes ergo ista tueri, Torquate, mihi crede, si te ipse et tuas cogitationes et studia perspexeris; Non ergo Epicurus ineruditus, sed ii indocti, qui, quae pueros non didicisse turpe est, ea putant usque ad senectutem esse discenda. Nam si dicent ab illis has res esse tractatas, ne ipsos quidem Graecos est cur tam multos legant, quam legendi sunt. Ego autem tibi, Piso, assentior usu hoc venire, ut acrius aliquanto et attentius de claris viris locorum admonitu cogitemus. Constituto autem illo, de quo ante diximus, quod honestum esset, id esse solum bonum, intellegi necesse est pluris id, quod honestum sit, aestimandum esse quam illa media, quae ex eo comparentur. Iudicia rerum in sensibus ponit, quibus si semel aliquid falsi pro vero probatum sit, sublatum esse omne iudicium veri et falsi putat.")
                , TimeSpan.FromMinutes(1), TimeoutBehaviour.Delete);
        }

        [Command("pagestring")]
        public async Task Page2(CommandContext e)
        {
            List<Page> pages = new List<Page>()
            {
                new Page()
                {
                    Content = "test 0"
                },
                new Page()
                {
                    Content = "test 1"
                },
                new Page()
                {
                    Content = "test 2"
                },
                new Page()
                {
                    Content = "test 3",
                    Embed = new DiscordEmbed()
                    {
                        Title = "yes!",
                        Description = "this has embeds!!"
                    }
                },
                new Page()
                {
                    Content = "test 4"
                }
            };
            await e.Client.GetInteractivityModule().SendPaginatedMessage(e.Channel, e.User,
                e.Client.GetInteractivityModule().GeneratePagesInStrings(/*Dont mind this, i've broken it in parts*/"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quae mihi ipsi, qui volo et esse et haberi gratus, grata non essent, nisi eum perspicerem mea causa mihi amicum fuisse, non sua, nisi hoc dicis sua, quod interest omnium recte facere. Omnes, qui non sint sapientes, aeque miseros esse, sapientes omnes summe beatos, recte facta omnia aequalia, omnia peccata paria; Vives, inquit Aristo, magnifice atque praeclare, quod erit cumque visum ages, numquam angere, numquam cupies, numquam timebis. Illud vero minime consectarium, sed in primis hebes, illorum scilicet, non tuum, gloriatione dignam esse beatam vitam, quod non possit sine honestate contingere, ut iure quisquam glorietur. Superiores tres erant, quae esse possent, quarum est una sola defensa, eaque vehementer. Duo Reges: constructio interrete. Ex ea quae sint apta, ea bonesta, ea pulchra, ea laudabilia, illa autem superiora naturale nominantur, quae coniuncta cum honestis vitam beatam perficiunt et absolvunt. Quantum Aristoxeni ingenium consumptum videmus in musicis? Ita, quod certissimum est, pro vera certaque iustitia simulationem nobis iustitiae traditis praecipitisque quodam modo ut nostram stabilem conscientiam contemnamus, aliorum errantem opinionem aucupemur. Etenim nec iustitia nec amicitia esse omnino poterunt, nisi ipsae per se expetuntur. Aufidio, praetorio, erudito homine, oculis capto, saepe audiebam, cum se lucis magis quam utilitatis desiderio moveri diceret. Ex quo intellegitur nec intemperantiam propter se esse fugiendam temperantiamque expetendam, non quia voluptates fugiat, sed quia maiores consequatur. Sin autem est in ea, quod quidam volunt, nihil impedit hanc nostram comprehensionem summi boni. Totius enim quaestionis eius, quae habetur de finibus bonorum et malorum, cum quaeritur, in his quid sít extremum et ultimum, fons reperiendus est, in quo sint prima invitamenta naturae; Quod idem cum vestri faciant, non satis magnam tribuunt inventoribus gratiam. Quaeque de virtutibus dicta sunt, quem ad modum eae semper voluptatibus inhaererent, eadem de amicitia dicenda sunt. Quo modo autem optimum, si bonum praeterea nullum est? Incommoda autem et commoda-ita enim estmata et dustmata appello-communia esse voluerunt, paria noluerunt. Cum autem assumpta ratío est, tanto in dominatu locatur, ut omnia illa prima naturae hulus tutelae subiciantur. Omne enim animal, simul et ortum est, se ipsum et omnes partes suas diligit duasque, quae maximae sunt, in primis amplectitur, animum et corpus, deinde utriusque partes. Pomponius Luciusque Cicero, frater noster cognatione patruelis, amore germanus, constituimus inter nos ut ambulationem postmeridianam conficeremus in Academia, maxime quod is locus ab omni turba id temporis vacuus esset. Atque his tribus generibus honestorum notatis quartum sequitur et in eadem pulchritudine et aptum ex illis tribus, in quo inest ordo et moderatio. Is hoc melior, quam Pyrrho, quod aliquod genus appetendi dedit, deterior quam ceteri, quod penitus a natura recessit. Ergo infelix una molestia, fellx rursus, cum is ipse anulus in praecordiis piscis inventus est? Quae cum ita sint, effectum est nihil esse malum, quod turpe non sit. Sed virtutem ipsam inchoavit, nihil amplius. In homine autem summa omnis animi est et in animo rationis, ex qua virtus est, quae rationis absolutio definitur, quam etiam atque etiam explicandam putant. Alii rursum isdem a principiis omne officium referent aut ad voluptatem aut ad non dolendum aut ad prima illa secundum naturam optinenda. Quodsi esset in voluptate summum bonum, ut dicitis, optabile esset maxima in voluptate nullo intervallo interiecto dies noctesque versari, cum omnes sensus dulcedine omni quasi perfusi moverentur. Quantam rem agas, ut Circeis qui habitet totum hunc mundum suum municipium esse existimet? Quod quam magnum sit fictae veterum fabulae declarant, in quibus tam multis tamque variis ab ultima antiquitate repetitis tria vix amicorum paria reperiuntur, ut ad Orestem pervenias profectus a Theseo. Et tamen vide, ne, si ego non intellegam quid Epicurus loquatur, cum Graece, ut videor, luculenter sciam, sit aliqua culpa eius, qui ita loquatur, ut non intellegatur. Non potes ergo ista tueri, Torquate, mihi crede, si te ipse et tuas cogitationes et studia perspexeris; Non ergo Epicurus ineruditus, sed ii indocti, qui, quae pueros non didicisse turpe est, ea putant usque ad senectutem esse discenda. Nam si dicent ab illis has res esse tractatas, ne ipsos quidem Graecos est cur tam multos legant, quam legendi sunt. Ego autem tibi, Piso, assentior usu hoc venire, ut acrius aliquanto et attentius de claris viris locorum admonitu cogitemus. Constituto autem illo, de quo ante diximus, quod honestum esset, id esse solum bonum, intellegi necesse est pluris id, quod honestum sit, aestimandum esse quam illa media, quae ex eo comparentur. Iudicia rerum in sensibus ponit, quibus si semel aliquid falsi pro vero probatum sit, sublatum esse omne iudicium veri et falsi putat.")
                , TimeSpan.FromMinutes(1), TimeoutBehaviour.Delete);
        }

        [Command("poll")]
        public async Task Poll(CommandContext e)
        {
            var m = await e.Message.RespondAsync("Hey everyone! Add some reactions to this message! you've got 30 seconds!");
            //await e.Message.DeleteAsync();
            var list = await e.Client.GetInteractivityModule().CollectReactionsAsync(m, TimeSpan.FromSeconds(30));
            string reactions = "We're done people!\n\nReactions:";
            foreach (var collected in list)
            {
                reactions += "\n" + collected.Key + ": " + collected.Value + "times!";
            }
            await m.RespondAsync(reactions);
        }

        [Command("kill"), RequireOwner]
        public async Task Kill(CommandContext e)
        {
            await e.Channel.SendMessageAsync("kthxbai 👋");
            e.Client.Dispose();
            await Task.Delay(-1);
        }

        [Command("reconnect"), RequireOwner]
        public async Task Restart(CommandContext e) =>
            await e.Client.ReconnectAsync();

        [Command("purgechannel")]
        public async Task PurgeChannel(CommandContext e)
        {
            var ids = (await e.Channel.GetMessagesAsync(before: e.Message.Id, limit: 50));
            await e.Channel.DeleteMessagesAsync(ids);
            await e.Message.RespondAsync($"Removed `{ids.Count}` messages");
        }

        [Command("presence")]
        public async Task Presence(CommandContext e) =>
            await e.Message.RespondAsync(e.User.Username);

        [Command("multifile")]
        public async Task MultiFile(CommandContext e)
        {
            Dictionary<string, Stream> files = new Dictionary<string, Stream>
            {
                { "file1.png", File.OpenRead("file1.png") },
                { "file2.jpeg", File.OpenRead("file2.jpeg") }
            };

            await e.Message.RespondAsync("multiple images?", files);
        }

        [Command("guild")]
        public async Task Guild(CommandContext e)
        {
            var roles = e.Guild.Roles.Select(xr => xr.Mention);
            var overs = e.Channel.PermissionOverwrites.Select(xo => string.Concat("Principal: ", xo.Id, " (", xo.Type, "), Allow: ", (ulong)xo.Allow, "; Deny: ", (ulong)xo.Deny));

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

            await e.Message.RespondAsync("", embed: embed);
        }

        [Command("embed")]
        public async Task Embed(CommandContext e)
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
            await e.Message.RespondAsync("testing embed:", embed: embed);
        }

        [Command("appinfo")]
        public async Task AppInfo(CommandContext e)
        {
            var app = e.Client.CurrentApplication;
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
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "ID", Value = app.Id.ToString() });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Created", Value = app.CreationDate.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") });
            embed.Fields.Add(new DiscordEmbedField { Inline = true, Name = "Owner", Value = $"{usrn}#{app.Owner.Discriminator} ({app.Owner.Id})" });

            await e.Message.RespondAsync("", embed: embed);
        }

        [Command("modifyme")]
        public async Task ModifyMe(CommandContext e) =>
            await e.Member.ModifyAsync("Tests D#+ instead of going outside", null, null, null, null, "D#+ Testing");

        [Group("voice"), Description("Provides voice commands."), Aliases("audio")]
        public class VoiceCommands
        {
            private CancellationTokenSource AudioLoopCancelTokenSource { get; set; }
            private CancellationToken AudioLoopCancelToken => this.AudioLoopCancelTokenSource.Token;
            private Task AudioLoopTask { get; set; }

            private FileStream _fs;
            private uint _ssrc;
            private async Task OnUserSpeaking(VoiceReceivedEventArgs e)
            {
                if (this._ssrc == 0)
                    this._ssrc = e.SSRC;

                if (e.SSRC != this._ssrc)
                    return;

                e.Client.DebugLogger.LogMessage(LogLevel.Debug, "VNEXT RX", $"{e.User?.Username ?? "Unknown user"} sent voice data.", DateTime.Now);
                var buff = e.Voice.ToArray();
                await this._fs.WriteAsync(buff, 0, buff.Length);
                await this._fs.FlushAsync();
            }

            [Command("join")]
            public async Task VoiceJoin(CommandContext ctx)
            {
                var vs = ctx.Member?.VoiceState;
                if (vs == null)
                {
                    await ctx.Message.RespondAsync("You are not in a voice channel");
                    return;
                }

                var chn = vs.Channel;
                if (chn == null)
                {
                    await ctx.Message.RespondAsync("Your voice channel was not found");
                    return;
                }

                var voice = ctx.Client.GetVoiceNextClient();
                if (voice == null)
                {
                    await ctx.Message.RespondAsync("Voice is not activated");
                    return;
                }

                await Task.Yield();
                var vnc = await voice.ConnectAsync(chn);
                await ctx.Message.RespondAsync($"Tryina join `{chn.Name}` ({chn.Id})");

                this._fs = File.Create("data.pcm");
                vnc.VoiceReceived += this.OnUserSpeaking;
            }

            [Command("leave")]
            public async Task VoiceLeave(CommandContext ctx)
            {
                var voice = ctx.Client.GetVoiceNextClient();
                if (voice == null)
                {
                    await ctx.Message.RespondAsync("Voice is not activated");
                    return;
                }

                var vnc = voice.GetConnection(ctx.Guild);
                if (vnc == null)
                {
                    await ctx.Message.RespondAsync("Voice is not connected in this guild");
                    return;
                }

                vnc.VoiceReceived -= this.OnUserSpeaking;
                this._fs.Dispose();

                vnc.Disconnect();
                await ctx.Message.RespondAsync("Disconnected");
            }

            [Command("play")]
            public async Task VoicePlay(CommandContext ctx, params string[] filename)
            {
                var voice = ctx.Client.GetVoiceNextClient();
                if (voice == null)
                {
                    await ctx.Message.RespondAsync("Voice is not activated");
                    return;
                }

                var vnc = voice.GetConnection(ctx.Guild);
                if (vnc == null)
                {
                    await ctx.Message.RespondAsync("Voice is not connected in this guild");
                    return;
                }

                var snd = string.Join(" ", filename);
                if (string.IsNullOrWhiteSpace(snd) || !File.Exists(snd))
                {
                    await ctx.Message.RespondAsync("Invalid file specified");
                    return;
                }

                while (vnc.IsPlaying)
                {
                    await ctx.Message.RespondAsync("This connection is playing audio, waiting for end.");
                    await vnc.WaitForPlaybackFinishAsync();
                }

                var exc = (Exception)null;
                await ctx.Message.RespondAsync($"Playing `{snd}`");
                await vnc.SendSpeakingAsync(true);
                try
                {
                    // borrowed from
                    // https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs

                    var ffmpeg_inf = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var ffmpeg = Process.Start(ffmpeg_inf);
                    var ffout = ffmpeg.StandardOutput.BaseStream;

                    using (var ms = new MemoryStream()) // if ffmpeg quits fast, that'll hold the data
                    {
                        await ffout.CopyToAsync(ms);
                        ms.Position = 0;

                        var buff = new byte[3840]; // buffer to hold the PCM data
                        var br = 0;
                        while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                        {
                            if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                                for (var i = br; i < buff.Length; i++)
                                    buff[i] = 0;

                            await vnc.SendAsync(buff, 20); // we're sending 20ms of data
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

            [Command("playloop")]
            public async Task VoicePlayLoop(CommandContext ctx, params string[] filename)
            {
                var voice = ctx.Client.GetVoiceNextClient();
                if (voice == null)
                {
                    await ctx.Message.RespondAsync("Voice is not activated");
                    return;
                }

                var vnc = voice.GetConnection(ctx.Guild);
                if (vnc == null)
                {
                    await ctx.Message.RespondAsync("Voice is not connected in this guild");
                    return;
                }

                if (this.AudioLoopTask != null && !this.AudioLoopCancelToken.IsCancellationRequested)
                {
                    await ctx.Message.RespondAsync("Audio loop is already playing");
                    return;
                }

                var snd = string.Join(" ", filename);
                if (string.IsNullOrWhiteSpace(snd) || !File.Exists(snd))
                {
                    await ctx.Message.RespondAsync("Invalid file specified");
                    return;
                }

                await ctx.Message.RespondAsync($"Playing `{snd}` in a loop");
                this.AudioLoopCancelTokenSource = new CancellationTokenSource();
                this.AudioLoopTask = Task.Run(async () =>
                {
                    var chn = ctx.Channel;
                    var token = this.AudioLoopCancelToken;
                    await vnc.SendSpeakingAsync(true);
                    try
                    {
                        // borrowed from
                        // https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs

                        var ffmpeg_inf = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 pipe:1",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };
                        var ffmpeg = Process.Start(ffmpeg_inf);
                        var ffout = ffmpeg.StandardOutput.BaseStream;

                        using (var ms = new MemoryStream()) // this will hold our PCM data
                        {
                            await ffout.CopyToAsync(ms);
                            ms.Position = 0;

                            var buff = new byte[3840]; // buffer to hold the PCM data
                            var br = 0;
                            while (true)
                            {
                                while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                                {
                                    if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                                        for (var i = br; i < buff.Length; i++)
                                            buff[i] = 0;

                                    await vnc.SendAsync(buff, 20); // we're sending 20ms of data
                                    token.ThrowIfCancellationRequested();
                                }
                                ms.Position = 0;
                                token.ThrowIfCancellationRequested();
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex) { await chn.SendMessageAsync($"Audio loop crashed: {ex.GetType()}: {ex.Message}"); }
                    finally
                    {
                        await vnc.SendSpeakingAsync(false);
                    }
                }, this.AudioLoopCancelToken);
            }

            [Command("playstop")]
            public async Task VoicePlayLoopStop(CommandContext ctx)
            {
                var voice = ctx.Client.GetVoiceNextClient();
                if (voice == null)
                {
                    await ctx.Message.RespondAsync("Voice is not activated");
                    return;
                }

                var vnc = voice.GetConnection(ctx.Guild);
                if (vnc == null)
                {
                    await ctx.Message.RespondAsync("Voice is not connected in this guild");
                    return;
                }

                if (this.AudioLoopTask == null || this.AudioLoopCancelToken.IsCancellationRequested)
                {
                    await ctx.Message.RespondAsync("Audio loop is already paused");
                    return;
                }

                this.AudioLoopCancelTokenSource.Cancel();
                await this.AudioLoopTask;
                this.AudioLoopTask = null;

                await ctx.Message.RespondAsync("Audio loop stopped");
            }

            [Command("playforce"), Description("Forces audio playback, regardless of whether audio is playing or not.")]
            public async Task VoicePlayForce(CommandContext ctx, params string[] filename)
            {
                var voice = ctx.Client.GetVoiceNextClient();
                if (voice == null)
                {
                    await ctx.Message.RespondAsync("Voice is not activated");
                    return;
                }

                var vnc = voice.GetConnection(ctx.Guild);
                if (vnc == null)
                {
                    await ctx.Message.RespondAsync("Voice is not connected in this guild");
                    return;
                }

                var snd = string.Join(" ", filename);
                if (string.IsNullOrWhiteSpace(snd) || !File.Exists(snd))
                {
                    await ctx.Message.RespondAsync("Invalid file specified");
                    return;
                }

                var exc = (Exception)null;
                await ctx.Message.RespondAsync($"Playing `{snd}`");
                await vnc.SendSpeakingAsync(true);
                try
                {
                    // borrowed from
                    // https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs

                    var ffmpeg_inf = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{snd}\" -ac 2 -f s16le -ar 48000 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var ffmpeg = Process.Start(ffmpeg_inf);
                    var ffout = ffmpeg.StandardOutput.BaseStream;

                    using (var ms = new MemoryStream()) // if ffmpeg quits fast, that'll hold the data
                    {
                        await ffout.CopyToAsync(ms);
                        ms.Position = 0;

                        var buff = new byte[3840]; // buffer to hold the PCM data
                        var br = 0;
                        while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                        {
                            if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                                for (var i = br; i < buff.Length; i++)
                                    buff[i] = 0;

                            await vnc.SendAsync(buff, 20); // we're sending 20ms of data
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
        }
    }
}
