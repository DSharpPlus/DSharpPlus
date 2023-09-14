// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DSharpPlus;

/// <summary>
/// A Discord client that shards automatically.
/// </summary>
public sealed partial class DiscordShardedClient
{
    #region Public Properties

    /// <summary>
    /// Gets the logger for this client.
    /// </summary>
    public ILogger<BaseDiscordClient> Logger { get; }

    /// <summary>
    /// Gets all client shards.
    /// </summary>
    public IReadOnlyDictionary<int, DiscordClient> ShardClients { get; }

    /// <summary>
    /// Gets the gateway info for the client's session.
    /// </summary>
    public GatewayInfo GatewayInfo { get; private set; }

    /// <summary>
    /// Gets the current user.
    /// </summary>
    public DiscordUser CurrentUser { get; private set; }

    /// <summary>
    /// Gets the current application.
    /// </summary>
    public DiscordApplication CurrentApplication { get; private set; }

    /// <summary>
    /// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
    /// </summary>
    public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions
        => this._voiceRegionsLazy?.Value;

    #endregion

    #region Private Properties/Fields

    private DiscordConfiguration Configuration { get; }

    /// <summary>
    /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
    /// </summary>
    private ConcurrentDictionary<string, DiscordVoiceRegion> _internalVoiceRegions;

