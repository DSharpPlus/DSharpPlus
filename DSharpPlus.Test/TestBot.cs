using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using c = DSharpPlus.Commands;
using DSharpPlus.CommandsNext;
//using DSharpPlus.VoiceNext;
using DSharpPlus.Interactivity;

namespace DSharpPlus.Test
{
    internal sealed class TestBot
    {
        private TestBotConfig Config { get; }
        public static DiscordClient Discord;
        private TestBotCommands Commands { get; }
        private c.CommandModule CommandService { get; }
        //private VoiceNextClient VoiceService { get; }
        private CommandsNextModule CommandsNextService { get; }
        private Timer GameGuard { get; set; }

        public TestBot(TestBotConfig cfg)
        {
            // global bot config
            this.Config = cfg;

            // discord instance config and the instance itself
            var dcfg = new DiscordConfig
            {
                AutoReconnect = true,
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250, 
                // Use unnecessary instead of debug for more verbosity
                //LogLevel = LogLevel.Unnecessary,
                LogLevel = LogLevel.Debug,
                Token = this.Config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false,
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

            // command config and the command service itself
            this.Commands = new TestBotCommands();
            var ccfg = new c.CommandConfig
            {
                Prefix = this.Config.CommandPrefix,
                SelfBot = false
            };

            Discord.UseInteractivity();

            this.CommandService = Discord.UseCommands(ccfg);
            this.CommandService.CommandError += this.CommandService_CommandError;

            // register all commands dynamically
            var t = new[] { typeof(TestBotCommands), typeof(Task), typeof(c.CommandEventArgs) };
            var cm = t[0].GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(xm => this.IsCommandMethod(xm, t[1], t[2]));

            var expr_inst = Expression.Constant(this.Commands);
            var expr_arg0 = Expression.Parameter(t[2]);
            foreach (var xm in cm)
            {
                var expr_call = Expression.Call(expr_inst, xm, expr_arg0);
                var expr_anon = Expression.Lambda<Func<c.CommandEventArgs, Task>>(expr_call, expr_arg0);
                var cmcall = expr_anon.Compile();

                Discord.AddCommand(xm.Name.ToLower(), cmcall);
                Discord.DebugLogger.LogMessage(LogLevel.Info, "DSPlus Test", $"Command {xm.Name.ToLower()} registered", DateTime.Now);
            }

            // voice config and the voice service itself
            /*var vcfg = new VoiceNextConfiguration
            {
                VoiceApplication = VoiceNext.Codec.VoiceApplication.Music
            };
            this.VoiceService = this.Discord.UseVoiceNext(vcfg);*/

            // commandsnext config and the commandsnext service itself
            var cncfg = new CommandsNextConfiguration
            {
                Prefix = "==",
                EnableDms = false,
                EnableMentionPrefix = true
            };
            this.CommandsNextService = Discord.UseCommandsNext(cncfg);
            this.CommandsNextService.CommandErrored += this.CommandsNextService_CommandErrored;
            this.CommandsNextService.CommandExecuted += this.CommandsNextService_CommandExecuted;
            this.CommandsNextService.RegisterCommands<TestBotNextCommands>();
        }

        public async Task RunAsync()
        {
            await Discord.Connect();
            await Task.Delay(-1);
        }

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[{0:yyyy-MM-dd HH:mm:ss zzz}] ", e.TimeStamp.ToLocalTime());

            var tag = e.Application;
            if (tag.Length > 12)
                tag = tag.Substring(0, 12);
            if (tag.Length < 12)
                tag = tag.PadLeft(12, ' ');
            Console.Write("[{0}] ", tag);

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

                case LogLevel.Unnecessary:
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
            }
            Console.Write("[{0}] ", e.Level.ToString().PadLeft(11));

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(e.Message);
        }

        private Task Discord_Ready()
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
            var usrn = e.User.Username
                .Replace(@"\", @"\\")
                .Replace(@"*", @"\*")
                .Replace(@"_", @"\_")
                .Replace(@"~", @"\~")
                .Replace(@"`", @"\`");

            var ch = e.Guild.Channels.FirstOrDefault(xc => xc.Name.Contains("logs"));
            if (ch != null)
                await ch.SendMessage($"**{usrn}#{e.User.Discriminator.ToString("0000")} got bent**");
        }

        private Task Discord_PresenceUpdate(PresenceUpdateEventArgs e)
        {
            //if (e.User != null)
            //    this.Discord.DebugLogger.LogMessage(LogLevel.Unnecessary, "DSPlus Test", $"{e.User.Username}#{e.User.Discriminator} ({e.UserID}): {e.Status ?? "<unknown>"} playing {e.Game ?? "<nothing>"}", DateTime.Now);

            return Task.Delay(0);
        }

        private async Task Discord_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Message.Content.Contains("<@!276042646693216258>") || e.Message.Content.Contains("<@276042646693216258>"))
                await e.Message.Respond("r u havin' a ggl thr m8");
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

        private async Task CommandService_CommandError(c.CommandErrorEventArgs e)
        {
            var ms = e.Exception.Message;
            var st = e.Exception.StackTrace;

            ms = ms.Length > 1000 ? ms.Substring(0, 1000) : ms;
            st = st.Length > 1000 ? st.Substring(0, 1000) : st;

            var embed = new DiscordEmbed
            {
                Color = 0xFF0000,
                Title = "An exception occured when executing a command",
                Description = $"`{e.Exception.GetType()}` occured when executing `{e.Command.Name}`.",
                Footer = new DiscordEmbedFooter
                {
                    IconUrl = Discord.Me.AvatarUrl,
                    Text = Discord.Me.Username
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
            await e.Message.Respond("\u200b", embed: embed);
        }

        private async Task CommandsNextService_CommandErrored(CommandsNext.CommandErrorEventArgs e)
        {
            Discord.DebugLogger.LogMessage(LogLevel.Error, "CommandsNext", $"{e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            var ms = e.Exception.Message;
            var st = e.Exception.StackTrace;

            ms = ms.Length > 1000 ? ms.Substring(0, 1000) : ms;
            st = st.Length > 1000 ? st.Substring(0, 1000) : st;

            var embed = new DiscordEmbed
            {
                Color = 0xFF0000,
                Title = "An exception occured when executing a command",
                Description = $"`{e.Exception.GetType()}` occured when executing `{e.Command.Name}`.",
                Footer = new DiscordEmbedFooter
                {
                    IconUrl = Discord.Me.AvatarUrl,
                    Text = Discord.Me.Username
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
            await e.Context.Channel.SendMessage("\u200b", embed: embed);
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
                Discord.UpdateStatus("testing with Chell").GetAwaiter().GetResult();
            }
            catch (Exception) { }
        }

        private bool IsCommandMethod(MethodInfo method, Type return_type, params Type[] arg_types)
        {
            if (method.ReturnType != return_type)
                return false;

            var prms = method.GetParameters();
            if (prms.Length != arg_types.Length)
                return false;

            for (var i = 0; i < arg_types.Length; i++)
                if (prms[i].ParameterType != arg_types[i])
                    return false;

            return true;
        }
    }
}
