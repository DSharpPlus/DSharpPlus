// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Test
{
    internal sealed class TestBot
    {
        internal static EventId TestBotEventId { get; } = new EventId(1000, "TestBot");

        private TestBotConfig Config { get; }
        public DiscordClient Discord { get; }
        private VoiceNextExtension VoiceService { get; }
        private CommandsNextExtension CommandsNextService { get; }
        private InteractivityExtension InteractivityService { get; }
        private LavalinkExtension LavalinkService { get; }
        private SlashCommandsExtension SlashCommandService { get; }

        public TestBot(TestBotConfig cfg, int shardid)
        {
            // global bot config
            this.Config = cfg;

            // discord instance config and the instance itself
            var dcfg = new DiscordConfiguration
            {
                AutoReconnect = true,
                LargeThreshold = 250,
                MinimumLogLevel = LogLevel.Debug,
                Token = this.Config.Token,
                TokenType = TokenType.Bot,
                ShardId = shardid,
                ShardCount = this.Config.ShardCount,
                MessageCacheSize = 2048,
                LogTimestampFormat = "dd-MM-yyyy HH:mm:ss zzz",
                Intents = DiscordIntents.All // if 4013 is received, change to DiscordIntents.AllUnprivileged
            };
            this.Discord = new DiscordClient(dcfg);

            // events
            this.Discord.Ready += this.Discord_Ready;
            this.Discord.GuildStickersUpdated += this.Discord_StickersUpdated;
            this.Discord.GuildAvailable += this.Discord_GuildAvailable;
            //Discord.PresenceUpdated += this.Discord_PresenceUpdated;
            //Discord.ClientErrored += this.Discord_ClientErrored;
            this.Discord.SocketErrored += this.Discord_SocketError;
            this.Discord.GuildCreated += this.Discord_GuildCreated;
            this.Discord.VoiceStateUpdated += this.Discord_VoiceStateUpdated;
            this.Discord.GuildDownloadCompleted += this.Discord_GuildDownloadCompleted;
            this.Discord.GuildUpdated += this.Discord_GuildUpdated;
            this.Discord.ChannelDeleted += this.Discord_ChannelDeleted;

            this.Discord.InteractionCreated += this.Discord_InteractionCreated;
            //this.Discord.ComponentInteractionCreated += this.RoleMenu;
            //this.Discord.ComponentInteractionCreated += this.DiscordComponentInteractionCreated;
            //this.Discord.InteractionCreated += this.SendButton;
            // For event timeout testing
            //Discord.GuildDownloadCompleted += async (s, e) =>
            //{
            //    await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            //    throw new Exception("Flippin' tables");
            //};

            this.Discord.ThreadCreated += this.Discord_ThreadCreated;
            this.Discord.ThreadUpdated += this.Discord_ThreadUpdated;
            this.Discord.ThreadDeleted += this.Discord_ThreadDeleted;
            this.Discord.ThreadListSynced += this.Discord_ThreadListSynced;
            this.Discord.ThreadMemberUpdated += this.Discord_ThreadMemberUpdated;
            this.Discord.ThreadMembersUpdated += this.Discord_ThreadMembersUpdated;

            // voice config and the voice service itself
            var vcfg = new VoiceNextConfiguration
            {
                AudioFormat = AudioFormat.Default,
                EnableIncoming = true
            };
            this.VoiceService = this.Discord.UseVoiceNext(vcfg);

            // build a dependency collection for commandsnext
            var depco = new ServiceCollection();

            // commandsnext config and the commandsnext service itself
            var cncfg = new CommandsNextConfiguration
            {
                StringPrefixes = this.Config.CommandPrefixes,
                EnableDms = true,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                Services = depco.BuildServiceProvider(true),
                IgnoreExtraArguments = false,
                UseDefaultCommandHandler = true,
                DefaultParserCulture = CultureInfo.InvariantCulture,
                //CommandExecutor = new ParallelQueuedCommandExecutor(2),
            };
            this.CommandsNextService = this.Discord.UseCommandsNext(cncfg);
            this.CommandsNextService.CommandErrored += this.CommandsNextService_CommandErrored;
            this.CommandsNextService.CommandExecuted += this.CommandsNextService_CommandExecuted;
            this.CommandsNextService.RegisterCommands(typeof(TestBot).GetTypeInfo().Assembly);
            this.CommandsNextService.SetHelpFormatter<TestBotHelpFormatter>();

            // interactivity service
            var icfg = new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromSeconds(10),
                AckPaginationButtons = true,
                ResponseBehavior = InteractionResponseBehavior.Respond,
                PaginationBehaviour = PaginationBehaviour.Ignore,
                ResponseMessage = "Sorry, but this wasn't a valid option, or does not belong to you!",
                PaginationButtons = new PaginationButtons()
                {
                    Stop = new DiscordButtonComponent(ButtonStyle.Danger, "stop", null, false, new DiscordComponentEmoji(862259725785497620)),
                    Left = new DiscordButtonComponent(ButtonStyle.Secondary, "left", null, false, new DiscordComponentEmoji(862259522478800916)),
                    Right = new DiscordButtonComponent(ButtonStyle.Secondary, "right", null, false, new DiscordComponentEmoji(862259691212242974)),
                    SkipLeft = new DiscordButtonComponent(ButtonStyle.Primary, "skipl", null, false, new DiscordComponentEmoji(862259605464023060)),
                    SkipRight = new DiscordButtonComponent(ButtonStyle.Primary, "skipr", null, false, new DiscordComponentEmoji(862259654403031050))
                }
            };

            this.InteractivityService = this.Discord.UseInteractivity(icfg);
            this.LavalinkService = this.Discord.UseLavalink();

            this.SlashCommandService = this.Discord.UseSlashCommands();
            this.SlashCommandService.SlashCommandErrored += this.SlashCommandService_CommandErrored;
            this.SlashCommandService.SlashCommandInvoked += this.SlashCommandService_CommandReceived;
            this.SlashCommandService.SlashCommandExecuted += this.SlashCommandService_CommandExecuted;

            if (this.Config.SlashCommandGuild != 0)
                this.SlashCommandService.RegisterCommands(typeof(TestBot).GetTypeInfo().Assembly, this.Config.SlashCommandGuild);

            //this.Discord.MessageCreated += async e =>
            //{
            //    if (e.Message.Author.IsBot)
            //        return;

            //    _ = Task.Run(async () => await e.Message.RespondAsync(e.Message.Content)).ConfigureAwait(false);
            //};
        }

        private async Task Discord_InteractionCreated(DiscordClient sender, InteractionCreateEventArgs e)
        {
            if (e.Interaction.Type != InteractionType.AutoComplete)
                return;

            this.Discord.Logger.LogInformation($"AutoComplete: Focused: {e.Interaction.Data.Options.First().Focused}, Data: {e.Interaction.Data.Options.First().Value}");

            var option = e.Interaction.Data.Options.First();

            if (string.IsNullOrEmpty(option.Value as string))
                return;

            var builder = new DiscordInteractionResponseBuilder()
                .AddAutoCompleteChoice(new DiscordAutoCompleteChoice(option.Value as string, "pog ig"));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, builder);

            return;
        }

        private Task Discord_StickersUpdated(DiscordClient sender, GuildStickersUpdateEventArgs e)
        {
            this.Discord.Logger.LogInformation($"{e.Guild.Id}'s stickers updated: {e.StickersBefore.Count()} -> {e.StickersAfter.Count()}");
            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            var act = new DiscordActivity("the screams of your ancestors", ActivityType.ListeningTo);
            await this.Discord.ConnectAsync(act, UserStatus.DoNotDisturb).ConfigureAwait(false);
        }

        public async Task StopAsync() => await this.Discord.DisconnectAsync().ConfigureAwait(false);

        private Task Discord_Ready(DiscordClient client, ReadyEventArgs e) => Task.CompletedTask;

        private Task Discord_GuildAvailable(DiscordClient client, GuildCreateEventArgs e)
        {
            client.Logger.LogInformation(TestBotEventId, "Guild available: '{Guild}'", e.Guild.Name);
            return Task.CompletedTask;
        }

        private Task Discord_GuildCreated(DiscordClient client, GuildCreateEventArgs e)
        {
            client.Logger.LogInformation(TestBotEventId, "Guild created: '{Guild}'", e.Guild.Name);
            return Task.CompletedTask;
        }

        //private Task Discord_ClientErrored(DiscordClient client, ClientErrorEventArgs e)
        //{
        //    e.Client.Logger.LogError(TestBotEventId, e.Exception, "Client threw an exception");
        //    return Task.CompletedTask;
        //}

        private Task Discord_SocketError(DiscordClient client, SocketErrorEventArgs e)
        {
            var ex = e.Exception is AggregateException ae ? ae.InnerException : e.Exception;
            client.Logger.LogError(TestBotEventId, ex, "WebSocket threw an exception");
            return Task.CompletedTask;
        }

        private Task Discord_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            client.Logger.LogDebug(TestBotEventId, "Voice state changed for '{User}' (mute: {MutedBefore} -> {MutedAfter}; deaf: {DeafBefore} -> {DeafAfter})", e.User, e.Before?.IsServerMuted, e.After.IsServerMuted, e.Before?.IsServerDeafened, e.After.IsServerDeafened);
            return Task.CompletedTask;
        }

        private Task Discord_GuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs e)
        {
            client.Logger.LogDebug(TestBotEventId, "Guild download completed");
            return Task.CompletedTask;
        }

        private async Task CommandsNextService_CommandErrored(CommandsNextExtension cnext, CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException && (e.Command == null || e.Command.QualifiedName != "help"))
                return;

            e.Context.Client.Logger.LogError(TestBotEventId, e.Exception, "Exception occurred during {User}'s invocation of '{Command}'", e.Context.User.Username, e.Context.Command.QualifiedName);

            var exs = new List<Exception>();
            if (e.Exception is AggregateException ae)
                exs.AddRange(ae.InnerExceptions);
            else
                exs.Add(e.Exception);

            foreach (var ex in exs)
            {
                if (ex is CommandNotFoundException && (e.Command == null || e.Command.QualifiedName != "help"))
                    return;

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "An exception occurred when executing a command",
                    Description = $"`{e.Exception.GetType()}` occurred when executing `{e.Command.QualifiedName}`.",
                    Timestamp = DateTime.UtcNow
                };
                embed.WithFooter(this.Discord.CurrentUser.Username, this.Discord.CurrentUser.AvatarUrl)
                    .AddField("Message", ex.Message);
                await e.Context.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        private Task CommandsNextService_CommandExecuted(CommandsNextExtension cnext, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(TestBotEventId, "User {User} executed '{Command}' in {Channel}", e.Context.User.Username, e.Command.QualifiedName, e.Context.Channel.Name);
            return Task.CompletedTask;
        }

        private async Task SlashCommandService_CommandErrored(SlashCommandsExtension sc, SlashCommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(TestBotEventId, e.Exception, "Exception occurred during {User}'s invocation of '{Command}'", e.Context.User.Username, e.Context.CommandName);

            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            // let's wrap the response into an embed
            var embed = new DiscordEmbedBuilder
            {
                Title = "Error",
                Description = $"{emoji} Error!",
                Color = new DiscordColor(0xFF0000) // red
            };
            await e.Context.CreateResponseAsync(embed);
        }

        private Task SlashCommandService_CommandReceived(SlashCommandsExtension sc, SlashCommandInvokedEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(TestBotEventId, "User {User} tries to execute '{Command}' in {Channel}", e.Context.User.Username, e.Context.CommandName, e.Context.Channel.Name);
            return Task.CompletedTask;
        }

        private Task SlashCommandService_CommandExecuted(SlashCommandsExtension sc, SlashCommandExecutedEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(TestBotEventId, "User {User} executed '{Command}' in {Channel}", e.Context.User.Username, e.Context.CommandName, e.Context.Channel.Name);
            return Task.CompletedTask;
        }

        private Task Discord_GuildUpdated(DiscordClient client, GuildUpdateEventArgs e)
        {
            var str = new StringBuilder();

            str.AppendLine($"The guild {e.GuildBefore.Name} has been updated.");

            foreach (var prop in typeof(DiscordGuild).GetProperties())
            {
                try
                {
                    var bfr = prop.GetValue(e.GuildBefore);
                    var aft = prop.GetValue(e.GuildAfter);

                    if (bfr is null)
                    {
                        client.Logger.LogDebug(TestBotEventId, "Guild update: property {Property} in before was null", prop.Name);
                    }

                    if (aft is null)
                    {
                        client.Logger.LogDebug(TestBotEventId, "Guild update: property {Property} in after was null", prop.Name);
                    }

                    if (bfr is null || aft is null)
                    {
                        continue;
                    }

                    if (bfr.ToString() == aft.ToString())
                    {
                        continue;
                    }

                    str.AppendLine($" - {prop.Name}: `{bfr}` to `{aft}`");
                }
                catch (Exception ex)
                {
                    client.Logger.LogError(TestBotEventId, ex, "Exception occurred during guild update");
                }
            }

            str.AppendLine($" - VoiceRegion: `{e.GuildBefore.VoiceRegion?.Name}` to `{e.GuildAfter.VoiceRegion?.Name}`");

            Console.WriteLine(str);

            return Task.CompletedTask;
        }

        private async Task Discord_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
        {
            var logs = (await e.Guild.GetAuditLogsAsync(5, null, AuditLogActionType.ChannelDelete).ConfigureAwait(false)).Cast<DiscordAuditLogChannelEntry>();
            foreach (var entry in logs)
            {
                Console.WriteLine("TargetId: " + entry.Target.Id);
            }
        }

        private Task Discord_ThreadCreated(DiscordClient client, ThreadCreateEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, $"Thread created in {e.Guild.Name}. Thread Name: {e.Thread.Name}");
            return Task.CompletedTask;
        }

        private Task Discord_ThreadUpdated(DiscordClient client, ThreadUpdateEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, $"Thread updated in {e.Guild.Name}. New Thread Name: {e.ThreadAfter.Name}");
            return Task.CompletedTask;
        }

        private Task Discord_ThreadDeleted(DiscordClient client, ThreadDeleteEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, $"Thread deleted in {e.Guild.Name}. Thread Name: {e.Thread.Name ?? "Unknown"}");
            return Task.CompletedTask;
        }

        private Task Discord_ThreadListSynced(DiscordClient client, ThreadListSyncEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, $"Threads synced in {e.Guild.Name}.");
            return Task.CompletedTask;
        }

        private Task Discord_ThreadMemberUpdated(DiscordClient client, ThreadMemberUpdateEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, $"Thread member updated.");
            Console.WriteLine($"Discord_ThreadMemberUpdated fired for thread {e.ThreadMember.ThreadId}. User ID {e.ThreadMember.Id}.");
            return Task.CompletedTask;
        }

        private Task Discord_ThreadMembersUpdated(DiscordClient client, ThreadMembersUpdateEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, $"Thread members updated in {e.Guild.Name}.");
            return Task.CompletedTask;
        }
    }
}
