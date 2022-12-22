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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Net.Serialization;
using Emzi0767.Utilities;
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
        => _voiceRegionsLazy?.Value;

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
        InternalSetup();

        if (config.ShardCount > 1)
        {
            _manuallySharding = true;
        }

        Configuration = config;
        ShardClients = new ReadOnlyConcurrentDictionary<int, DiscordClient>(_shards);

        if (Configuration.LoggerFactory == null)
        {
            Configuration.LoggerFactory = new DefaultLoggerFactory();
            Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(Configuration.MinimumLogLevel, Configuration.LogTimestampFormat));
        }
        Logger = Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();
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
        if (_isStarted)
        {
            throw new InvalidOperationException("This client has already been started.");
        }

        _isStarted = true;

        try
        {
            if (Configuration.TokenType != TokenType.Bot)
            {
                Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
            }

            Logger.LogInformation(LoggerEvents.Startup, "DSharpPlus, version {Version}", _versionString.Value);

            int shardc = await InitializeShardsAsync().ConfigureAwait(false);
            List<Task> connectTasks = new List<Task>();
            Logger.LogInformation(LoggerEvents.ShardStartup, "Booting {ShardCount} shards.", shardc);

            for (int i = 0; i < shardc; i++)
            {
                //This should never happen, but in case it does...
                if (GatewayInfo.SessionBucket.MaxConcurrency < 1)
                {
                    GatewayInfo.SessionBucket.MaxConcurrency = 1;
                }

                if (GatewayInfo.SessionBucket.MaxConcurrency == 1)
                {
                    await ConnectShardAsync(i).ConfigureAwait(false);
                }
                else
                {
                    //Concurrent login.
                    connectTasks.Add(ConnectShardAsync(i));

                    if (connectTasks.Count == GatewayInfo.SessionBucket.MaxConcurrency)
                    {
                        await Task.WhenAll(connectTasks).ConfigureAwait(false);
                        connectTasks.Clear();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await InternalStopAsync(false).ConfigureAwait(false);

            string message = $"Shard initialization failed, check inner exceptions for details: ";

            Logger.LogCritical(LoggerEvents.ShardClientError, $"{message}\n{ex}");
            throw new AggregateException(message, ex);
        }
    }
    /// <summary>
    /// Disconnects and disposes of all shards.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task StopAsync()
        => InternalStopAsync();

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
        int index = _manuallySharding ? GetShardIdFromGuilds(guildId) : Utilities.GetShardId(guildId, ShardClients.Count);

        return index != -1 ? _shards[index] : null;
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
        => GetShard(guild.Id);

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
        foreach (DiscordClient client in _shards.Values)
        {
            tasks.Add(client.UpdateStatusAsync(activity, userStatus, idleSince));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    #endregion

    #region Internal Methods

    public async Task<int> InitializeShardsAsync()
    {
        if (_shards.Count != 0)
        {
            return _shards.Count;
        }

        GatewayInfo = await GetGatewayInfoAsync().ConfigureAwait(false);
        int shardc = Configuration.ShardCount == 1 ? GatewayInfo.ShardCount : Configuration.ShardCount;
        ShardedLoggerFactory lf = new ShardedLoggerFactory(Logger);
        for (int i = 0; i < shardc; i++)
        {
            DiscordConfiguration cfg = new DiscordConfiguration(Configuration)
            {
                ShardId = i,
                ShardCount = shardc,
                LoggerFactory = lf
            };

            DiscordClient client = new DiscordClient(cfg);
            if (!_shards.TryAdd(i, client))
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
        http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(Configuration));

        Logger.LogDebug(LoggerEvents.ShardRest, $"Obtaining gateway information from GET {Endpoints.GATEWAY}{Endpoints.BOT}...");
        HttpResponseMessage resp = await http.GetAsync(url).ConfigureAwait(false);

        http.Dispose();

        if (!resp.IsSuccessStatusCode)
        {
            bool ratelimited = await HandleHttpError(url, resp).ConfigureAwait(false);

            if (ratelimited)
            {
                return await GetGatewayInfoAsync().ConfigureAwait(false);
            }
        }

        Stopwatch timer = new Stopwatch();
        timer.Start();

        JObject jo = JObject.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
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
                Logger.LogError(LoggerEvents.ShardClientError, $"Ratelimit hit, requeuing request to {reqUrl}");

                Dictionary<string, string> hs = msg.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
                int waitInterval = 0;

                if (hs.TryGetValue("Retry-After", out string? retry_after_raw))
                {
                    waitInterval = int.Parse(retry_after_raw, CultureInfo.InvariantCulture);
                }

                await Task.Delay(waitInterval).ConfigureAwait(false);
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
        if (!_shards.TryGetValue(i, out DiscordClient? client))
        {
            throw new Exception($"Could not initialize shard {i}.");
        }

        if (GatewayInfo != null)
        {
            client.GatewayInfo = GatewayInfo;
            client.GatewayUri = new Uri(client.GatewayInfo.Url);
        }

        if (CurrentUser != null)
        {
            client.CurrentUser = CurrentUser;
        }

        if (CurrentApplication != null)
        {
            client.CurrentApplication = CurrentApplication;
        }

        if (_internalVoiceRegions != null)
        {
            client.InternalVoiceRegions = _internalVoiceRegions;
            client._voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(client.InternalVoiceRegions));
        }

        HookEventHandlers(client);

        client._isShard = true;
        await client.ConnectAsync().ConfigureAwait(false);
        Logger.LogInformation(LoggerEvents.ShardStartup, "Booted shard {Shard}.", i);

        if (CurrentUser == null)
        {
            CurrentUser = client.CurrentUser;
        }

        if (CurrentApplication == null)
        {
            CurrentApplication = client.CurrentApplication;
        }

        if (_internalVoiceRegions == null)
        {
            _internalVoiceRegions = client.InternalVoiceRegions;
            _voiceRegionsLazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(_internalVoiceRegions));
        }
    }

    private Task InternalStopAsync(bool enableLogger = true)
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException("This client has not been started.");
        }

        if (enableLogger)
        {
            Logger.LogInformation(LoggerEvents.ShardShutdown, "Disposing {ShardCount} shards.", _shards.Count);
        }

        _isStarted = false;
        _voiceRegionsLazy = null;

        GatewayInfo = null;
        CurrentUser = null;
        CurrentApplication = null;

        for (int i = 0; i < _shards.Count; i++)
        {
            if (_shards.TryGetValue(i, out DiscordClient? client))
            {
                UnhookEventHandlers(client);

                client.Dispose();

                if (enableLogger)
                {
                    Logger.LogInformation(LoggerEvents.ShardShutdown, "Disconnected shard {Shard}.", i);
                }
            }
        }

        _shards.Clear();

        return Task.CompletedTask;
    }

    #endregion

    #region Event Handler Initialization/Registering

    private void InternalSetup()
    {
        _clientErrored = new AsyncEvent<DiscordClient, ClientErrorEventArgs>("CLIENT_ERRORED", DiscordClient.EventExecutionLimit, Goof);
        _socketErrored = new AsyncEvent<DiscordClient, SocketErrorEventArgs>("SOCKET_ERRORED", DiscordClient.EventExecutionLimit, Goof);
        _socketOpened = new AsyncEvent<DiscordClient, SocketEventArgs>("SOCKET_OPENED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _socketClosed = new AsyncEvent<DiscordClient, SocketCloseEventArgs>("SOCKET_CLOSED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _ready = new AsyncEvent<DiscordClient, ReadyEventArgs>("READY", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _resumed = new AsyncEvent<DiscordClient, ReadyEventArgs>("RESUMED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _channelCreated = new AsyncEvent<DiscordClient, ChannelCreateEventArgs>("CHANNEL_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _channelUpdated = new AsyncEvent<DiscordClient, ChannelUpdateEventArgs>("CHANNEL_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _channelDeleted = new AsyncEvent<DiscordClient, ChannelDeleteEventArgs>("CHANNEL_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _dmChannelDeleted = new AsyncEvent<DiscordClient, DmChannelDeleteEventArgs>("DM_CHANNEL_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _channelPinsUpdated = new AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs>("CHANNEL_PINS_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildCreated = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildAvailable = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_AVAILABLE", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildUpdated = new AsyncEvent<DiscordClient, GuildUpdateEventArgs>("GUILD_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildDeleted = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildUnavailable = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_UNAVAILABLE", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildDownloadCompleted = new AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs>("GUILD_DOWNLOAD_COMPLETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _inviteCreated = new AsyncEvent<DiscordClient, InviteCreateEventArgs>("INVITE_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _inviteDeleted = new AsyncEvent<DiscordClient, InviteDeleteEventArgs>("INVITE_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _messageCreated = new AsyncEvent<DiscordClient, MessageCreateEventArgs>("MESSAGE_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _presenceUpdated = new AsyncEvent<DiscordClient, PresenceUpdateEventArgs>("PRESENCE_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildBanAdded = new AsyncEvent<DiscordClient, GuildBanAddEventArgs>("GUILD_BAN_ADDED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildBanRemoved = new AsyncEvent<DiscordClient, GuildBanRemoveEventArgs>("GUILD_BAN_REMOVED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildEmojisUpdated = new AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs>("GUILD_EMOJI_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildStickersUpdated = new AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs>("GUILD_STICKER_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildIntegrationsUpdated = new AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs>("GUILD_INTEGRATIONS_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildMemberAdded = new AsyncEvent<DiscordClient, GuildMemberAddEventArgs>("GUILD_MEMBER_ADDED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildMemberRemoved = new AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs>("GUILD_MEMBER_REMOVED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildMemberUpdated = new AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs>("GUILD_MEMBER_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildRoleCreated = new AsyncEvent<DiscordClient, GuildRoleCreateEventArgs>("GUILD_ROLE_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildRoleUpdated = new AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs>("GUILD_ROLE_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildRoleDeleted = new AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs>("GUILD_ROLE_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _messageUpdated = new AsyncEvent<DiscordClient, MessageUpdateEventArgs>("MESSAGE_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _messageDeleted = new AsyncEvent<DiscordClient, MessageDeleteEventArgs>("MESSAGE_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _messageBulkDeleted = new AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs>("MESSAGE_BULK_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _interactionCreated = new AsyncEvent<DiscordClient, InteractionCreateEventArgs>("INTERACTION_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _componentInteractionCreated = new AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs>("COMPONENT_INTERACTED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _modalSubmitted = new AsyncEvent<DiscordClient, ModalSubmitEventArgs>("MODAL_SUBMITTED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _contextMenuInteractionCreated = new AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs>("CONTEXT_MENU_INTERACTED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _typingStarted = new AsyncEvent<DiscordClient, TypingStartEventArgs>("TYPING_STARTED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _userSettingsUpdated = new AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs>("USER_SETTINGS_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _userUpdated = new AsyncEvent<DiscordClient, UserUpdateEventArgs>("USER_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _voiceStateUpdated = new AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs>("VOICE_STATE_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _voiceServerUpdated = new AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs>("VOICE_SERVER_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _guildMembersChunk = new AsyncEvent<DiscordClient, GuildMembersChunkEventArgs>("GUILD_MEMBERS_CHUNKED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _unknownEvent = new AsyncEvent<DiscordClient, UnknownEventArgs>("UNKNOWN_EVENT", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _messageReactionAdded = new AsyncEvent<DiscordClient, MessageReactionAddEventArgs>("MESSAGE_REACTION_ADDED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _messageReactionRemoved = new AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>("MESSAGE_REACTION_REMOVED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _messageReactionsCleared = new AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>("MESSAGE_REACTIONS_CLEARED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _messageReactionRemovedEmoji = new AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs>("MESSAGE_REACTION_REMOVED_EMOJI", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _webhooksUpdated = new AsyncEvent<DiscordClient, WebhooksUpdateEventArgs>("WEBHOOKS_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _heartbeated = new AsyncEvent<DiscordClient, HeartbeatEventArgs>("HEARTBEATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _zombied = new AsyncEvent<DiscordClient, ZombiedEventArgs>("ZOMBIED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _applicationCommandCreated = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _applicationCommandUpdated = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _applicationCommandDeleted = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _integrationCreated = new AsyncEvent<DiscordClient, IntegrationCreateEventArgs>("INTEGRATION_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _integrationUpdated = new AsyncEvent<DiscordClient, IntegrationUpdateEventArgs>("INTEGRATION_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _integrationDeleted = new AsyncEvent<DiscordClient, IntegrationDeleteEventArgs>("INTEGRATION_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _stageInstanceCreated = new AsyncEvent<DiscordClient, StageInstanceCreateEventArgs>("STAGE_INSTANCE_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _stageInstanceUpdated = new AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs>("STAGE_INSTANCE_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _stageInstanceDeleted = new AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs>("STAGE_INSTANCE_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);

        #region ThreadEvents
        _threadCreated = new AsyncEvent<DiscordClient, ThreadCreateEventArgs>("THREAD_CREATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _threadUpdated = new AsyncEvent<DiscordClient, ThreadUpdateEventArgs>("THREAD_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _threadDeleted = new AsyncEvent<DiscordClient, ThreadDeleteEventArgs>("THREAD_DELETED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _threadListSynced = new AsyncEvent<DiscordClient, ThreadListSyncEventArgs>("THREAD_LIST_SYNCED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _threadMemberUpdated = new AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs>("THREAD_MEMBER_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        _threadMembersUpdated = new AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs>("THREAD_MEMBERS_UPDATED", DiscordClient.EventExecutionLimit, EventErrorHandler);
        #endregion
    }

    private void HookEventHandlers(DiscordClient client)
    {
        client.ClientErrored += Client_ClientError;
        client.SocketErrored += Client_SocketError;
        client.SocketOpened += Client_SocketOpened;
        client.SocketClosed += Client_SocketClosed;
        client.Ready += Client_Ready;
        client.Resumed += Client_Resumed;
        client.ChannelCreated += Client_ChannelCreated;
        client.ChannelUpdated += Client_ChannelUpdated;
        client.ChannelDeleted += Client_ChannelDeleted;
        client.DmChannelDeleted += Client_DMChannelDeleted;
        client.ChannelPinsUpdated += Client_ChannelPinsUpdated;
        client.GuildCreated += Client_GuildCreated;
        client.GuildAvailable += Client_GuildAvailable;
        client.GuildUpdated += Client_GuildUpdated;
        client.GuildDeleted += Client_GuildDeleted;
        client.GuildUnavailable += Client_GuildUnavailable;
        client.GuildDownloadCompleted += Client_GuildDownloadCompleted;
        client.InviteCreated += Client_InviteCreated;
        client.InviteDeleted += Client_InviteDeleted;
        client.MessageCreated += Client_MessageCreated;
        client.PresenceUpdated += Client_PresenceUpdate;
        client.GuildBanAdded += Client_GuildBanAdd;
        client.GuildBanRemoved += Client_GuildBanRemove;
        client.GuildEmojisUpdated += Client_GuildEmojisUpdate;
        client.GuildStickersUpdated += Client_GuildStickersUpdate;
        client.GuildIntegrationsUpdated += Client_GuildIntegrationsUpdate;
        client.GuildMemberAdded += Client_GuildMemberAdd;
        client.GuildMemberRemoved += Client_GuildMemberRemove;
        client.GuildMemberUpdated += Client_GuildMemberUpdate;
        client.GuildRoleCreated += Client_GuildRoleCreate;
        client.GuildRoleUpdated += Client_GuildRoleUpdate;
        client.GuildRoleDeleted += Client_GuildRoleDelete;
        client.MessageUpdated += Client_MessageUpdate;
        client.MessageDeleted += Client_MessageDelete;
        client.MessagesBulkDeleted += Client_MessageBulkDelete;
        client.InteractionCreated += Client_InteractionCreate;
        client.ComponentInteractionCreated += Client_ComponentInteractionCreate;
        client.ModalSubmitted += Client_ModalSubmitted;
        client.ContextMenuInteractionCreated += Client_ContextMenuInteractionCreate;
        client.TypingStarted += Client_TypingStart;
        client.UserSettingsUpdated += Client_UserSettingsUpdate;
        client.UserUpdated += Client_UserUpdate;
        client.VoiceStateUpdated += Client_VoiceStateUpdate;
        client.VoiceServerUpdated += Client_VoiceServerUpdate;
        client.GuildMembersChunked += Client_GuildMembersChunk;
        client.UnknownEvent += Client_UnknownEvent;
        client.MessageReactionAdded += Client_MessageReactionAdd;
        client.MessageReactionRemoved += Client_MessageReactionRemove;
        client.MessageReactionsCleared += Client_MessageReactionRemoveAll;
        client.MessageReactionRemovedEmoji += Client_MessageReactionRemovedEmoji;
        client.WebhooksUpdated += Client_WebhooksUpdate;
        client.Heartbeated += Client_HeartBeated;
        client.Zombied += Client_Zombied;
        client.IntegrationCreated += Client_IntegrationCreated;
        client.IntegrationUpdated += Client_IntegrationUpdated;
        client.IntegrationDeleted += Client_IntegrationDeleted;
        client.StageInstanceCreated += Client_StageInstanceCreated;
        client.StageInstanceUpdated += Client_StageInstanceUpdated;
        client.StageInstanceDeleted += Client_StageInstanceDeleted;

        #region ThreadEvents
        client.ThreadCreated += Client_ThreadCreated;
        client.ThreadUpdated += Client_ThreadUpdated;
        client.ThreadDeleted += Client_ThreadDeleted;
        client.ThreadListSynced += Client_ThreadListSynced;
        client.ThreadMemberUpdated += Client_ThreadMemberUpdated;
        client.ThreadMembersUpdated += Client_ThreadMembersUpdated;
        #endregion
    }

    private void UnhookEventHandlers(DiscordClient client)
    {
        client.ClientErrored -= Client_ClientError;
        client.SocketErrored -= Client_SocketError;
        client.SocketOpened -= Client_SocketOpened;
        client.SocketClosed -= Client_SocketClosed;
        client.Ready -= Client_Ready;
        client.Resumed -= Client_Resumed;
        client.ChannelCreated -= Client_ChannelCreated;
        client.ChannelUpdated -= Client_ChannelUpdated;
        client.ChannelDeleted -= Client_ChannelDeleted;
        client.DmChannelDeleted -= Client_DMChannelDeleted;
        client.ChannelPinsUpdated -= Client_ChannelPinsUpdated;
        client.GuildCreated -= Client_GuildCreated;
        client.GuildAvailable -= Client_GuildAvailable;
        client.GuildUpdated -= Client_GuildUpdated;
        client.GuildDeleted -= Client_GuildDeleted;
        client.GuildUnavailable -= Client_GuildUnavailable;
        client.GuildDownloadCompleted -= Client_GuildDownloadCompleted;
        client.InviteCreated -= Client_InviteCreated;
        client.InviteDeleted -= Client_InviteDeleted;
        client.MessageCreated -= Client_MessageCreated;
        client.PresenceUpdated -= Client_PresenceUpdate;
        client.GuildBanAdded -= Client_GuildBanAdd;
        client.GuildBanRemoved -= Client_GuildBanRemove;
        client.GuildEmojisUpdated -= Client_GuildEmojisUpdate;
        client.GuildStickersUpdated -= Client_GuildStickersUpdate;
        client.GuildIntegrationsUpdated -= Client_GuildIntegrationsUpdate;
        client.GuildMemberAdded -= Client_GuildMemberAdd;
        client.GuildMemberRemoved -= Client_GuildMemberRemove;
        client.GuildMemberUpdated -= Client_GuildMemberUpdate;
        client.GuildRoleCreated -= Client_GuildRoleCreate;
        client.GuildRoleUpdated -= Client_GuildRoleUpdate;
        client.GuildRoleDeleted -= Client_GuildRoleDelete;
        client.MessageUpdated -= Client_MessageUpdate;
        client.MessageDeleted -= Client_MessageDelete;
        client.MessagesBulkDeleted -= Client_MessageBulkDelete;
        client.InteractionCreated -= Client_InteractionCreate;
        client.ComponentInteractionCreated -= Client_ComponentInteractionCreate;
        client.ModalSubmitted -= Client_ModalSubmitted;
        client.ContextMenuInteractionCreated -= Client_ContextMenuInteractionCreate;
        client.TypingStarted -= Client_TypingStart;
        client.UserSettingsUpdated -= Client_UserSettingsUpdate;
        client.UserUpdated -= Client_UserUpdate;
        client.VoiceStateUpdated -= Client_VoiceStateUpdate;
        client.VoiceServerUpdated -= Client_VoiceServerUpdate;
        client.GuildMembersChunked -= Client_GuildMembersChunk;
        client.UnknownEvent -= Client_UnknownEvent;
        client.MessageReactionAdded -= Client_MessageReactionAdd;
        client.MessageReactionRemoved -= Client_MessageReactionRemove;
        client.MessageReactionsCleared -= Client_MessageReactionRemoveAll;
        client.MessageReactionRemovedEmoji -= Client_MessageReactionRemovedEmoji;
        client.WebhooksUpdated -= Client_WebhooksUpdate;
        client.Heartbeated -= Client_HeartBeated;
        client.Zombied -= Client_Zombied;
        client.IntegrationCreated -= Client_IntegrationCreated;
        client.IntegrationUpdated -= Client_IntegrationUpdated;
        client.IntegrationDeleted -= Client_IntegrationDeleted;
        client.StageInstanceCreated -= Client_StageInstanceCreated;
        client.StageInstanceUpdated -= Client_StageInstanceUpdated;
        client.StageInstanceDeleted -= Client_StageInstanceDeleted;

        #region ThreadEvents
        client.ThreadCreated -= Client_ThreadCreated;
        client.ThreadUpdated -= Client_ThreadUpdated;
        client.ThreadDeleted -= Client_ThreadDeleted;
        client.ThreadListSynced -= Client_ThreadListSynced;
        client.ThreadMemberUpdated -= Client_ThreadMemberUpdated;
        client.ThreadMembersUpdated -= Client_ThreadMembersUpdated;
        #endregion
    }

    private int GetShardIdFromGuilds(ulong id)
    {
        foreach (DiscordClient s in _shards.Values)
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
        => InternalStopAsync(false).GetAwaiter().GetResult();

    #endregion
}
