using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;

namespace DSharpPlus.Test
{
    internal sealed class TestBot
    {
        private TestBotConfig Config { get; }
        public DiscordClient Discord;
        private TestBotCommands Commands { get; }
        private VoiceNextClient VoiceService { get; }
        private CommandsNextModule CommandsNextService { get; }
        private InteractivityModule InteractivityService { get; }
        private Timer GameGuard { get; set; }

        public TestBot(TestBotConfig cfg, int shardid)
        {
            // global bot config
            this.Config = cfg;

            // discord instance config and the instance itself
            var dcfg = new DiscordConfig
            {
                AutoReconnect = true,
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,
                Token = this.Config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false,
                ShardId = shardid,
                ShardCount = this.Config.ShardCount,
                GatewayVersion = 5,
                EnableCompression = true,
                MessageCacheSize = 50
            };
            Discord = new DiscordClient(dcfg);

            // events
            Discord.DebugLogger.LogMessageReceived += this.DebugLogger_LogMessageReceived;
            Discord.Ready += this.Discord_Ready;
            Discord.GuildAvailable += this.Discord_GuildAvailable;
            Discord.GuildBanAdd += this.Discord_GuildBanAdd;
            Discord.MessageCreated += this.Discord_MessageCreated;
            Discord.MessageReactionAdd += this.Discord_MessageReactionAdd;
            Discord.MessageReactionRemoveAll += this.Discord_MessageReactionRemoveAll;
            Discord.PresenceUpdate += this.Discord_PresenceUpdate;
            Discord.ClientError += this.Discord_ClientError;

            // voice config and the voice service itself
            var vcfg = new VoiceNextConfiguration
            {
                VoiceApplication = VoiceNext.Codec.VoiceApplication.Music
            };
            this.VoiceService = this.Discord.UseVoiceNext(vcfg);

            // commandsnext config and the commandsnext service itself
            var cncfg = new CommandsNextConfiguration
            {
                StringPrefix = this.Config.CommandPrefix,
                CustomPrefixPredicate = msg =>
                {
                    if (TestBotNextCommands.Prefixes.ContainsKey(msg.Channel.Id) && TestBotNextCommands.Prefixes.TryGetValue(msg.Channel.Id, out var pfix))
                        return msg.GetStringPrefixLength(pfix);
                    return -1;
                },
                EnableDms = true,
                EnableMentionPrefix = true
            };
            this.CommandsNextService = Discord.UseCommandsNext(cncfg);
            this.CommandsNextService.CommandErrored += this.CommandsNextService_CommandErrored;
            this.CommandsNextService.CommandExecuted += this.CommandsNextService_CommandExecuted;
            this.CommandsNextService.RegisterCommands<TestBotCommands>();
            this.CommandsNextService.RegisterCommands<TestBotNextCommands>();
            this.CommandsNextService.RegisterCommands<TestBotEvalCommands>();

            // interactivity service
            this.InteractivityService = Discord.UseInteractivity();
        }

        public async Task RunAsync()
        {
            await Discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[{0:yyyy-MM-dd HH:mm:ss zzz}] ", e.Timestamp.ToLocalTime());

            var tag = e.Application;
            if (tag.Length > 12)
                tag = tag.Substring(0, 12);
            if (tag.Length < 12)
                tag = tag.PadLeft(12, ' ');
            Console.Write("[{0}] ", tag);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[{0}] ", string.Concat("SHARD ", this.Discord.ShardId.ToString("00")));

            switch (e.Level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                    
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
            }
            Console.Write("[{0}] ", e.Level.ToString().PadLeft(8));

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(e.Message);
        }

        private Task Discord_Ready(ReadyEventArgs e)
        {
            this.GameGuard = new Timer(TimerCallback, null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(15));
            return Task.Delay(0);
        }

        private Task Discord_GuildAvailable(GuildCreateEventArgs e)
        {
            Discord.DebugLogger.LogMessage(LogLevel.Info, "DSPlus Test", $"Guild available: {e.Guild.Name}", DateTime.Now);
            return Task.Delay(0);
        }