    private readonly ConcurrentDictionary<int, DiscordClient> _shards = new();
    private Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> _voiceRegionsLazy;
    private bool _isStarted;
    private readonly bool _manuallySharding;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes new auto-sharding Discord client.
    /// </summary>
    /// <param name="config">Configuration to use.</param>
    public DiscordShardedClient(DiscordConfiguration config)
    {
        this.InternalSetup();

        if (config.ShardCount > 1)
        {
            this._manuallySharding = true;
        }

        this.Configuration = config;
        this.ShardClients = new ReadOnlyConcurrentDictionary<int, DiscordClient>(this._shards);

        if (this.Configuration.LoggerFactory == null)
        {
            this.Configuration.LoggerFactory = new DefaultLoggerFactory();
            this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this.Configuration.MinimumLogLevel, this.Configuration.LogTimestampFormat));
        }
        this.Logger = this.Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes and connects all shards.
    /// </summary>
    /// <exception cref="AggregateException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns></returns>
    public async Task StartAsync()
    {
        if (this._isStarted)
        {
            throw new InvalidOperationException("This client has already been started.");
        }

        this._isStarted = true;

        try
        {
            if (this.Configuration.TokenType != TokenType.Bot)
            {
                this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
            }

            this.Logger.LogInformation(LoggerEvents.Startup, "DSharpPlus, version {Version}", this._versionString.Value);

            int shardc = await this.InitializeShardsAsync();
            List<Task> connectTasks = new List<Task>();
            this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booting {ShardCount} shards.", shardc);

            for (int i = 0; i < shardc; i++)
            {
                //This should never happen, but in case it does...
                if (this.GatewayInfo.SessionBucket.MaxConcurrency < 1)
                {
                    this.GatewayInfo.SessionBucket.MaxConcurrency = 1;
                }

                if (this.GatewayInfo.SessionBucket.MaxConcurrency == 1)
                {
                    await this.ConnectShardAsync(i);
                }
                else
                {
                    //Concurrent login.
                    connectTasks.Add(this.ConnectShardAsync(i));

                    if (connectTasks.Count == this.GatewayInfo.SessionBucket.MaxConcurrency)
                    {
                        await Task.WhenAll(connectTasks);
                        connectTasks.Clear();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await this.InternalStopAsync(false);

            string message = $"Shard initialization failed, check inner exceptions for details: ";

            this.Logger.LogCritical(LoggerEvents.ShardClientError, $"{message}\n{ex}");
            throw new AggregateException(message, ex);
        }
    }
    /// <summary>
    /// Disconnects and disposes of all shards.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task StopAsync()
        => this.InternalStopAsync();

    /// <summary>
    /// Gets a shard from a guild ID.
    /// <para>
    ///     If automatically sharding, this will use the <see cref="Utilities.GetShardId(ulong, int)"/> method.
    ///     Otherwise if manually sharding, it will instead iterate through each shard's guild caches.
    /// </para>
    /// </summary>
    /// <param name="guildId">The guild ID for the shard.</param>
    /// <returns>The found <see cref="DiscordClient"/> shard. Otherwise <see langword="null"/> if the shard was not found for the guild ID.</returns>
    public DiscordClient GetShard(ulong guildId)
    {
        int index = this._manuallySharding ? this.GetShardIdFromGuilds(guildId) : Utilities.GetShardId(guildId, this.ShardClients.Count);

        return index != -1 ? this._shards[index] : null;
    }

    /// <summary>
    /// Gets a shard from a guild.
    /// <para>
    ///     If automatically sharding, this will use the <see cref="Utilities.GetShardId(ulong, int)"/> method.
    ///     Otherwise if manually sharding, it will instead iterate through each shard's guild caches.
    /// </para>
    /// </summary>
    /// <param name="guild">The guild for the shard.</param>
    /// <returns>The found <see cref="DiscordClient"/> shard. Otherwise <see langword="null"/> if the shard was not found for the guild.</returns>
    public DiscordClient GetShard(DiscordGuild guild)
        => this.GetShard(guild.Id);

    /// <summary>
    /// Updates playing statuses on all shards.
    /// </summary>
    /// <param name="activity">Activity to set.</param>
    /// <param name="userStatus">Status of the user.</param>
    /// <param name="idleSince">Since when is the client performing the specified activity.</param>
    /// <returns>Asynchronous operation.</returns>
    public async Task UpdateStatusAsync(DiscordActivity activity = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null)
    {
        List<Task> tasks = new List<Task>();
        foreach (DiscordClient client in this._shards.Values)
        {
            tasks.Add(client.UpdateStatusAsync(activity, userStatus, idleSince));
        }

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Internal Methods

    public async Task<int> InitializeShardsAsync()
    {
        if (this._shards.Count != 0)
        {
            return this._shards.Count;
        }

        this.GatewayInfo = await this.GetGatewayInfoAsync();
        int shardc = this.Configuration.ShardCount == 1 ? this.GatewayInfo.ShardCount : this.Configuration.ShardCount;
        ShardedLoggerFactory lf = new ShardedLoggerFactory(this.Logger);
        for (int i = 0; i < shardc; i++)
        {
            DiscordConfiguration cfg = new DiscordConfiguration(this.Configuration)
            {
                ShardId = i,
                ShardCount = shardc,
                LoggerFactory = lf
            };

            DiscordClient client = new DiscordClient(cfg);
            if (!this._shards.TryAdd(i, client))
            {
                throw new InvalidOperationException("Could not initialize shards.");
            }
        }

        return shardc;
    }

    #endregion

    #region Private Methods/Version Property

    private async Task<GatewayInfo> GetGatewayInfoAsync()
    {
        string url = $"{Utilities.GetApiBaseUri()}{Endpoints.GATEWAY}{Endpoints.BOT}";
        HttpClient http = new HttpClient();

        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
        http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(this.Configuration));

        this.Logger.LogDebug(LoggerEvents.ShardRest, $"Obtaining gateway information from GET {Endpoints.GATEWAY}{Endpoints.BOT}...");
        HttpResponseMessage resp = await http.GetAsync(url);

        http.Dispose();

        if (!resp.IsSuccessStatusCode)
        {
            bool ratelimited = await HandleHttpError(url, resp);

            if (ratelimited)
            {
                return await this.GetGatewayInfoAsync();
            }
        }

        Stopwatch timer = new Stopwatch();
        timer.Start();

        JObject jo = JObject.Parse(await resp.Content.ReadAsStringAsync());
        GatewayInfo info = jo.ToDiscordObject<GatewayInfo>();

        //There is a delay from parsing here.
        timer.Stop();

        info.SessionBucket.ResetAfterInternal -= (int)timer.ElapsedMilliseconds;
        info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.ResetAfterInternal);

        return info;

        async Task<bool> HandleHttpError(string reqUrl, HttpResponseMessage msg)
        {
            int code = (int)msg.StatusCode;

            if (code == 401 || code == 403)
            {
                throw new Exception($"Authentication failed, check your token and try again: {code} {msg.ReasonPhrase}");
            }
            else if (code == 429)
            {
                this.Logger.LogError(LoggerEvents.ShardClientError, $"Ratelimit hit, requeuing request to {reqUrl}");

                Dictionary<string, string> hs = msg.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
                int waitInterval = 0;

                if (hs.TryGetValue("Retry-After", out string? retryAfterRaw))
                {
                    waitInterval = int.Parse(retryAfterRaw, CultureInfo.InvariantCulture);
                }

                await Task.Delay(waitInterval);
                return true;
            }
            else if (code >= 500)
            {
                throw new Exception($"Internal Server Error: {code} {msg.ReasonPhrase}");
            }
            else
            {
                throw new Exception($"An unsuccessful HTTP status code was encountered: {code} {msg.ReasonPhrase}");
            }
        }
    }


    private readonly Lazy<string> _versionString = new(() =>
    {
        Assembly a = typeof(DiscordShardedClient).GetTypeInfo().Assembly;

        AssemblyInformationalVersionAttribute? iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (iv != null)
        {
            return iv.InformationalVersion;
        }

        Version? v = a.GetName().Version;
        string vs = v.ToString(3);

        if (v.Revision > 0)
        {
            vs = $"{vs}, CI build {v.Revision}";
        }

        return vs;
    });


    #endregion

    #region Private Connection Methods

    private async Task ConnectShardAsync(int i)
    {
        if (!this._shards.TryGetValue(i, out DiscordClient? client))
        {
            throw new Exception($"Could not initialize shard {i}.");
        }

        if (this.GatewayInfo != null)
        {
            client.GatewayInfo = this.GatewayInfo;
            client.GatewayUri = new Uri(client.GatewayInfo.Url);
        }

        if (this.CurrentUser != null)
        {
            client.CurrentUser = this.CurrentUser;
        }

        if (this.CurrentApplication != null)
        {
            client.CurrentApplication = this.CurrentApplication;
        }

        if (this._internalVoiceRegions != null)
        {
            client.InternalVoiceRegions = this._internalVoiceRegions;
            client._voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(client.InternalVoiceRegions));
        }

        this.HookEventHandlers(client);

        client._isShard = true;
        await client.ConnectAsync();
        this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booted shard {Shard}.", i);

        if (this.CurrentUser == null)
        {
            this.CurrentUser = client.CurrentUser;
        }

        if (this.CurrentApplication == null)
        {
            this.CurrentApplication = client.CurrentApplication;
        }

        if (this._internalVoiceRegions == null)
        {
            this._internalVoiceRegions = client.InternalVoiceRegions;
            this._voiceRegionsLazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this._internalVoiceRegions));
        }
    }

    private Task InternalStopAsync(bool enableLogger = true)
    {
        if (!this._isStarted)
        {
            throw new InvalidOperationException("This client has not been started.");
        }

        this._isStarted = false;

        if (enableLogger)
        {
            this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disposing {ShardCount} shards.", this._shards.Count);
        }

        this._voiceRegionsLazy = null;
        this.GatewayInfo = null;
        this.CurrentUser = null;
        this.CurrentApplication = null;

        for (int i = 0; i < this._shards.Count; i++)
        {
            if (this._shards.TryGetValue(i, out DiscordClient? client))
            {
                this.UnhookEventHandlers(client);

                client.Dispose();

                if (enableLogger)
                {
                    this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disconnected shard {Shard}.", i);
                }
            }
        }

        this._shards.Clear();

        return Task.CompletedTask;
    }

    #endregion

    #region Event Handler Initialization/Registering

    private void InternalSetup()
    {
        this._applicationCommandPermissionsUpdated = new AsyncEvent<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs>("APPLICATION_COMMAND_PERMISSIONS_UPDATED", this.EventErrorHandler);
        this._clientErrored = new AsyncEvent<DiscordClient, ClientErrorEventArgs>("CLIENT_ERRORED", this.Goof);
        this._heartbeated = new AsyncEvent<DiscordClient, HeartbeatEventArgs>("HEARTBEATED", this.EventErrorHandler);
        this._ready = new AsyncEvent<DiscordClient, SessionReadyEventArgs>("READY", this.EventErrorHandler);
        this._resumed = new AsyncEvent<DiscordClient, SessionReadyEventArgs>("RESUMED", this.EventErrorHandler);
        
        #region AutoModeration
        this._autoModerationRuleCreated = new AsyncEvent<DiscordClient, AutoModerationRuleCreateEventArgs>("AUTO_MODERATION_RULE_CREATED", this.EventErrorHandler);
        this._autoModerationRuleDeleted = new AsyncEvent<DiscordClient, AutoModerationRuleDeleteEventArgs>("AUTO_MODERATION_RULE_DELETED", this.EventErrorHandler);
        this._autoModerationRuleExecuted = new AsyncEvent<DiscordClient, AutoModerationRuleExecuteEventArgs>("AUTO_MODERATION_RULE_EXECUTED", this.EventErrorHandler);
        this._autoModerationRuleUpdated = new AsyncEvent<DiscordClient, AutoModerationRuleUpdateEventArgs>("AUTO_MODERATION_RULE_UPDATED", this.EventErrorHandler);
        #endregion

        #region Channel
        this._channelCreated = new AsyncEvent<DiscordClient, ChannelCreateEventArgs>("CHANNEL_CREATED", this.EventErrorHandler);
        this._channelDeleted = new AsyncEvent<DiscordClient, ChannelDeleteEventArgs>("CHANNEL_DELETED", this.EventErrorHandler);
        this._channelPinsUpdated = new AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs>("CHANNEL_PINS_UPDATED", this.EventErrorHandler);
        this._channelUpdated = new AsyncEvent<DiscordClient, ChannelUpdateEventArgs>("CHANNEL_UPDATED", this.EventErrorHandler);
        #endregion
        
        #region Guild
        this._guildAvailable = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_AVAILABLE", this.EventErrorHandler);
        this._guildBanAdded = new AsyncEvent<DiscordClient, GuildBanAddEventArgs>("GUILD_BAN_ADDED", this.EventErrorHandler);
        this._guildBanRemoved = new AsyncEvent<DiscordClient, GuildBanRemoveEventArgs>("GUILD_BAN_REMOVED", this.EventErrorHandler);
        this._guildCreated = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_CREATED", this.EventErrorHandler);
        this._guildDeleted = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_DELETED", this.EventErrorHandler);
        this._guildDownloadCompleted = new AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs>("GUILD_DOWNLOAD_COMPLETED", this.EventErrorHandler);
        this._guildEmojisUpdated = new AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs>("GUILD_EMOJI_UPDATED", this.EventErrorHandler);
        this._guildIntegrationsUpdated = new AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs>("GUILD_INTEGRATIONS_UPDATED", this.EventErrorHandler);
        this._guildMemberAdded = new AsyncEvent<DiscordClient, GuildMemberAddEventArgs>("GUILD_MEMBER_ADDED", this.EventErrorHandler);
        this._guildMemberRemoved = new AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs>("GUILD_MEMBER_REMOVED", this.EventErrorHandler);
        this._guildMembersChunk = new AsyncEvent<DiscordClient, GuildMembersChunkEventArgs>("GUILD_MEMBERS_CHUNKED", this.EventErrorHandler);
        this._guildMemberUpdated = new AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs>("GUILD_MEMBER_UPDATED", this.EventErrorHandler);
        this._guildRoleCreated = new AsyncEvent<DiscordClient, GuildRoleCreateEventArgs>("GUILD_ROLE_CREATED", this.EventErrorHandler);
        this._guildRoleDeleted = new AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs>("GUILD_ROLE_DELETED", this.EventErrorHandler);
        this._guildRoleUpdated = new AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs>("GUILD_ROLE_UPDATED", this.EventErrorHandler);
        this._guildStickersUpdated = new AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs>("GUILD_STICKER_UPDATED", this.EventErrorHandler);
        this._guildUnavailable = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_UNAVAILABLE", this.EventErrorHandler);
        this._guildUpdated = new AsyncEvent<DiscordClient, GuildUpdateEventArgs>("GUILD_UPDATED", this.EventErrorHandler);
        this._inviteCreated = new AsyncEvent<DiscordClient, InviteCreateEventArgs>("INVITE_CREATED", this.EventErrorHandler);
        this._inviteDeleted = new AsyncEvent<DiscordClient, InviteDeleteEventArgs>("INVITE_DELETED", this.EventErrorHandler);
        #endregion

        #region Messages
        this._messageBulkDeleted = new AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs>("MESSAGE_BULK_DELETED", this.EventErrorHandler);
        this._messageCreated = new AsyncEvent<DiscordClient, MessageCreateEventArgs>("MESSAGE_CREATED", this.EventErrorHandler);
        this._messageDeleted = new AsyncEvent<DiscordClient, MessageDeleteEventArgs>("MESSAGE_DELETED", this.EventErrorHandler);
        this._messageReactionAdded = new AsyncEvent<DiscordClient, MessageReactionAddEventArgs>("MESSAGE_REACTION_ADDED", this.EventErrorHandler);
        this._messageReactionRemoved = new AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>("MESSAGE_REACTION_REMOVED", this.EventErrorHandler);
        this._messageReactionRemovedEmoji = new AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs>("MESSAGE_REACTION_REMOVED_EMOJI", this.EventErrorHandler);
        this._messageReactionsCleared = new AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>("MESSAGE_REACTIONS_CLEARED", this.EventErrorHandler);
        this._messageUpdated = new AsyncEvent<DiscordClient, MessageUpdateEventArgs>("MESSAGE_UPDATED", this.EventErrorHandler);
        #endregion

        #region Integrations
        this._integrationCreated = new AsyncEvent<DiscordClient, IntegrationCreateEventArgs>("INTEGRATION_CREATED", this.EventErrorHandler);
        this._integrationDeleted = new AsyncEvent<DiscordClient, IntegrationDeleteEventArgs>("INTEGRATION_DELETED", this.EventErrorHandler);
        this._integrationUpdated = new AsyncEvent<DiscordClient, IntegrationUpdateEventArgs>("INTEGRATION_UPDATED", this.EventErrorHandler);
        this._interactionCreated = new AsyncEvent<DiscordClient, InteractionCreateEventArgs>("INTERACTION_CREATED", this.EventErrorHandler);
        #endregion
        
        
        this._componentInteractionCreated = new AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs>("COMPONENT_INTERACTED", this.EventErrorHandler);
        this._contextMenuInteractionCreated = new AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs>("CONTEXT_MENU_INTERACTED", this.EventErrorHandler);
        this._dmChannelDeleted = new AsyncEvent<DiscordClient, DmChannelDeleteEventArgs>("DM_CHANNEL_DELETED", this.EventErrorHandler);
        
        
        
        this._modalSubmitted = new AsyncEvent<DiscordClient, ModalSubmitEventArgs>("MODAL_SUBMITTED", this.EventErrorHandler);
        this._presenceUpdated = new AsyncEvent<DiscordClient, PresenceUpdateEventArgs>("PRESENCE_UPDATED", this.EventErrorHandler);
        this._scheduledGuildEventCompleted = new AsyncEvent<DiscordClient, ScheduledGuildEventCompletedEventArgs>("SCHEDULED_GUILD_EVENT_COMPLETED", this.EventErrorHandler);
        this._scheduledGuildEventCreated = new AsyncEvent<DiscordClient, ScheduledGuildEventCreateEventArgs>("SCHEDULED_GUILD_EVENT_CREATED", this.EventErrorHandler);
        this._scheduledGuildEventDeleted = new AsyncEvent<DiscordClient, ScheduledGuildEventDeleteEventArgs>("SCHEDULED_GUILD_EVENT_DELETED", this.EventErrorHandler);
        this._scheduledGuildEventUpdated = new AsyncEvent<DiscordClient, ScheduledGuildEventUpdateEventArgs>("SCHEDULED_GUILD_EVENT_UPDATED", this.EventErrorHandler);
        this._scheduledGuildEventUserAdded = new AsyncEvent<DiscordClient, ScheduledGuildEventUserAddEventArgs>("SCHEDULED_GUILD_EVENT_USER_ADDED", this.EventErrorHandler);
        this._scheduledGuildEventUserRemoved = new AsyncEvent<DiscordClient, ScheduledGuildEventUserRemoveEventArgs>("SCHEDULED_GUILD_EVENT_USER_REMOVED", this.EventErrorHandler);
        this._socketClosed = new AsyncEvent<DiscordClient, SocketCloseEventArgs>("SOCKET_CLOSED", this.EventErrorHandler);
        this._socketErrored = new AsyncEvent<DiscordClient, SocketErrorEventArgs>("SOCKET_ERRORED", this.Goof);
        this._socketOpened = new AsyncEvent<DiscordClient, SocketEventArgs>("SOCKET_OPENED", this.EventErrorHandler);
        this._stageInstanceCreated = new AsyncEvent<DiscordClient, StageInstanceCreateEventArgs>("STAGE_INSTANCE_CREATED", this.EventErrorHandler);
        this._stageInstanceDeleted = new AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs>("STAGE_INSTANCE_DELETED", this.EventErrorHandler);
        this._stageInstanceUpdated = new AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs>("STAGE_INSTANCE_UPDATED", this.EventErrorHandler);
        this._threadCreated = new AsyncEvent<DiscordClient, ThreadCreateEventArgs>("THREAD_CREATED", this.EventErrorHandler);
        this._threadDeleted = new AsyncEvent<DiscordClient, ThreadDeleteEventArgs>("THREAD_DELETED", this.EventErrorHandler);
        this._threadListSynced = new AsyncEvent<DiscordClient, ThreadListSyncEventArgs>("THREAD_LIST_SYNCED", this.EventErrorHandler);
        this._threadMembersUpdated = new AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs>("THREAD_MEMBERS_UPDATED", this.EventErrorHandler);
        this._threadMemberUpdated = new AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs>("THREAD_MEMBER_UPDATED", this.EventErrorHandler);
        this._threadUpdated = new AsyncEvent<DiscordClient, ThreadUpdateEventArgs>("THREAD_UPDATED", this.EventErrorHandler);
        this._typingStarted = new AsyncEvent<DiscordClient, TypingStartEventArgs>("TYPING_STARTED", this.EventErrorHandler);
        this._unknownEvent = new AsyncEvent<DiscordClient, UnknownEventArgs>("UNKNOWN_EVENT", this.EventErrorHandler);
        this._userSettingsUpdated = new AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs>("USER_SETTINGS_UPDATED", this.EventErrorHandler);
        this._userUpdated = new AsyncEvent<DiscordClient, UserUpdateEventArgs>("USER_UPDATED", this.EventErrorHandler);
        this._voiceServerUpdated = new AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs>("VOICE_SERVER_UPDATED", this.EventErrorHandler);
        this._voiceStateUpdated = new AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs>("VOICE_STATE_UPDATED", this.EventErrorHandler);
        this._webhooksUpdated = new AsyncEvent<DiscordClient, WebhooksUpdateEventArgs>("WEBHOOKS_UPDATED", this.EventErrorHandler);
        this._zombied = new AsyncEvent<DiscordClient, ZombiedEventArgs>("ZOMBIED", this.EventErrorHandler);
    }

    private void HookEventHandlers(DiscordClient client)
    {
        client.ApplicationCommandPermissionsUpdated += this.Client_ApplicationCommandPermissionsUpdated;
        client.AutoModerationRuleCreated += this.Client_AutoModerationRuleCreated;
        client.AutoModerationRuleDeleted += this.Client_AutoModerationRuleDeleted;
        client.AutoModerationRuleExecuted += this.Client_AutoModerationRuleExecuted;
        client.AutoModerationRuleUpdated += this.Client_AutoModerationRuleUpdated;
        client.ChannelCreated += this.Client_ChannelCreated;
        client.ChannelDeleted += this.Client_ChannelDeleted;
        client.ChannelPinsUpdated += this.Client_ChannelPinsUpdated;
        client.ChannelUpdated += this.Client_ChannelUpdated;
        client.ClientErrored += this.Client_ClientError;
        client.ComponentInteractionCreated += this.Client_ComponentInteractionCreate;
        client.ContextMenuInteractionCreated += this.Client_ContextMenuInteractionCreate;
        client.DmChannelDeleted += this.Client_DMChannelDeleted;
        client.GuildAvailable += this.Client_GuildAvailable;
        client.GuildBanAdded += this.Client_GuildBanAdd;
        client.GuildBanRemoved += this.Client_GuildBanRemove;
        client.GuildCreated += this.Client_GuildCreated;
        client.GuildDeleted += this.Client_GuildDeleted;
        client.GuildDownloadCompleted += this.Client_GuildDownloadCompleted;
        client.GuildEmojisUpdated += this.Client_GuildEmojisUpdate;
        client.GuildIntegrationsUpdated += this.Client_GuildIntegrationsUpdate;
        client.GuildMemberAdded += this.Client_GuildMemberAdd;
        client.GuildMemberRemoved += this.Client_GuildMemberRemove;
        client.GuildMembersChunked += this.Client_GuildMembersChunk;
        client.GuildMemberUpdated += this.Client_GuildMemberUpdate;
        client.GuildRoleCreated += this.Client_GuildRoleCreate;
        client.GuildRoleDeleted += this.Client_GuildRoleDelete;
        client.GuildRoleUpdated += this.Client_GuildRoleUpdate;
        client.GuildStickersUpdated += this.Client_GuildStickersUpdate;
        client.GuildUnavailable += this.Client_GuildUnavailable;
        client.GuildUpdated += this.Client_GuildUpdated;
        client.Heartbeated += this.Client_HeartBeated;
        client.IntegrationCreated += this.Client_IntegrationCreated;
        client.IntegrationDeleted += this.Client_IntegrationDeleted;
        client.IntegrationUpdated += this.Client_IntegrationUpdated;
        client.InteractionCreated += this.Client_InteractionCreate;
        client.InviteCreated += this.Client_InviteCreated;
        client.InviteDeleted += this.Client_InviteDeleted;
        client.MessageCreated += this.Client_MessageCreated;
        client.MessageDeleted += this.Client_MessageDelete;
        client.MessageReactionAdded += this.Client_MessageReactionAdd;
        client.MessageReactionRemoved += this.Client_MessageReactionRemove;
        client.MessageReactionRemovedEmoji += this.Client_MessageReactionRemovedEmoji;
        client.MessageReactionsCleared += this.Client_MessageReactionRemoveAll;
        client.MessagesBulkDeleted += this.Client_MessageBulkDelete;
        client.MessageUpdated += this.Client_MessageUpdate;
        client.ModalSubmitted += this.Client_ModalSubmitted;
        client.PresenceUpdated += this.Client_PresenceUpdate;
        client.ScheduledGuildEventCompleted += this.Client_ScheduledGuildEventCompleted;
        client.ScheduledGuildEventCreated += this.Client_ScheduledGuildEventCreated;
        client.ScheduledGuildEventDeleted += this.Client_ScheduledGuildEventDeleted;
        client.ScheduledGuildEventUpdated += this.Client_ScheduledGuildEventUpdated;
        client.ScheduledGuildEventUserAdded += this.Client_ScheduledGuildEventUserAdded;
        client.ScheduledGuildEventUserRemoved += this.Client_ScheduledGuildEventUserRemoved;
        client.SessionCreated += this.Client_Ready;
        client.SessionResumed += this.Client_Resumed;
        client.SocketClosed += this.Client_SocketClosed;
        client.SocketErrored += this.Client_SocketError;
        client.SocketOpened += this.Client_SocketOpened;
        client.StageInstanceCreated += this.Client_StageInstanceCreated;
        client.StageInstanceDeleted += this.Client_StageInstanceDeleted;
        client.StageInstanceUpdated += this.Client_StageInstanceUpdated;
        client.ThreadCreated += this.Client_ThreadCreated;
        client.ThreadDeleted += this.Client_ThreadDeleted;
        client.ThreadListSynced += this.Client_ThreadListSynced;
        client.ThreadMembersUpdated += this.Client_ThreadMembersUpdated;
        client.ThreadMemberUpdated += this.Client_ThreadMemberUpdated;
        client.ThreadUpdated += this.Client_ThreadUpdated;
        client.TypingStarted += this.Client_TypingStart;
        client.UnknownEvent += this.Client_UnknownEvent;
        client.UserSettingsUpdated += this.Client_UserSettingsUpdate;
        client.UserUpdated += this.Client_UserUpdate;
        client.VoiceServerUpdated += this.Client_VoiceServerUpdate;
        client.VoiceStateUpdated += this.Client_VoiceStateUpdate;
        client.WebhooksUpdated += this.Client_WebhooksUpdate;
        client.Zombied += this.Client_Zombied;
    }

    private void UnhookEventHandlers(DiscordClient client)
    {
        client.ApplicationCommandPermissionsUpdated -= this.Client_ApplicationCommandPermissionsUpdated;
        client.AutoModerationRuleCreated -= this.Client_AutoModerationRuleCreated;
        client.AutoModerationRuleDeleted -= this.Client_AutoModerationRuleDeleted;
        client.AutoModerationRuleExecuted -= this.Client_AutoModerationRuleExecuted;
        client.AutoModerationRuleUpdated -= this.Client_AutoModerationRuleUpdated;
        client.ChannelCreated -= this.Client_ChannelCreated;
        client.ChannelDeleted -= this.Client_ChannelDeleted;
        client.ChannelPinsUpdated -= this.Client_ChannelPinsUpdated;
        client.ChannelUpdated -= this.Client_ChannelUpdated;
        client.ClientErrored -= this.Client_ClientError;
        client.ComponentInteractionCreated -= this.Client_ComponentInteractionCreate;
        client.ContextMenuInteractionCreated -= this.Client_ContextMenuInteractionCreate;
        client.DmChannelDeleted -= this.Client_DMChannelDeleted;
        client.GuildAvailable -= this.Client_GuildAvailable;
        client.GuildBanAdded -= this.Client_GuildBanAdd;
        client.GuildBanRemoved -= this.Client_GuildBanRemove;
        client.GuildCreated -= this.Client_GuildCreated;
        client.GuildDeleted -= this.Client_GuildDeleted;
        client.GuildDownloadCompleted -= this.Client_GuildDownloadCompleted;
        client.GuildEmojisUpdated -= this.Client_GuildEmojisUpdate;
        client.GuildIntegrationsUpdated -= this.Client_GuildIntegrationsUpdate;
        client.GuildMemberAdded -= this.Client_GuildMemberAdd;
        client.GuildMemberRemoved -= this.Client_GuildMemberRemove;
        client.GuildMembersChunked -= this.Client_GuildMembersChunk;
        client.GuildMemberUpdated -= this.Client_GuildMemberUpdate;
        client.GuildRoleCreated -= this.Client_GuildRoleCreate;
        client.GuildRoleDeleted -= this.Client_GuildRoleDelete;
        client.GuildRoleUpdated -= this.Client_GuildRoleUpdate;
        client.GuildStickersUpdated -= this.Client_GuildStickersUpdate;
        client.GuildUnavailable -= this.Client_GuildUnavailable;
        client.GuildUpdated -= this.Client_GuildUpdated;
        client.Heartbeated -= this.Client_HeartBeated;
        client.IntegrationCreated -= this.Client_IntegrationCreated;
        client.IntegrationDeleted -= this.Client_IntegrationDeleted;
        client.IntegrationUpdated -= this.Client_IntegrationUpdated;
        client.InteractionCreated -= this.Client_InteractionCreate;
        client.InviteCreated -= this.Client_InviteCreated;
        client.InviteDeleted -= this.Client_InviteDeleted;
        client.MessageCreated -= this.Client_MessageCreated;
        client.MessageDeleted -= this.Client_MessageDelete;
        client.MessageReactionAdded -= this.Client_MessageReactionAdd;
        client.MessageReactionRemoved -= this.Client_MessageReactionRemove;
        client.MessageReactionRemovedEmoji -= this.Client_MessageReactionRemovedEmoji;
        client.MessageReactionsCleared -= this.Client_MessageReactionRemoveAll;
        client.MessagesBulkDeleted -= this.Client_MessageBulkDelete;
        client.MessageUpdated -= this.Client_MessageUpdate;
        client.ModalSubmitted -= this.Client_ModalSubmitted;
        client.PresenceUpdated -= this.Client_PresenceUpdate;
        client.ScheduledGuildEventCompleted -= this.Client_ScheduledGuildEventCompleted;
        client.ScheduledGuildEventCreated -= this.Client_ScheduledGuildEventCreated;
        client.ScheduledGuildEventDeleted -= this.Client_ScheduledGuildEventDeleted;
        client.ScheduledGuildEventUpdated -= this.Client_ScheduledGuildEventUpdated;
        client.ScheduledGuildEventUserAdded -= this.Client_ScheduledGuildEventUserAdded;
        client.ScheduledGuildEventUserRemoved -= this.Client_ScheduledGuildEventUserRemoved;
        client.SessionCreated -= this.Client_Ready;
        client.SessionResumed -= this.Client_Resumed;
        client.SocketClosed -= this.Client_SocketClosed;
        client.SocketErrored -= this.Client_SocketError;
        client.SocketOpened -= this.Client_SocketOpened;
        client.StageInstanceCreated -= this.Client_StageInstanceCreated;
        client.StageInstanceDeleted -= this.Client_StageInstanceDeleted;
        client.StageInstanceUpdated -= this.Client_StageInstanceUpdated;
        client.ThreadCreated -= this.Client_ThreadCreated;
        client.ThreadDeleted -= this.Client_ThreadDeleted;
        client.ThreadListSynced -= this.Client_ThreadListSynced;
        client.ThreadMembersUpdated -= this.Client_ThreadMembersUpdated;
        client.ThreadMemberUpdated -= this.Client_ThreadMemberUpdated;
        client.ThreadUpdated -= this.Client_ThreadUpdated;
        client.TypingStarted -= this.Client_TypingStart;
        client.UnknownEvent -= this.Client_UnknownEvent;
        client.UserSettingsUpdated -= this.Client_UserSettingsUpdate;
        client.UserUpdated -= this.Client_UserUpdate;
        client.VoiceServerUpdated -= this.Client_VoiceServerUpdate;
        client.VoiceStateUpdated -= this.Client_VoiceStateUpdate;
        client.WebhooksUpdated -= this.Client_WebhooksUpdate;
        client.Zombied -= this.Client_Zombied;
    }

    private int GetShardIdFromGuilds(ulong id)
    {
        foreach (DiscordClient s in this._shards.Values)
        {
            if (s._guilds.TryGetValue(id, out _))
            {
                return s.ShardId;
            }
        }

        return -1;
    }

    #endregion

    #region Destructor

    ~DiscordShardedClient()
    {
        if (this._isStarted)
        {
            this.InternalStopAsync(false).GetAwaiter().GetResult();
        }
    }

    #endregion
}
