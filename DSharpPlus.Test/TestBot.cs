using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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
            var dcfg = new DiscordConfiguration
            {
                AutoReconnect = true,
                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,
                Token = this.Config.Token,
                TokenType = this.Config.UseUserToken ? TokenType.User : TokenType.Bot,
                UseInternalLogHandler = false,
                ShardId = shardid,
                ShardCount = this.Config.ShardCount,
                EnableCompression = true,
                MessageCacheSize = 50,
                AutomaticGuildSync = !this.Config.UseUserToken
            };
            Discord = new DiscordClient(dcfg);

            // events
            Discord.DebugLogger.LogMessageReceived += this.DebugLogger_LogMessageReceived;
            Discord.Ready += this.Discord_Ready;
            Discord.GuildAvailable += this.Discord_GuildAvailable;
            Discord.GuildBanAdded += this.Discord_GuildBanAdd;
            Discord.MessageCreated += this.Discord_MessageCreated;
            Discord.MessageReactionAdded += this.Discord_MessageReactionAdd;
            Discord.MessageReactionsCleared += this.Discord_MessageReactionRemoveAll;
            Discord.PresenceUpdated += this.Discord_PresenceUpdate;
            Discord.ClientErrored += this.Discord_ClientErrored;
            Discord.SocketErrored += this.Discord_SocketError;
            Discord.GuildCreated += this.Discord_GuildCreated;

            // voice config and the voice service itself
            var vcfg = new VoiceNextConfiguration
            {
                VoiceApplication = VoiceNext.Codec.VoiceApplication.Music,
                EnableIncoming = false
            };
            this.VoiceService = this.Discord.UseVoiceNext(vcfg);

            // build a dependency collection for commandsnext
            var depco = new DependencyCollectionBuilder();
            depco.AddInstance("This is a dependency string.");
            depco.Add<TestDependency>();

            // commandsnext config and the commandsnext service itself
            var cncfg = new CommandsNextConfiguration
            {
                StringPrefix = this.Config.CommandPrefix,
                CustomPrefixPredicate = msg =>
                {
                    if (TestBotNextCommands.Prefixes.ContainsKey(msg.Channel.Id) && TestBotNextCommands.Prefixes.TryGetValue(msg.Channel.Id, out var pfix))
                        return Task.FromResult(msg.GetStringPrefixLength(pfix));
                    return Task.FromResult(-1);
                },
                EnableDms = true,
                EnableMentionPrefix = true,
                CaseSensitive = true,
                Dependencies = depco.Build(),
                SelfBot = this.Config.UseUserToken,
                IgnoreExtraArguments = false
                //DefaultHelpChecks = new List<CheckBaseAttribute>() { new RequireOwnerAttribute() }
            };
            this.CommandsNextService = Discord.UseCommandsNext(cncfg);
            this.CommandsNextService.CommandErrored += this.CommandsNextService_CommandErrored;
            this.CommandsNextService.CommandExecuted += this.CommandsNextService_CommandExecuted;
            //this.CommandsNextService.RegisterCommands<TestBotCommands>();
            //this.CommandsNextService.RegisterCommands<TestBotNextCommands>();
            //this.CommandsNextService.RegisterCommands<TestBotEvalCommands>();
            //this.CommandsNextService.RegisterCommands<TestBotDependentCommands>();
            //this.CommandsNextService.RegisterCommands<TestBotGroupInheritedChecksCommands>();
            this.CommandsNextService.RegisterCommands(typeof(TestBot).GetTypeInfo().Assembly);
            this.CommandsNextService.SetHelpFormatter<TestBotHelpFormatter>();

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
            if (!this.Config.UseUserToken)
                this.GameGuard = new Timer(TimerCallback, null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(15));
            return Task.Delay(0);
        }

        private Task Discord_GuildAvailable(GuildCreateEventArgs e)
        {
            Discord.DebugLogger.LogMessage(LogLevel.Info, "DSPlus Test", $"Guild available: {e.Guild.Name}", DateTime.Now);
            //Discord.DebugLogger.LogMessage(LogLevel.Debug, "DSPlus Test", $"e.Guild.MemberCount: {e.Guild.MemberCount}", DateTime.Now);
            //Discord.DebugLogger.LogMessage(LogLevel.Debug, "DSPlus Test", $"e.Guild.Members.Count: {e.Guild.Members.Count}", DateTime.Now);
            //Discord.DebugLogger.LogMessage(LogLevel.Debug, "DSPlus Test", $"Discord.Guilds[e.Guild.Id].MemberCount: {Discord.Guilds[e.Guild.Id].MemberCount}", DateTime.Now);
            //Discord.DebugLogger.LogMessage(LogLevel.Debug, "DSPlus Test", $"Discord.Guilds[e.Guild.Id].Members.Count: {Discord.Guilds[e.Guild.Id].Members.Count}", DateTime.Now);
            return Task.Delay(0);
        }

        private Task Discord_GuildCreated(GuildCreateEventArgs e)
        {
            // Tryna fix the "double guild member" bug
            Discord.DebugLogger.LogMessage(LogLevel.Info, "DSPlus Test", $"Guild created: {e.Guild.Name}", DateTime.Now);
            //Discord.DebugLogger.LogMessage(LogLevel.Debug, "DSPlus Test", $"e.Guild.MemberCount: {e.Guild.MemberCount}", DateTime.Now);
            //Discord.DebugLogger.LogMessage(LogLevel.Debug, "DSPlus Test", $"e.Guild.Members.Count: {e.Guild.Members.Count}", DateTime.Now);
            //Discord.DebugLogger.LogMessage(LogLevel.Debug, "DSPlus Test", $"Discord.Guilds[e.Guild.Id].MemberCount: {Discord.Guilds[e.Guild.Id].MemberCount}", DateTime.Now);
            //Discord.DebugLogger.LogMessage(LogLevel.Debug, "DSPlus Test", $"Discord.Guilds[e.Guild.Id].Members.Count: {Discord.Guilds[e.Guild.Id].Members.Count}", DateTime.Now);
            return Task.Delay(0);
        }

        private Task Discord_GuildBanAdd(GuildBanAddEventArgs e)
        {
            /*var usrn = e.Member.Username?
                .Replace(@"\", @"\\")
                .Replace(@"*", @"\*")
                .Replace(@"_", @"\_")
                .Replace(@"~", @"\~")
                .Replace(@"`", @"\`");

            var ch = e.Guild.Channels.FirstOrDefault(xc => xc.Name.Contains("logs"));
            if (ch != null)
                await ch.SendMessageAsync($"**{usrn}#{e.Member.Discriminator} got bent**");*/

            return Task.Delay(0);
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

        private /*async*/ Task Discord_MessageReactionRemoveAll(MessageReactionsClearEventArgs e)
        {
            return Task.Delay(0);

            //await e.Message.DeleteAllReactions();
        }

        private Task Discord_ClientErrored(ClientErrorEventArgs e)
        {
            this.Discord.DebugLogger.LogMessage(LogLevel.Error, "DSP Test", $"Client threw an exception: {e.Exception.GetType()}", DateTime.Now);
            return Task.Delay(0);
        }

        private Task Discord_SocketError(SocketErrorEventArgs e)
        {
            this.Discord.DebugLogger.LogMessage(LogLevel.Error, "WebSocket", $"WS threw an exception: {e.Exception.GetType()}", DateTime.Now);
            return Task.Delay(0);
        }

        private async Task CommandsNextService_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException && (e.Command == null || e.Command.QualifiedName != "help"))
                return;

            Discord.DebugLogger.LogMessage(LogLevel.Error, "CommandsNext", $"An exception occured during {e.Context.User.Username}'s invocation of '{e.Context.Command.QualifiedName}': {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

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
                st = !string.IsNullOrWhiteSpace(st) ? (st.Length > 1000 ? st.Substring(0, 1000) : st) : "<no stack trace>";

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "An exception occured when executing a command",
                    Description = $"`{e.Exception.GetType()}` occured when executing `{e.Command.QualifiedName}`.",
                    Timestamp = DateTime.UtcNow
                };
                embed.WithFooter(Discord.CurrentUser.Username, Discord.CurrentUser.AvatarUrl)
                    .AddField("Message", ms, false)
                    .AddField("Stack trace", $"```cs\n{st}\n```", false);
                await e.Context.Channel.SendMessageAsync("\u200b", embed: embed.Build());
            }
        }

        private Task CommandsNextService_CommandExecuted(CommandExecutionEventArgs e)
        {
            Discord.DebugLogger.LogMessage(LogLevel.Info, "CommandsNext", $"{e.Context.User.Username} executed '{e.Command.QualifiedName}' in {e.Context.Channel.Name}", DateTime.Now);
            return Task.Delay(0);
        }

        private void TimerCallback(object _)
        {
            try
            {
                this.Discord.UpdateStatusAsync(new Game("gitting better at API")).GetAwaiter().GetResult();
            }
            catch (Exception) { }
        }
    }
}