        private async Task Discord_GuildBanAdd(GuildBanAddEventArgs e)
        {
            var usrn = e.Member.Username
                .Replace(@"\", @"\\")
                .Replace(@"*", @"\*")
                .Replace(@"_", @"\_")
                .Replace(@"~", @"\~")
                .Replace(@"`", @"\`");

            var ch = e.Guild.Channels.FirstOrDefault(xc => xc.Name.Contains("logs"));
            if (ch != null)
                await ch.SendMessageAsync($"**{usrn}#{e.Member.Discriminator} got bent**");
        }

        private Task Discord_PresenceUpdate(PresenceUpdateEventArgs e)
        {
            //if (e.User != null)
            //    this.Discord.DebugLogger.LogMessage(LogLevel.Unnecessary, "DSPlus Test", $"{e.User.Username}#{e.User.Discriminator} ({e.UserID}): {e.Status ?? "<unknown>"} playing {e.Game ?? "<nothing>"}", DateTime.Now);

            return Task.Delay(0);
        }

        private async Task Discord_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Message.Content.Contains($"<@!{e.Client.CurrentUser.Id}>") || e.Message.Content.Contains($"<@{e.Client.CurrentUser.Id}>"))
                await e.Message.RespondAsync("r u havin' a ggl thr m8");
        }

        private /*async*/ Task Discord_MessageReactionAdd(MessageReactionAddEventArgs e)
        {
            return Task.Delay(0);

            //await e.Message.DeleteAllReactions();
        }

        private /*async*/ Task Discord_MessageReactionRemoveAll(MessageReactionRemoveAllEventArgs e)
        {
            return Task.Delay(0);

            //await e.Message.DeleteAllReactions();
        }

        private async Task Discord_ClientError(ClientErrorEventArgs e)
        {
            var exs = new List<Exception>();
            if (e.Exception is AggregateException ae)
                exs.AddRange(ae.InnerExceptions);
            else
                exs.Add(e.Exception);

            foreach (var ex in exs)
            {
                var ms = ex.Message;
                var st = ex.StackTrace;

                ms = ms.Length > 1000 ? ms.Substring(0, 1000) : ms;
                st = st.Length > 1000 ? st.Substring(0, 1000) : st;

                var embed = new DiscordEmbed
                {
                    Color = 0xFF0000,
                    Title = "An exception occured within the client",
                    Description = $"Event `{e.EventName}` threw an exception.",
                    Footer = new DiscordEmbedFooter
                    {
                        IconUrl = Discord.CurrentUser.AvatarUrl,
                        Text = Discord.CurrentUser.Username
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<DiscordEmbedField>()
                {
                    new DiscordEmbedField
                    {
                        Name = "Message",
                        Value = ms,
                        Inline = false
                    },
                    new DiscordEmbedField
                    {
                        Name = "Stack trace",
                        Value = $"```cs\n{st}\n```",
                        Inline = false
                    }
                }
                };
                await e.Client.SendMessageAsync(186565646161674240u, "\u200b", embed: embed);
            }
        }

        private async Task CommandsNextService_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException && (e.Command == null || e.Command.QualifiedName != "help"))
                return;

            Discord.DebugLogger.LogMessage(LogLevel.Error, "CommandsNext", $"{e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            var exs = new List<Exception>();
            if (e.Exception is AggregateException ae)
                exs.AddRange(ae.InnerExceptions);
            else
                exs.Add(e.Exception);

            foreach (var ex in exs)
            {
                if (ex is CommandNotFoundException && (e.Command == null || e.Command.QualifiedName != "help"))
                    return;

                var ms = ex.Message;
                var st = ex.StackTrace;

                ms = ms.Length > 1000 ? ms.Substring(0, 1000) : ms;
                st = st.Length > 1000 ? st.Substring(0, 1000) : st;

                var embed = new DiscordEmbed
                {
                    Color = 0xFF0000,
                    Title = "An exception occured when executing a command",
                    Description = $"`{e.Exception.GetType()}` occured when executing `{e.Command.Name}`.",
                    Footer = new DiscordEmbedFooter
                    {
                        IconUrl = Discord.CurrentUser.AvatarUrl,
                        Text = Discord.CurrentUser.Username
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<DiscordEmbedField>()
                    {
                        new DiscordEmbedField
                        {
                            Name = "Message",
                            Value = ms,
                            Inline = false
                        },
                        new DiscordEmbedField
                        {
                            Name = "Stack trace",
                            Value = $"```cs\n{st}\n```",
                            Inline = false
                        }
                    }
                };
                await e.Context.Channel.SendMessageAsync("\u200b", embed: embed);
            }
        }

        private Task CommandsNextService_CommandExecuted(CommandExecutedEventArgs e)
        {
            Discord.DebugLogger.LogMessage(LogLevel.Info, "CommandsNext", $"{e.Context.User.Username} executed {e.Command.Name} in {e.Context.Channel.Name}", DateTime.Now);
            return Task.Delay(0);
        }

        private void TimerCallback(object _)
        {
            try
            {
                this.Discord.UpdateStatusAsync(new Game("gitting gud at API")).GetAwaiter().GetResult();
            }
            catch (Exception) { }
        }
    }
}
