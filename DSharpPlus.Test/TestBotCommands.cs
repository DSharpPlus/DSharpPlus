using System;
using System.Collections.Generic;
//using System.IO;
using System.Linq;
//using System.Speech.Synthesis;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
//using DSharpPlus.VoiceNext;
//using NAudio.Wave;
using DSharpPlus.Interactivity;
using System.IO;

namespace DSharpPlus.Test
{
    public sealed class TestBotCommands
    {
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
            await e.Client.SendMessageAsync(e.Message.Channel.Id, $@"```
Servername: {e.Guild.Name}
Serverowner: {e.Guild.OwnerID}
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
            await e.Message.DeleteAsync();
            var list = await e.Client.GetInteractivityModule().CollectReactionsAsync(m, TimeSpan.FromSeconds(30));
            string reactions = "We're done people!\n\nReactions:";
            foreach (var collected in list)
            {
                reactions += "\n" + collected.Key + ": " + collected.Value + "times!";
            }
            await m.RespondAsync(reactions);
        }

        [Command("kill")]
        public async Task Kill(CommandContext e)
        {
            await e.Channel.SendMessageAsync("kthxbai 👋");
            e.Client.Dispose();
            await Task.Delay(-1);
        }

        [Command("reconnect")]
        public async Task Restart(CommandContext e) =>
            await e.Client.ReconnectAsync();

        [Command("purgechannel")]
        public async Task PurgeChannel(CommandContext e)
        {
            var ids = (await e.Channel.GetMessagesAsync(before: e.Message.Id, limit: 50))
                .Select(y => y.Id)
                .ToList();
            await e.Channel.BulkDeleteMessagesAsync(ids);
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
            var app = await e.Client.GetCurrentAppAsync();
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
            await e.Client.ModifyMemberAsync(e.Guild.Id, e.User.Id, "Tests D#+ instead of going outside");

        /*[Command("voicejoin")]
        public async Task VoiceJoin(CommandContext e)
        {
            var vs = e.Guild.VoiceStates.FirstOrDefault(xvs => xvs.UserID == e.User.ID);
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

            var voice = e.Client.GetVoiceNextClient();
            if (voice == null)
            {
                await e.Message.Respond("Voice is not activated");
                return;
            }

            await Task.Yield();
            await voice.ConnectAsync(chn);
            await e.Message.Respond($"Tryina join `{chn.Name}` ({chn.ID})");
        }

        [Command("voiceleave")]
        public async Task VoiceLeave(CommandContext e)
        {
            var voice = e.Client.GetVoiceNextClient();
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

        [Command("voiceplay")]
        public async Task VoicePlay(CommandContext e)
        {
            var voice = e.Client.GetVoiceNextClient();
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

        [Command("voicespeak")]
        public async Task VoiceSpeak(CommandContext e)
        {
            var voice = e.Client.GetVoiceNextClient();
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
