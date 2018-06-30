#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    /// <summary>
    /// A Discord api wrapper
    /// </summary>
    public class DiscordClient : BaseDiscordClient, INotifyPropertyChanged
    {
        #region Internal Variables
        internal static UTF8Encoding UTF8 = new UTF8Encoding(false);
        internal static DateTimeOffset DiscordEpoch = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

        internal CancellationTokenSource _cancelTokenSource;
        internal CancellationToken _cancelToken;

        internal List<BaseExtension> _extensions = new List<BaseExtension>();

        internal BaseWebSocketClient _webSocketClient;
        internal string _sessionToken = "";
        internal string _sessionId = "";
        internal int _heartbeatInterval;
        internal Task _heartbeatTask;
        internal DateTimeOffset _lastHeartbeat;
        internal long _lastSequence;
        internal int _skippedHeartbeats = 0;
        internal bool _guildDownloadCompleted = false;

        public int HeartbeatInterval => _heartbeatInterval;
        public BaseWebSocketClient SocketClient => _webSocketClient;

        internal ConcurrentDictionary<ulong, DiscordReadState> ReadStates { get; } = new ConcurrentDictionary<ulong, DiscordReadState>();
        internal RingBuffer<DiscordMessage> MessageCache { get; }
        #endregion

        #region Public Variables
        /// <summary>
        /// Gets the gateway protocol version.
        /// </summary>
        public int GatewayVersion
            => _gatewayVersion;

        internal int _gatewayVersion;

        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        public Uri GatewayUri
            => _gatewayUri;

        internal Uri _gatewayUri;

        /// <summary>
        /// Gets the total number of shards the bot is connected to.
        /// </summary>
        public int ShardCount
            => Configuration.ShardCount;

        internal int _shardCount = 1;

        /// <summary>
        /// Gets the currently connected shard ID.
        /// </summary>
        public int ShardId
            => Configuration.ShardId;

        /// <summary>
        /// The current user's settings according to Discord.
        /// </summary>
        public DiscordUserSettings UserSettings { get => _userSettings; internal set => OnPropertySet(ref _userSettings, value); }

        /// <summary>
        /// List of DM Channels
        /// </summary>
        public IReadOnlyList<DiscordDmChannel> PrivateChannels
            => _privateChannelsLazy.Value;

        internal List<DiscordDmChannel> _privateChannels = new List<DiscordDmChannel>();
        private Lazy<IReadOnlyList<DiscordDmChannel>> _privateChannelsLazy;

        /// <summary>
        /// List of Guilds
        /// </summary>
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds
            => _guildsLazy.Value;

        internal Dictionary<ulong, DiscordGuild> _guilds = new Dictionary<ulong, DiscordGuild>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordGuild>> _guildsLazy;

        /// <summary>
        /// Gets the WS latency for this client.
        /// </summary>
        public int Ping
            => Volatile.Read(ref _ping);

        private int _ping;

        /// <summary>
        /// Gets the collection of presences held by this client.
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordPresence> Presences
            => _presencesLazy.Value;

        internal Dictionary<ulong, DiscordPresence> _presences = new Dictionary<ulong, DiscordPresence>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> _presencesLazy;

        private List<DiscordRelationship> _relationships;
        public IReadOnlyList<DiscordRelationship> Relationships => new ReadOnlyCollection<DiscordRelationship>(_relationships);
        #endregion

        #region Connection semaphore
        private static SemaphoreSlim ConnectionSemaphore
            => _semaphoreInit.Value;

        private static Lazy<SemaphoreSlim> _semaphoreInit = new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1));
        #endregion

        /// <summary>
        /// Initializes a new instance of DiscordClient.
        /// </summary>
        /// <param name="config">Specifies configuration parameters.</param>
        public DiscordClient(DiscordConfiguration config)
            : base(config)
        {
            if (Configuration.MessageCacheSize > 0)
            {
                MessageCache = new RingBuffer<DiscordMessage>(Configuration.MessageCacheSize);
            }

            InternalSetup();
        }

        internal void InternalSetup()
        {
            _clientErrored = new AsyncEvent<ClientErrorEventArgs>(Goof, "CLIENT_ERRORED");
            _socketErrored = new AsyncEvent<SocketErrorEventArgs>(Goof, "SOCKET_ERRORED");
            _socketOpened = new AsyncEvent(EventErrorHandler, "SOCKET_OPENED");
            _socketClosed = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "SOCKET_CLOSED");
            _ready = new AsyncEvent<ReadyEventArgs>(EventErrorHandler, "READY");
            _resumed = new AsyncEvent<ReadyEventArgs>(EventErrorHandler, "RESUMED");
            _channelCreated = new AsyncEvent<ChannelCreateEventArgs>(EventErrorHandler, "CHANNEL_CREATED");
            _dmChannelCreated = new AsyncEvent<DmChannelCreateEventArgs>(EventErrorHandler, "DM_CHANNEL_CREATED");
            _channelUpdated = new AsyncEvent<ChannelUpdateEventArgs>(EventErrorHandler, "CHANNEL_UPDATED");
            _channelDeleted = new AsyncEvent<ChannelDeleteEventArgs>(EventErrorHandler, "CHANNEL_DELETED");
            _dmChannelDeleted = new AsyncEvent<DmChannelDeleteEventArgs>(EventErrorHandler, "DM_CHANNEL_DELETED");
            _channelPinsUpdated = new AsyncEvent<ChannelPinsUpdateEventArgs>(EventErrorHandler, "CHANNEL_PINS_UPDATEED");
            _guildCreated = new AsyncEvent<GuildCreateEventArgs>(EventErrorHandler, "GUILD_CREATED");
            _guildAvailable = new AsyncEvent<GuildCreateEventArgs>(EventErrorHandler, "GUILD_AVAILABLE");
            _guildUpdated = new AsyncEvent<GuildUpdateEventArgs>(EventErrorHandler, "GUILD_UPDATED");
            _guildDeleted = new AsyncEvent<GuildDeleteEventArgs>(EventErrorHandler, "GUILD_DELETED");
            _guildUnavailable = new AsyncEvent<GuildDeleteEventArgs>(EventErrorHandler, "GUILD_UNAVAILABLE");
            _guildDownloadCompletedEv = new AsyncEvent<GuildDownloadCompletedEventArgs>(EventErrorHandler, "GUILD_DOWNLOAD_COMPLETED");
            _messageCreated = new AsyncEvent<MessageCreateEventArgs>(EventErrorHandler, "MESSAGE_CREATED");
            _presenceUpdated = new AsyncEvent<PresenceUpdateEventArgs>(EventErrorHandler, "PRESENCE_UPDATEED");
            _guildBanAdded = new AsyncEvent<GuildBanAddEventArgs>(EventErrorHandler, "GUILD_BAN_ADD");
            _guildBanRemoved = new AsyncEvent<GuildBanRemoveEventArgs>(EventErrorHandler, "GUILD_BAN_REMOVED");
            _guildEmojisUpdated = new AsyncEvent<GuildEmojisUpdateEventArgs>(EventErrorHandler, "GUILD_EMOJI_UPDATED");
            _guildIntegrationsUpdated = new AsyncEvent<GuildIntegrationsUpdateEventArgs>(EventErrorHandler, "GUILD_INTEGRATIONS_UPDATED");
            _guildMemberAdded = new AsyncEvent<GuildMemberAddEventArgs>(EventErrorHandler, "GUILD_MEMBER_ADD");
            _guildMemberRemoved = new AsyncEvent<GuildMemberRemoveEventArgs>(EventErrorHandler, "GUILD_MEMBER_REMOVED");
            _guildMemberUpdated = new AsyncEvent<GuildMemberUpdateEventArgs>(EventErrorHandler, "GUILD_MEMBER_UPDATED");
            _guildRoleCreated = new AsyncEvent<GuildRoleCreateEventArgs>(EventErrorHandler, "GUILD_ROLE_CREATED");
            _guildRoleUpdated = new AsyncEvent<GuildRoleUpdateEventArgs>(EventErrorHandler, "GUILD_ROLE_UPDATED");
            _guildRoleDeleted = new AsyncEvent<GuildRoleDeleteEventArgs>(EventErrorHandler, "GUILD_ROLE_DELETED");
            _messageAcknowledged = new AsyncEvent<MessageAcknowledgeEventArgs>(EventErrorHandler, "MESSAGE_ACKNOWLEDGED");
            _messageUpdated = new AsyncEvent<MessageUpdateEventArgs>(EventErrorHandler, "MESSAGE_UPDATED");
            _messageDeleted = new AsyncEvent<MessageDeleteEventArgs>(EventErrorHandler, "MESSAGE_DELETED");
            _messagesBulkDeleted = new AsyncEvent<MessageBulkDeleteEventArgs>(EventErrorHandler, "MESSAGE_BULK_DELETED");
            _typingStarted = new AsyncEvent<TypingStartEventArgs>(EventErrorHandler, "TYPING_STARTED");
            _userSettingsUpdated = new AsyncEvent<UserSettingsUpdateEventArgs>(EventErrorHandler, "USER_SETTINGS_UPDATED");
            _userUpdated = new AsyncEvent<UserUpdateEventArgs>(EventErrorHandler, "USER_UPDATED");
            _voiceStateUpdated = new AsyncEvent<VoiceStateUpdateEventArgs>(EventErrorHandler, "VOICE_STATE_UPDATED");
            _voiceServerUpdated = new AsyncEvent<VoiceServerUpdateEventArgs>(EventErrorHandler, "VOICE_SERVER_UPDATED");
            _guildMembersChunked = new AsyncEvent<GuildMembersChunkEventArgs>(EventErrorHandler, "GUILD_MEMBERS_CHUNKED");
            _unknownEvent = new AsyncEvent<UnknownEventArgs>(EventErrorHandler, "UNKNOWN_EVENT");
            _messageReactionAdded = new AsyncEvent<MessageReactionAddEventArgs>(EventErrorHandler, "MESSAGE_REACTION_ADDED");
            _messageReactionRemoved = new AsyncEvent<MessageReactionRemoveEventArgs>(EventErrorHandler, "MESSAGE_REACTION_REMOVED");
            _messageReactionsCleared = new AsyncEvent<MessageReactionsClearEventArgs>(EventErrorHandler, "MESSAGE_REACTIONS_CLEARED");
            _webhooksUpdated = new AsyncEvent<WebhooksUpdateEventArgs>(EventErrorHandler, "WEBHOOKS_UPDATED");
            _heartbeated = new AsyncEvent<HeartbeatEventArgs>(EventErrorHandler, "HEARTBEATED");

            _relationshipAdded = new AsyncEvent<RelationshipEventArgs>(EventErrorHandler, "RELATIONSHIP_ADD");
            _relationshipRemoved = new AsyncEvent<RelationshipEventArgs>(EventErrorHandler, "RElATIONSHIP_REMOVE");

            _privateChannels = new List<DiscordDmChannel>();
            _guilds = new Dictionary<ulong, DiscordGuild>();

            _privateChannelsLazy = new Lazy<IReadOnlyList<DiscordDmChannel>>(() => new ReadOnlyCollection<DiscordDmChannel>(_privateChannels));
            _guildsLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(_guilds));
            _presencesLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordPresence>>(() => new ReadOnlyDictionary<ulong, DiscordPresence>(_presences));

            if (Configuration.UseInternalLogHandler)
            {
                DebugLogger.LogMessageReceived += (sender, e) => DebugLogger.LogHandler(sender, e);
            }
        }

        /// <summary>
        /// Registers an extension with this client.
        /// </summary>
        /// <param name="ext">Extension to register.</param>
        /// <returns></returns>
        public void AddExtension(BaseExtension ext)
        {
            ext.Setup(this);
            _extensions.Add(ext);
        }

        /// <summary>
        /// Retrieves a previously-registered extension from this client.
        /// </summary>
        /// <typeparam name="T">Type of extension to retrieve.</typeparam>
        /// <returns>The requested extension.</returns>
        public T GetExtension<T>() where T : BaseExtension
            => _extensions.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

        /// <summary>
        /// Connects to the gateway
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            var w = 7500;
            var i = 5;
            var s = false;
            Exception cex = null;

            if (Configuration.TokenType != TokenType.Bot)
            {
                DebugLogger.LogMessage(LogLevel.Warning, "DSharpPlus", "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.", DateTime.Now);
            }

            DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", $"DSharpPlus, version {VersionString}", DateTime.Now);

            while (i-- > 0 || Configuration.ReconnectIndefinitely)
            {
                try
                {
                    await InternalConnectAsync().ConfigureAwait(false);
                    s = true;
                    break;
                }
                catch (UnauthorizedException)
                {
                    throw new UnauthorizedAccessException("Authentication failed. Check your token and try again.");
                }
                catch (PlatformNotSupportedException)
                {
                    throw;
                }
                catch (NotImplementedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (Configuration.AutoReconnect)
                    {
                        cex = ex;
                        if (i <= 0 && !Configuration.ReconnectIndefinitely)
                        {
                            break;
                        }

                        DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"Connection attempt failed, retrying in {w / 1000}s", DateTime.Now);
                        await Task.Delay(w).ConfigureAwait(false);

                        if (i > 0)
                        {
                            w *= 2;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (!s && cex != null)
            {
                throw new Exception("Could not connect to Discord.", cex);
            }
        }

        public Task ReconnectAsync(bool startNewSession = false)
        {
            if (startNewSession)
            {
                _sessionId = "";
            }

            return _webSocketClient.DisconnectAsync(null);
        }

        internal Task InternalReconnectAsync()
            => ConnectAsync();

        internal async Task InternalConnectAsync()
        {
            await InternalUpdateGatewayAsync().ConfigureAwait(false);
            await InitializeAsync().ConfigureAwait(false);
            _relationships = new List<DiscordRelationship>();

            if (!Presences.ContainsKey(CurrentUser.Id))
            {
                _presences[CurrentUser.Id] = new DiscordPresence
                {
                    Discord = this,
                    RawActivity = new TransportActivity(),
                    Activity = new DiscordActivity(),
                    InternalStatus = "online",
                    InternalUser = new TransportUser
                    {
                        Id = CurrentUser.Id,
                        Username = CurrentUser.Username,
                        Discriminator = CurrentUser.Discriminator,
                        AvatarHash = CurrentUser.AvatarHash
                    }
                };
            }
            else
            {
                var pr = _presences[CurrentUser.Id];
                pr.RawActivity = new TransportActivity();
                pr.Activity = new DiscordActivity();
                pr.InternalStatus = "online";
            }

            Volatile.Write(ref _skippedHeartbeats, 0);

            var webSocketClient = Configuration.WebSocketClientFactory(Configuration.Proxy);

            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            SwapWebSocketClient(webSocketClient);

            var gwuri = new UriBuilder(_gatewayUri)
            {
                Query = Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Stream ? "v=6&encoding=json&compress=zlib-stream" : "v=6&encoding=json"
            };

            await ConnectionSemaphore.WaitAsync().ConfigureAwait(false);
            await _webSocketClient.ConnectAsync(gwuri.Uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Swaps web socket client, DOES NOT RECONNECT.
        /// </summary>
        /// <param name="newSocketClient"></param>
        public void SwapWebSocketClient(BaseWebSocketClient newSocketClient)
        {
            if (_webSocketClient != null)
            {
                _webSocketClient.OnConnect -= SocketOnConnect;
                _webSocketClient.OnDisconnect -= SocketOnDisconnect;
                _webSocketClient.OnMessage -= SocketOnMessage;
                _webSocketClient.OnError -= SocketOnError;
            }

            newSocketClient.OnConnect += SocketOnConnect;
            newSocketClient.OnDisconnect += SocketOnDisconnect;
            newSocketClient.OnMessage += SocketOnMessage;
            newSocketClient.OnError += SocketOnError;

            _webSocketClient = newSocketClient;
        }

        internal Task SocketOnConnect()
            => _socketOpened.InvokeAsync();

        internal async Task SocketOnMessage(SocketMessageEventArgs e)
        {
            try
            {
                await HandleSocketMessageAsync(e.Message);
            }
            catch (Exception ex)
            {
                DebugLogger.LogMessage(LogLevel.Error, "Websocket", $"Socket swallowed an exception: {ex} This is bad!!!!", DateTime.Now);
            }
        }

        internal Task SocketOnError(SocketErrorEventArgs e)
            => _socketErrored.InvokeAsync(new SocketErrorEventArgs(this) { Exception = e.Exception });

        internal async Task SocketOnDisconnect(SocketCloseEventArgs e)
        {
            _cancelTokenSource.Cancel();

            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Connection closed", DateTime.Now);
            await _socketClosed.InvokeAsync(new SocketCloseEventArgs(this) { CloseCode = e.CloseCode, CloseMessage = e.CloseMessage }).ConfigureAwait(false);

            if (Configuration.AutoReconnect)
            {
                DebugLogger.LogMessage(LogLevel.Critical, "Websocket", $"Socket connection terminated ({e.CloseCode.ToString(CultureInfo.InvariantCulture)}, '{e.CloseMessage}'). Reconnecting", DateTime.Now);
                await ConnectAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Disconnects from the gateway
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            Configuration.AutoReconnect = false;
            if (_webSocketClient != null)
            {
                await _webSocketClient.DisconnectAsync(null).ConfigureAwait(false);
            }
        }

        #region Public Functions
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public async Task<DiscordUser> GetUserAsync(ulong userId)
        {
            var usr = InternalGetCachedUser(userId);
            if (usr != null)
            {
                return usr;
            }

            usr = await ApiClient.GetUserAsync(userId).ConfigureAwait(false);
            usr = UserCache.AddOrUpdate(userId, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            return usr;
        }

        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordChannel> GetChannelAsync(ulong id)
            => InternalGetCachedChannel(id) ?? await ApiClient.GetChannelAsync(id).ConfigureAwait(false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="isTTS"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content = null, bool isTTS = false, DiscordEmbed embed = null)
            => ApiClient.CreateMessageAsync(channel.Id, content, isTTS, embed);

        /// <summary>
        /// Creates a guild. This requires the bot to be in less than 10 guilds total.
        /// </summary>
        /// <param name="name">Name of the guild.</param>
        /// <param name="region">Voice region of the guild.</param>
        /// <param name="icon">Stream containing the icon for the guild.</param>
        /// <param name="verificationLevel">Verification level for the guild.</param>
        /// <param name="defaultMessageNotifications">Default message notification settings for the guild.</param>
        /// <returns>The created guild.</returns>
        public Task<DiscordGuild> CreateGuildAsync(string name, string region = null, Optional<Stream> icon = default, VerificationLevel? verificationLevel = null,
            DefaultMessageNotifications? defaultMessageNotifications = null)
        {
            var iconb64 = Optional<string>.FromNoValue();
            if (icon.HasValue && icon.Value != null)
            {
                using (var imgtool = new ImageTool(icon.Value))
                {
                    iconb64 = imgtool.GetBase64();
                }
            }
            else if (icon.HasValue)
            {
                iconb64 = null;
            }

            return ApiClient.CreateGuildAsync(name, region, iconb64, verificationLevel, defaultMessageNotifications);
        }

        /// <summary>
        /// Gets a guild
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> GetGuildAsync(ulong id)
        {
            if (_guilds.ContainsKey(id))
            {
                return _guilds[id];
            }

            var gld = await ApiClient.GetGuildAsync(id).ConfigureAwait(false);
            var chns = await ApiClient.GetGuildChannelsAsync(gld.Id).ConfigureAwait(false);
            gld._channels.AddRange(chns);

            return gld;
        }

        /// <summary>
        /// Gets an invite
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Task<DiscordInvite> GetInviteByCodeAsync(string code)
            => ApiClient.GetInviteAsync(code);

        /// <summary>
        /// Gets a list of connections
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordConnection>> GetConnectionsAsync()
            => ApiClient.GetUsersConnectionsAsync();

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookAsync(ulong id)
            => ApiClient.GetWebhookAsync(id);

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token)
            => ApiClient.GetWebhookWithTokenAsync(id, token);

        /// <summary>
        /// Updates current user's activity and status.
        /// </summary>
        /// <param name="activity">Activity to set.</param>
        /// <param name="userStatus">Status of the user.</param>
        /// <param name="idleSince">Since when is the client performing the specified activity.</param>
        /// <returns></returns>
        public Task UpdateStatusAsync(DiscordActivity activity = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null)
            => InternalUpdateStatusAsync(activity, userStatus, idleSince);

        /// <summary>
        /// Gets information about specified API application.
        /// </summary>
        /// <param name="id">ID of the application.</param>
        /// <returns>Information about specified API application.</returns>
        public Task<DiscordApplication> GetApplicationAsync(ulong id)
            => ApiClient.GetApplicationInfoAsync(id);

        /// <summary>
        /// Edits current user.
        /// </summary>
        /// <param name="username">New username.</param>
        /// <param name="avatar">New avatar.</param>
        /// <returns></returns>
        public async Task<DiscordUser> UpdateCurrentUserAsync(string username = null, Optional<Stream> avatar = default)
        {
            var av64 = Optional<string>.FromNoValue();
            if (avatar.HasValue && avatar.Value != null)
            {
                using (var imgtool = new ImageTool(avatar.Value))
                {
                    av64 = imgtool.GetBase64();
                }
            }
            else if (avatar.HasValue)
            {
                av64 = null;
            }

            var usr = await ApiClient.ModifyCurrentUserAsync(username, av64).ConfigureAwait(false);

            CurrentUser.Username = usr.Username;
            CurrentUser.Discriminator = usr.Discriminator;
            CurrentUser.AvatarHash = usr.AvatarHash;
            return CurrentUser;
        }

        /// <summary>
        /// Requests guild sync for specified guilds. Guild sync sends information about members and presences for a given guild, and makes gateway dispatch additional events.
        /// 
        /// This can only be done for user tokens.
        /// </summary>
        /// <param name="guilds">Guilds to send a sync request for.</param>
        /// <returns></returns>
        public Task SyncGuildsAsync(params DiscordGuild[] guilds)
        {
            if (Configuration.TokenType != TokenType.User)
            {
                throw new InvalidOperationException("This can only be done for user tokens.");
            }

            var to_sync = guilds.Where(xg => !xg.IsSynced).Select(xg => xg.Id);

            if (!to_sync.Any())
            {
                return Task.Delay(0);
            }

            var guild_sync = new GatewayPayload
            {
                OpCode = GatewayOpCode.GuildSync,
                Data = to_sync
            };
            var guild_syncstr = JsonConvert.SerializeObject(guild_sync);

            _webSocketClient.SendMessage(guild_syncstr);
            return Task.Delay(0);
        }
        #endregion

        #region Websocket (Events)
        internal async Task HandleSocketMessageAsync(string data)
        {
            var payload = JsonConvert.DeserializeObject<GatewayPayload>(data);
            switch (payload.OpCode)
            {
                case GatewayOpCode.Dispatch:
                    await HandleDispatchAsync(payload).ConfigureAwait(false);
                    break;

                case GatewayOpCode.Heartbeat:
                    await OnHeartbeatAsync((long)payload.Data).ConfigureAwait(false);
                    break;

                case GatewayOpCode.Reconnect:
                    await OnReconnectAsync().ConfigureAwait(false);
                    break;

                case GatewayOpCode.InvalidSession:
                    await OnInvalidateSessionAsync((bool)payload.Data).ConfigureAwait(false);
                    break;

                case GatewayOpCode.Hello:
                    await OnHelloAsync((payload.Data as JObject).ToObject<GatewayHello>()).ConfigureAwait(false);
                    break;

                case GatewayOpCode.HeartbeatAck:
                    await OnHeartbeatAckAsync().ConfigureAwait(false);
                    break;

                default:
                    DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown OP-Code: {((int)payload.OpCode).ToString(CultureInfo.InvariantCulture)}\n{payload.Data}", DateTime.Now);
                    break;
            }
        }

        internal async Task HandleDispatchAsync(GatewayPayload payload)
        {
            DiscordChannel chn;
            ulong gid;
            ulong cid;
            TransportUser usr;

            if (payload.Data is JObject dat)
            {
                //DebugLogger.LogMessage(LogLevel.Debug, "Gateway", $"Recieved dispatch: {payload.EventName}", DateTime.Now);
                switch (payload.EventName.ToLowerInvariant())
                {
                    case "ready":
                        var glds = (JArray)dat["guilds"];
                        var dmcs = (JArray)dat["private_channels"];
                        await OnReadyEventAsync(dat.ToObject<ReadyPayload>(), glds, dmcs, dat.TryGetValue("relationships", out var t) ? t as JArray : new JArray(), dat.TryGetValue("presences", out var p) ? p as JArray : new JArray(), dat.TryGetValue("user_settings", out var s) ? s as JObject : null, dat.TryGetValue("read_state", out var read) ? read as JArray : null).ConfigureAwait(false);
                        break;

                    case "resumed":
                        await OnResumedAsync().ConfigureAwait(false);
                        break;

                    case "message_ack":
                        cid = (ulong)dat["channel_id"];
                        var mid = (ulong)dat["message_id"];
                        await OnMessageAckEventAsync(InternalGetCachedChannel(cid), mid).ConfigureAwait(false);
                        break;

                    case "message_create":
                        await OnMessageCreateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"].ToObject<TransportUser>()).ConfigureAwait(false);
                        break;

                    case "message_update":
                        await OnMessageUpdateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"]?.ToObject<TransportUser>()).ConfigureAwait(false);
                        break;

                    // delete event does *not* include message object 
                    case "message_delete":
                        await OnMessageDeleteEventAsync((ulong)dat["id"], InternalGetCachedChannel((ulong)dat["channel_id"])).ConfigureAwait(false);
                        break;

                    case "message_delete_bulk":
                        await OnMessageBulkDeleteEventAsync(dat["ids"].ToObject<IEnumerable<ulong>>(), InternalGetCachedChannel((ulong)dat["channel_id"])).ConfigureAwait(false);
                        break;

                    case "presence_update":
                        await OnPresenceUpdateEventAsync(dat, (JObject)dat["user"]).ConfigureAwait(false);
                        break;

                    case "typing_start":
                        cid = (ulong)dat["channel_id"];
                        await OnTypingStartEventAsync((ulong)dat["user_id"], InternalGetCachedChannel(cid), Utilities.GetDateTimeOffset((long)dat["timestamp"])).ConfigureAwait(false);
                        break;

                    case "channel_create":
                        chn = dat.ToObject<DiscordChannel>();
                        await OnChannelCreateEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn, dat["recipients"] as JArray).ConfigureAwait(false);
                        break;

                    case "channel_update":
                        await OnChannelUpdateEventAsync(dat.ToObject<DiscordChannel>()).ConfigureAwait(false);
                        break;

                    case "channel_delete":
                        chn = dat.ToObject<DiscordChannel>();
                        await OnChannelDeleteEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn).ConfigureAwait(false);
                        break;

                    case "channel_pins_update":
                        cid = (ulong)dat["channel_id"];
                        await OnChannelPinsUpdate(InternalGetCachedChannel(cid), DateTimeOffset.Parse((string)dat["last_pin_timestamp"], CultureInfo.InvariantCulture)).ConfigureAwait(false);
                        break;

                    case "guild_create":
                        await OnGuildCreateEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"], dat["presences"].ToObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
                        break;

                    case "guild_update":
                        await OnGuildUpdateEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]).ConfigureAwait(false);
                        break;

                    case "guild_delete":
                        await OnGuildDeleteEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]).ConfigureAwait(false);
                        break;

                    case "guild_sync":
                        gid = (ulong)dat["id"];
                        await OnGuildSyncEventAsync(_guilds[gid], (bool)dat["large"], (JArray)dat["members"], dat["presences"].ToObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
                        break;

                    case "guild_ban_add":
                        usr = dat["user"].ToObject<TransportUser>();
                        gid = (ulong)dat["guild_id"];
                        await OnGuildBanAddEventAsync(usr, _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "guild_ban_remove":
                        usr = dat["user"].ToObject<TransportUser>();
                        gid = (ulong)dat["guild_id"];
                        await OnGuildBanRemoveEventAsync(usr, _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "guild_emojis_update":
                        gid = (ulong)dat["guild_id"];
                        var ems = dat["emojis"].ToObject<IEnumerable<DiscordEmoji>>();
                        await OnGuildEmojisUpdateEventAsync(_guilds[gid], ems).ConfigureAwait(false);
                        break;

                    case "guild_integrations_update":
                        gid = (ulong)dat["guild_id"];
                        await OnGuildIntegrationsUpdateEventAsync(_guilds[gid]).ConfigureAwait(false);
                        break;

                    case "guild_member_add":
                        gid = (ulong)dat["guild_id"];
                        await OnGuildMemberAddEventAsync(dat.ToObject<TransportMember>(), _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "guild_member_remove":
                        gid = (ulong)dat["guild_id"];
                        if (!_guilds.ContainsKey(gid))
                        { DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"Could not find {gid.ToString(CultureInfo.InvariantCulture)} in guild cache.", DateTime.Now); return; }
                        await OnGuildMemberRemoveEventAsync(dat["user"].ToObject<TransportUser>(), _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "guild_member_update":
                        gid = (ulong)dat["guild_id"];
                        await OnGuildMemberUpdateEventAsync(dat["user"].ToObject<TransportUser>(), _guilds[gid], dat["roles"].ToObject<IEnumerable<ulong>>(), (string)dat["nick"]).ConfigureAwait(false);
                        break;

                    case "guild_member_chunk":
                        gid = (ulong)dat["guild_id"];
                        await OnGuildMembersChunkEventAsync(dat["members"].ToObject<IEnumerable<TransportMember>>(), _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "guild_role_create":
                        gid = (ulong)dat["guild_id"];
                        await OnGuildRoleCreateEventAsync(dat["role"].ToObject<DiscordRole>(), _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "guild_role_update":
                        gid = (ulong)dat["guild_id"];
                        await OnGuildRoleUpdateEventAsync(dat["role"].ToObject<DiscordRole>(), _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "guild_role_delete":
                        gid = (ulong)dat["guild_id"];
                        await OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "user_settings_update":
                        await OnUserSettingsUpdateEventAsync(dat).ConfigureAwait(false);
                        break;

                    case "user_update":
                        await OnUserUpdateEventAsync(dat.ToObject<TransportUser>()).ConfigureAwait(false);
                        break;

                    case "voice_state_update":
                        await OnVoiceStateUpdateEventAsync(dat).ConfigureAwait(false);
                        break;

                    case "voice_server_update":
                        gid = (ulong)dat["guild_id"];
                        await OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], _guilds[gid]).ConfigureAwait(false);
                        break;

                    case "message_reaction_add":
                        cid = (ulong)dat["channel_id"];
                        await OnMessageReactionAddAsync((ulong)dat["user_id"], (ulong)dat["message_id"], InternalGetCachedChannel(cid), dat["emoji"].ToObject<DiscordEmoji>()).ConfigureAwait(false);
                        break;

                    case "message_reaction_remove":
                        cid = (ulong)dat["channel_id"];
                        await OnMessageReactionRemoveAsync((ulong)dat["user_id"], (ulong)dat["message_id"], InternalGetCachedChannel(cid), dat["emoji"].ToObject<DiscordEmoji>()).ConfigureAwait(false);
                        break;

                    case "message_reaction_remove_all":
                        cid = (ulong)dat["channel_id"];
                        await OnMessageReactionRemoveAllAsync((ulong)dat["message_id"], InternalGetCachedChannel(cid)).ConfigureAwait(false);
                        break;

                    case "webhooks_update":
                        gid = (ulong)dat["guild_id"];
                        cid = (ulong)dat["channel_id"];
                        await OnWebhooksUpdateAsync(_guilds[gid]._channels.FirstOrDefault(xc => xc.Id == cid), _guilds[gid]).ConfigureAwait(false);
                        break;
                    case "relationship_add":
                        await OnRelationshipAddAsync(dat).ConfigureAwait(false);
                        break;
                    case "relationship_remove":
                        await OnRelationshipRemoveAsync(dat).ConfigureAwait(false);
                        break;
                    default:
                        await OnUnknownEventAsync(payload).ConfigureAwait(false);
                        DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown event: {payload.EventName}\n{payload.Data}", DateTime.Now);
                        break;
                }
            }
            else if (payload.Data is JArray arr)
            {
                DebugLogger.LogMessage(LogLevel.Debug, payload.EventName, arr.ToString(), DateTime.Now);
            }
        }

        #region Events
        internal async Task OnReadyEventAsync(ReadyPayload ready, JArray rawGuilds, JArray rawDmChannels, JArray rawRelationships, JArray rawPresences, JObject settings, JArray readStates)
        {
            //ready.CurrentUser.Discord = this;

            var rusr = ready.CurrentUser;
            CurrentUser.Username = rusr.Username;
            CurrentUser.Discriminator = rusr.Discriminator;
            CurrentUser.AvatarHash = rusr.AvatarHash;
            CurrentUser.MfaEnabled = rusr.MfaEnabled;
            CurrentUser.Verified = rusr.Verified;
            CurrentUser.IsBot = rusr.IsBot;
            CurrentUser.HasNitro = ready.CurrentUser.IsPremium;

            _gatewayVersion = ready.GatewayVersion;
            _sessionId = ready.SessionId;
            var raw_guild_index = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

            var set = settings?.ToObject<DiscordUserSettings>();
            UserSettings = set;

            _presences[ready.CurrentUser.Id] = new DiscordPresence() { InternalStatus = set.Status ?? "online" };

            _privateChannels = rawDmChannels
                .Select(xjt =>
                {
                    var xdc = xjt.ToObject<DiscordDmChannel>();

                    xdc.Discord = this;

                    //xdc._recipients = 
                    //    .Select(xtu => this.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this })
                    //    .ToList();

                    var recips_raw = xjt["recipients"].ToObject<IEnumerable<TransportUser>>();
                    xdc._recipients = new List<DiscordUser>();
                    foreach (var xr in recips_raw)
                    {
                        var xu = new DiscordUser(xr) { Discord = this };
                        xu = UserCache.AddOrUpdate(xr.Id, xu, (id, old) =>
                        {
                            old.Username = xu.Username;
                            old.Discriminator = xu.Discriminator;
                            old.AvatarHash = xu.AvatarHash;
                            return old;
                        });

                        xdc._recipients.Add(xu);
                    }

                    return xdc;
                }).ToList();

            _guilds = ready.Guilds
                .Select(xg =>
                {
                    xg.Discord = this;

                    if (xg._channels == null)
                    {
                        xg._channels = new List<DiscordChannel>();
                    }

                    foreach (var xc in xg.Channels)
                    {
                        xc.GuildId = xg.Id;
                        xc.Discord = this;
                        foreach (var xo in xc._permission_overwrites)
                        {
                            xo.Discord = this;
                            xo._channel_id = xc.Id;
                        }
                    }

                    if (xg._roles == null)
                    {
                        xg._roles = new List<DiscordRole>();
                    }

                    foreach (var xr in xg.Roles)
                    {
                        xr.Discord = this;
                        xr._guild_id = xg.Id;
                    }

                    var raw_guild = raw_guild_index[xg.Id];
                    var raw_members = (JArray)raw_guild["members"];
                    xg._members = new List<DiscordMember>();

                    if (raw_members != null)
                    {
                        foreach (var xj in raw_members)
                        {
                            var xtm = xj.ToObject<TransportMember>();

                            var xu = new DiscordUser(xtm.User) { Discord = this };
                            xu = UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
                            {
                                old.Username = xu.Username;
                                old.Discriminator = xu.Discriminator;
                                old.AvatarHash = xu.AvatarHash;
                                return old;
                            });

                            xg._members.Add(new DiscordMember(xtm) { Discord = this, _guild_id = xg.Id });
                        }
                    }

                    if (xg._emojis == null)
                    {
                        xg._emojis = new List<DiscordEmoji>();
                    }

                    foreach (var xe in xg.Emojis)
                    {
                        xe.Discord = this;
                    }

                    if (xg._voice_states == null)
                    {
                        xg._voice_states = new List<DiscordVoiceState>();
                    }

                    foreach (var xvs in xg.VoiceStates)
                    {
                        xvs.Discord = this;
                    }

                    return xg;
                }).OrderBy(g => set?.GuildPositions?.IndexOf(g.Id)).ToDictionary(xg => xg.Id, xg => xg);

            _guildsLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(_guilds));

            InvokePropertyChanged(nameof(Guilds));

            if(readStates != null)
            {
                foreach (var dat in readStates.ToObject<DiscordReadState[]>())
                {
                    dat.Discord = this;
                    ReadStates[dat.Id] = dat;
                }
            }

            await _ready.InvokeAsync(new ReadyEventArgs(this)).ConfigureAwait(false);

            foreach (var dat in rawPresences)
            {
                await OnPresenceUpdateEventAsync(dat as JObject, (JObject)dat["user"]).ConfigureAwait(false);
            }

            if (rawRelationships != null)
            {
                _relationships.AddRange(rawRelationships.Select(r =>
                {
                    var rel = r.ToObject<DiscordRelationship>();
                    rel.Discord = this;

                    var xu = new DiscordUser(rel.InternalUser) { Discord = this };
                    xu = UserCache.AddOrUpdate(rel.InternalUser.Id, xu, (id, old) =>
                    {
                        old.Username = xu.Username;
                        old.Discriminator = xu.Discriminator;
                        old.AvatarHash = xu.AvatarHash;
                        return old;
                    });

                    return rel;
                }));
            }

            if (!Configuration.Token.StartsWith("Bot"))
            {
                if (Configuration.TokenType == TokenType.User && Configuration.AutomaticGuildSync)
                {
                    await SendGuildSyncAsync().ConfigureAwait(false);
                }
                else if (Configuration.TokenType == TokenType.User)
                {
                    await SyncGuildsAsync(_guilds.Values.Where(g => g.MemberCount <= Configuration.LargeThreshold).ToArray()).ConfigureAwait(false);
                    Volatile.Write(ref _guildDownloadCompleted, true);
                }
            }
        }

        internal Task OnResumedAsync()
        {
            DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", "Session resumed.", DateTime.Now);
            return _resumed.InvokeAsync(new ReadyEventArgs(this));
        }

        private async Task OnRelationshipAddAsync(JToken json)
        {
            var rel = json.ToObject<DiscordRelationship>();
            rel.Discord = this;

            var oldRel = _relationships.FirstOrDefault(r => r.Id == rel.Id);
            if (oldRel != null)
            {
                oldRel.RelationshipType = rel.RelationshipType;
            }
            else
            {
                var xu = new DiscordUser(rel.InternalUser) { Discord = this };
                xu = UserCache.AddOrUpdate(rel.InternalUser.Id, xu, (id, old) =>
                {
                    old.Username = xu.Username;
                    old.Discriminator = xu.Discriminator;
                    old.AvatarHash = xu.AvatarHash;
                    return old;
                });
                _relationships.Add(rel);
            }

            await _relationshipAdded?.InvokeAsync(new RelationshipEventArgs() { Relationship = rel });
        }

        private async Task OnRelationshipRemoveAsync(JToken json)
        {
            var rel = json.ToObject<DiscordRelationship>();
            rel.Discord = this;

            var oldRel = _relationships.FirstOrDefault(r => r.Id == rel.Id);
            if (oldRel != null)
            {
                _relationships.Remove(oldRel);
            }

            await _relationshipAdded?.InvokeAsync(new RelationshipEventArgs() { Relationship = rel });
        }

        internal async Task OnChannelCreateEventAsync(DiscordChannel channel, JArray rawRecipients)
        {
            channel.Discord = this;

            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var chn = channel as DiscordDmChannel;

                var recips = rawRecipients.ToObject<IEnumerable<TransportUser>>()
                    .Select(xtu => InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this });
                chn._recipients = recips.ToList();

                _privateChannels.Add(chn);

                await _dmChannelCreated.InvokeAsync(new DmChannelCreateEventArgs(this) { Channel = chn }).ConfigureAwait(false);
            }
            else
            {
                channel.Discord = this;
                foreach (var xo in channel._permission_overwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = channel.Id;
                }

                _guilds[channel.GuildId]._channels.Add(channel);

                await _channelCreated.InvokeAsync(new ChannelCreateEventArgs(this) { Channel = channel, Guild = channel.Guild }).ConfigureAwait(false);
            }
        }

        internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            channel.Discord = this;

            var gld = channel.Guild;

            var channel_new = InternalGetCachedChannel(channel.Id);
            DiscordChannel channel_old = null;

            if (channel_new != null)
            {
                channel_old = new DiscordChannel
                {
                    Bitrate = channel_new.Bitrate,
                    Discord = this,
                    GuildId = channel_new.GuildId,
                    Id = channel_new.Id,
                    //IsPrivate = channel_new.IsPrivate,
                    LastMessageId = channel_new.LastMessageId,
                    Name = channel_new.Name,
                    _permission_overwrites = new List<DiscordOverwrite>(channel_new._permission_overwrites),
                    Position = channel_new.Position,
                    Topic = channel_new.Topic,
                    Type = channel_new.Type,
                    UserLimit = channel_new.UserLimit,
                    ParentId = channel_new.ParentId,
                    IsNSFW = channel_new.IsNSFW
                };
            }
            else
            {
                gld._channels.Add(channel);
            }

            channel_new.Bitrate = channel.Bitrate;
            channel_new.Name = channel.Name;
            channel_new.Position = channel.Position;
            channel_new.Topic = channel.Topic;
            channel_new.UserLimit = channel.UserLimit;
            channel_new.ParentId = channel.ParentId;
            channel_new.IsNSFW = channel.IsNSFW;

            channel_new._permission_overwrites.Clear();

            foreach (var po in channel._permission_overwrites)
            {
                po.Discord = this;
                po._channel_id = channel.Id;
            }

            channel_new._permission_overwrites.AddRange(channel._permission_overwrites);

            await _channelUpdated.InvokeAsync(new ChannelUpdateEventArgs(this) { ChannelAfter = channel_new, Guild = gld, ChannelBefore = channel_old }).ConfigureAwait(false);
        }

        internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            channel.Discord = this;

            //if (channel.IsPrivate)
            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var chn = channel as DiscordDmChannel;

                var index = _privateChannels.FindIndex(xc => xc.Id == chn.Id);
                chn = _privateChannels[index];
                _privateChannels.RemoveAt(index);

                await _dmChannelDeleted.InvokeAsync(new DmChannelDeleteEventArgs(this) { Channel = chn }).ConfigureAwait(false);
            }
            else
            {
                var gld = channel.Guild;
                var index = gld._channels.FindIndex(xc => xc.Id == channel.Id);
                channel = gld._channels[index];
                gld._channels.RemoveAt(index);

                await _channelDeleted.InvokeAsync(new ChannelDeleteEventArgs(this) { Channel = channel, Guild = gld }).ConfigureAwait(false);
            }
        }

        internal async Task OnChannelPinsUpdate(DiscordChannel channel, DateTimeOffset lastPinTimestamp)
        {
            if (channel == null)
            {
                return;
            }

            var ea = new ChannelPinsUpdateEventArgs(this)
            {
                Channel = channel,
                LastPinTimestamp = lastPinTimestamp
            };
            await _channelPinsUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            if (presences != null)
            {
                foreach (var xp in presences)
                {
                    xp.Discord = this;
                    xp.Activity = new DiscordActivity(xp.RawActivity);
                    _presences[xp.InternalUser.Id] = xp;
                }
            }

            var exists = _guilds.ContainsKey(guild.Id);

            guild.Discord = this;
            guild.IsUnavailable = false;
            var event_guild = guild;
            if (exists)
            {
                guild = _guilds[event_guild.Id];
            }

            if (guild._channels == null)
            {
                guild._channels = new List<DiscordChannel>();
            }

            if (guild._roles == null)
            {
                guild._roles = new List<DiscordRole>();
            }

            if (guild._emojis == null)
            {
                guild._emojis = new List<DiscordEmoji>();
            }

            if (guild._voice_states == null)
            {
                guild._voice_states = new List<DiscordVoiceState>();
            }

            if (guild._members == null)
            {
                guild._members = new List<DiscordMember>();
            }

            UpdateCachedGuild(event_guild, rawMembers);

            guild.JoinedAt = event_guild.JoinedAt;
            guild.IsLarge = event_guild.IsLarge;
            guild.MemberCount = Math.Max(event_guild.MemberCount, guild._members.Count);
            guild.IsUnavailable = event_guild.IsUnavailable;
            guild._voice_states.AddRange(event_guild._voice_states);

            foreach (var xc in guild._channels)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permission_overwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach (var xe in guild._emojis)
            {
                xe.Discord = this;
            }

            foreach (var xvs in guild._voice_states)
            {
                xvs.Discord = this;
            }

            foreach (var xr in guild._roles)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }

            var old = Volatile.Read(ref _guildDownloadCompleted);
            var dcompl = _guilds.Values.All(xg => !xg.IsUnavailable);
            Volatile.Write(ref _guildDownloadCompleted, dcompl);

            if (exists)
            {
                await _guildAvailable.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild }).ConfigureAwait(false);
            }
            else
            {
                await _guildCreated.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild }).ConfigureAwait(false);
            }

            if (dcompl && !old)
            {
                await _guildDownloadCompletedEv.InvokeAsync(new GuildDownloadCompletedEventArgs(this)).ConfigureAwait(false);
            }
        }

        internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray rawMembers)
        {
            if (!_guilds.ContainsKey(guild.Id))
            {
                _guilds[guild.Id] = guild;
            }

            guild.Discord = this;
            guild.IsUnavailable = false;
            var event_guild = guild;
            guild = _guilds[event_guild.Id];

            if (guild._channels == null)
            {
                guild._channels = new List<DiscordChannel>();
            }

            if (guild._roles == null)
            {
                guild._roles = new List<DiscordRole>();
            }

            if (guild._emojis == null)
            {
                guild._emojis = new List<DiscordEmoji>();
            }

            if (guild._voice_states == null)
            {
                guild._voice_states = new List<DiscordVoiceState>();
            }

            if (guild._members == null)
            {
                guild._members = new List<DiscordMember>();
            }

            UpdateCachedGuild(event_guild, rawMembers);

            foreach (var xc in guild._channels)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permission_overwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach (var xe in guild._emojis)
            {
                xe.Discord = this;
            }

            foreach (var xvs in guild._voice_states)
            {
                xvs.Discord = this;
            }

            foreach (var xr in guild._roles)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }

            await _guildUpdated.InvokeAsync(new GuildUpdateEventArgs(this) { Guild = guild }).ConfigureAwait(false);
        }

        internal async Task OnGuildDeleteEventAsync(DiscordGuild guild, JArray rawMembers)
        {
            if (!_guilds.ContainsKey(guild.Id))
            {
                return;
            }

            var gld = _guilds[guild.Id];
            if (guild.IsUnavailable)
            {
                gld.IsUnavailable = true;

                await _guildUnavailable.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = guild, Unavailable = true }).ConfigureAwait(false);
            }
            else
            {
                _guilds.Remove(guild.Id);

                await _guildDeleted.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = gld }).ConfigureAwait(false);
            }
        }

        internal async Task OnGuildSyncEventAsync(DiscordGuild guild, bool isLarge, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            presences = presences.Select(xp => { xp.Discord = this; xp.Activity = new DiscordActivity(xp.RawActivity); return xp; });
            foreach (var xp in presences)
            {
                _presences[xp.InternalUser.Id] = xp;
            }

            guild.IsSynced = true;
            guild.IsLarge = isLarge;

            UpdateCachedGuild(guild, rawMembers);

            if (Configuration.AutomaticGuildSync)
            {
                var dcompl = _guilds.Values.All(xg => xg.IsSynced);
                Volatile.Write(ref _guildDownloadCompleted, dcompl);
            }

            await _guildAvailable.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild }).ConfigureAwait(false);
        }

        internal async Task OnPresenceUpdateEventAsync(JObject rawPresence, JObject rawUser, bool noFire = false)
        {
            var uid = (ulong)rawUser["id"];
            DiscordPresence old = null;
            if (_presences.TryGetValue(uid, out var presence))
            {
                old = new DiscordPresence(presence);
                JsonConvert.PopulateObject(rawPresence.ToString(), presence);

                if (rawPresence["game"] == null || rawPresence["game"].Type == JTokenType.Null)
                {
                    presence.RawActivity = null;
                }

                if (presence.Activity != null)
                {
                    presence.Activity.UpdateWith(presence.RawActivity);
                }
                else
                {
                    presence.Activity = new DiscordActivity();
                }
            }
            else
            {
                presence = rawPresence.ToObject<DiscordPresence>();
                presence.Discord = this;
                presence.Activity = new DiscordActivity(presence.RawActivity);
                _presences[presence.InternalUser.Id] = presence;
            }

            if (UserCache.TryGetValue(uid, out var usr))
            {
                if (old != null)
                {
                    old.InternalUser.Username = usr.Username;
                    old.InternalUser.Discriminator = usr.Discriminator;
                    old.InternalUser.AvatarHash = usr.AvatarHash;
                }

                if (rawUser["username"] is object)
                {
                    usr.Username = (string)rawUser["username"];
                }

                if (rawUser["discriminator"] is object)
                {
                    usr.Discriminator = (string)rawUser["discriminator"];
                }

                if (rawUser["avatar"] is object)
                {
                    usr.AvatarHash = (string)rawUser["avatar"];
                }

                presence.InternalUser.Username = usr.Username;
                presence.InternalUser.Discriminator = usr.Discriminator;
                presence.InternalUser.AvatarHash = usr.AvatarHash;
            }

            if (usr != null)
            {
                usr.InvokePropertyChanged("Presence");
            }

            var usrafter = usr ?? new DiscordUser(presence.InternalUser);
            var ea = new PresenceUpdateEventArgs
            {
                Client = this,
                Status = presence.Status,
                Activity = presence.Activity,
                User = usr,
                PresenceBefore = old,
                PresenceAfter = presence,
                UserBefore = old != null ? new DiscordUser(old.InternalUser) : usrafter,
                UserAfter = usrafter
            };
            if (!noFire)
            {
                await _presenceUpdated.InvokeAsync(ea).ConfigureAwait(false);
            }
        }

        internal async Task OnGuildBanAddEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanAddEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await _guildBanAdded.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildBanRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanRemoveEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await _guildBanRemoved.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
        {
            var old_emojis = new List<DiscordEmoji>(guild._emojis);
            guild._emojis.Clear();
            guild._emojis.AddRange(newEmojis.Select(xe => { xe.Discord = this; return xe; }));
            var ea = new GuildEmojisUpdateEventArgs(this)
            {
                Guild = guild,
                EmojisAfter = guild.Emojis,
                EmojisBefore = new ReadOnlyCollection<DiscordEmoji>(old_emojis)
            };
            await _guildEmojisUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild guild)
        {
            var ea = new GuildIntegrationsUpdateEventArgs(this)
            {
                Guild = guild
            };
            await _guildIntegrationsUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
        {
            var usr = new DiscordUser(member.User) { Discord = this };
            usr = UserCache.AddOrUpdate(member.User.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            var mbr = new DiscordMember(member)
            {
                Discord = this,
                _guild_id = guild.Id
            };

            guild._members.Add(mbr);
            guild.MemberCount++;

            var ea = new GuildMemberAddEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await _guildMemberAdded.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(new DiscordUser(user)) { Discord = this, _guild_id = guild.Id };

            var index = guild._members.FindIndex(xm => xm.Id == mbr.Id);
            if (index > -1)
            {
                guild._members.RemoveAt(index);
            }

            guild.MemberCount--;

            var ea = new GuildMemberRemoveEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await _guildMemberRemoved.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMemberUpdateEventAsync(TransportUser user, DiscordGuild guild, IEnumerable<ulong> roles, string nick)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };

            var nick_old = mbr.Nickname;
            var roles_old = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles));

            mbr.Nickname = nick;
            mbr._role_ids.Clear();
            mbr._role_ids.AddRange(roles);

            mbr.InvokePropertyChanged("Roles");
            mbr.InvokePropertyChanged("Color");
            mbr.InvokePropertyChanged("ColorBrush");

            var ea = new GuildMemberUpdateEventArgs(this)
            {
                Guild = guild,
                Member = mbr,

                NicknameAfter = mbr.Nickname,
                RolesAfter = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles)),

                NicknameBefore = nick_old,
                RolesBefore = roles_old
            };
            await _guildMemberUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildRoleCreateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            role.Discord = this;
            role._guild_id = guild.Id;

            guild._roles.Add(role);

            var ea = new GuildRoleCreateEventArgs(this)
            {
                Guild = guild,
                Role = role
            };
            await _guildRoleCreated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            var role_new = guild.Roles.FirstOrDefault(xr => xr.Id == role.Id);
            var role_old = new DiscordRole
            {
                _guild_id = guild.Id,
                _color = role_new._color,
                Discord = this,
                IsHoisted = role_new.IsHoisted,
                Id = role_new.Id,
                IsManaged = role_new.IsManaged,
                IsMentionable = role_new.IsManaged,
                Name = role_new.Name,
                Permissions = role_new.Permissions,
                Position = role_new.Position
            };

            role_new._guild_id = guild.Id;
            role_new._color = role._color;
            role_new.IsHoisted = role.IsHoisted;
            role_new.IsManaged = role.IsManaged;
            role_new.IsMentionable = role.IsMentionable;
            role_new.Name = role.Name;
            role_new.Permissions = role.Permissions;
            role_new.Position = role.Position;

            var ea = new GuildRoleUpdateEventArgs(this)
            {
                Guild = guild,
                RoleAfter = role_new,
                RoleBefore = role_old
            };
            await _guildRoleUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildRoleDeleteEventAsync(ulong roleId, DiscordGuild guild)
        {
            var index = guild._roles.FindIndex(xr => xr.Id == roleId);
            var role = guild._roles[index];
            guild._roles.RemoveAt(index);

            var ea = new GuildRoleDeleteEventArgs(this)
            {
                Guild = guild,
                Role = role
            };
            await _guildRoleDeleted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageAckEventAsync(DiscordChannel chn, ulong messageId)
        {
            DiscordMessage msg = null;
            if (MessageCache?.TryGet(xm => xm.Id == messageId && xm.ChannelId == chn.Id, out msg) != true)
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = chn.Id,
                    Discord = this,
                };
            }

            DiscordReadState state = ReadStates[chn.Id];
            state.LastMessageId = messageId;
            state.MentionCount = 0;

            await _messageAcknowledged.InvokeAsync(new MessageAcknowledgeEventArgs(this) { Message = msg }).ConfigureAwait(false);
        }

        internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author)
        {
            message.Discord = this;

            if (message.Channel == null)
            {
                DebugLogger.LogMessage(LogLevel.Warning, "Event", "Could not find channel last message belonged to", DateTime.Now);
            }
            else
            {
                message.Channel.LastMessageId = message.Id;
            }

            var guild = message.Channel?.Guild;

            var usr = new DiscordUser(author) { Discord = this };
            usr = UserCache.AddOrUpdate(author.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (guild != null)
            {
                var mbr = guild.Members.FirstOrDefault(xm => xm.Id == author.Id);
                if (mbr == null)
                {
                    mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
                    guild._members.Add(mbr);
                }

                message.Author = mbr;
            }
            else
            {
                message.Author = usr;
            }

            var mentioned_users = new List<DiscordUser>();
            var mentioned_roles = guild != null ? new List<DiscordRole>() : null;
            var mentioned_channels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentioned_users = Utilities.GetUserMentions(message).Select(xid => guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentioned_roles = Utilities.GetRoleMentions(message).Select(xid => guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentioned_channels = Utilities.GetChannelMentions(message).Select(xid => guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentioned_users = Utilities.GetUserMentions(message).Select(InternalGetCachedUser).ToList();
                }
            }

            message._mentionedUsers = mentioned_users;
            message._mentionedRoles = mentioned_roles;
            message._mentionedChannels = mentioned_channels;

            if (message._reactions == null)
            {
                message._reactions = new List<DiscordReaction>();
            }

            foreach (var xr in message._reactions)
            {
                xr.Emoji.Discord = this;
            }

            if (Configuration.MessageCacheSize > 0 && message.Channel != null)
            {
                MessageCache.Add(message);
            }

            if (message.Channel.ReadState != null)
            {
                message.Channel.ReadState.LastMessageId = message.Id;
                message.Channel.ReadState.InvokePropertyChanged("LastMessageId");

                if (author.Id != CurrentUser.Id)
                {

                    if (message.MentionEveryone || message._mentionedUsers.Any(u => u?.Id == CurrentUser.Id))
                    {
                        message.Channel.ReadState.MentionCount += 1;
                    }

                    message.Channel.ReadState.InvokePropertyChanged("Unread");
                    message.Channel.InvokePropertyChanged("ReadState");
                }
            }

            MessageCreateEventArgs ea = new MessageCreateEventArgs(this)
            {
                Message = message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentioned_users),
                MentionedRoles = mentioned_roles != null ? new ReadOnlyCollection<DiscordRole>(mentioned_roles) : null,
                MentionedChannels = mentioned_channels != null ? new ReadOnlyCollection<DiscordChannel>(mentioned_channels) : null
            };
            await _messageCreated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author)
        {
            DiscordGuild guild;

            message.Discord = this;
            var event_message = message;

            DiscordMessage oldmsg = null;
            if (Configuration.MessageCacheSize == 0 || !MessageCache.TryGet(xm => xm.Id == event_message.Id && xm.ChannelId == event_message.ChannelId, out message))
            {
                message = event_message;
                guild = message.Channel?.Guild;

                if (author != null)
                {
                    var usr = new DiscordUser(author) { Discord = this };
                    usr = UserCache.AddOrUpdate(author.Id, usr, (id, old) =>
                    {
                        old.Username = usr.Username;
                        old.Discriminator = usr.Discriminator;
                        old.AvatarHash = usr.AvatarHash;
                        return old;
                    });

                    if (guild != null)
                    {
                        var mbr = guild.Members.FirstOrDefault(xm => xm.Id == author.Id) ?? new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
                        message.Author = mbr;
                    }
                    else
                    {
                        message.Author = usr;
                    }
                }

                if (message._reactions == null)
                {
                    message._reactions = new List<DiscordReaction>();
                }

                foreach (var xr in message._reactions)
                {
                    xr.Emoji.Discord = this;
                }
            }
            else
            {
                oldmsg = new DiscordMessage(message);

                guild = message.Channel?.Guild;
                message.EditedTimestampRaw = event_message.EditedTimestampRaw;
                message._embeds.Clear();
                message._embeds.AddRange(event_message._embeds);
                message.Pinned = event_message.Pinned;
                message.IsTTS = event_message.IsTTS;
                message.Content = event_message.Content ?? message.Content;
            }

            var mentioned_users = new List<DiscordUser>();
            var mentioned_roles = guild != null ? new List<DiscordRole>() : null;
            var mentioned_channels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentioned_users = Utilities.GetUserMentions(message).Select(xid => guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentioned_roles = Utilities.GetRoleMentions(message).Select(xid => guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentioned_channels = Utilities.GetChannelMentions(message).Select(xid => guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentioned_users = Utilities.GetUserMentions(message).Select(InternalGetCachedUser).ToList();
                }
            }

            message._mentionedUsers = mentioned_users;
            message._mentionedRoles = mentioned_roles;
            message._mentionedChannels = mentioned_channels;

            var ea = new MessageUpdateEventArgs(this)
            {
                Message = message,
                MessageBefore = oldmsg,
                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentioned_users),
                MentionedRoles = mentioned_roles != null ? new ReadOnlyCollection<DiscordRole>(mentioned_roles) : null,
                MentionedChannels = mentioned_channels != null ? new ReadOnlyCollection<DiscordChannel>(mentioned_channels) : null
            };
            await _messageUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageDeleteEventAsync(ulong messageId, DiscordChannel channel)
        {
            if (Configuration.MessageCacheSize == 0 || !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channel.Id,
                    Discord = this,
                };
            }
            if (Configuration.MessageCacheSize > 0)
            {
                MessageCache.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channel.Id);
            }

            var ea = new MessageDeleteEventArgs(this)
            {
                Channel = channel,
                Message = msg
            };
            await _messageDeleted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageBulkDeleteEventAsync(IEnumerable<ulong> messageIds, DiscordChannel channel)
        {
            var msgs = new List<DiscordMessage>(messageIds.Count());
            foreach (var message_id in messageIds)
            {
                if (Configuration.MessageCacheSize == 0 || !MessageCache.TryGet(xm => xm.Id == message_id && xm.ChannelId == channel.Id, out var msg))
                {
                    msg = new DiscordMessage
                    {
                        Id = message_id,
                        ChannelId = channel.Id,
                        Discord = this,
                    };
                }
                if (Configuration.MessageCacheSize > 0)
                {
                    MessageCache.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channel.Id);
                }

                msgs.Add(msg);
            }

            var ea = new MessageBulkDeleteEventArgs(this)
            {
                Channel = channel,
                Messages = new ReadOnlyCollection<DiscordMessage>(msgs)
            };
            await _messagesBulkDeleted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnTypingStartEventAsync(ulong userId, DiscordChannel channel, DateTimeOffset started)
        {
            if (channel == null)
            {
                return;
            }

            if (!UserCache.TryGetValue(userId, out var usr))
            {
                usr = new DiscordUser { Id = userId, Discord = this };
            }

            if (channel.Guild != null)
            {
                usr = channel.Guild.Members.FirstOrDefault(xm => xm.Id == userId) ?? new DiscordMember(usr) { Discord = this, _guild_id = channel.GuildId };
            }

            var ea = new TypingStartEventArgs(this)
            {
                Channel = channel,
                User = usr,
                StartedAt = started
            };
            await _typingStarted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnUserSettingsUpdateEventAsync(JObject json)
        {
            var usr = new DiscordUser(json.ToObject<TransportUser>()) { Discord = this };

            if ((json as JObject).TryGetValue("theme", out var t))
            {
                UserSettings.Theme = t.ToObject<string>();
            }

            InvokePropertyChanged(nameof(UserSettings));

            Presences[CurrentUser.Id].InternalStatus = json.TryGetValue("status", out var j) ? j.ToString() : Presences[CurrentUser.Id].InternalStatus;
            InvokePropertyChanged(nameof(CurrentUser));

            var ea = new UserSettingsUpdateEventArgs(this)
            {
                User = usr
            };
            await _userSettingsUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnUserUpdateEventAsync(TransportUser user)
        {
            var usr_old = new DiscordUser
            {
                AvatarHash = CurrentUser.AvatarHash,
                Discord = this,
                Discriminator = CurrentUser.Discriminator,
                Email = CurrentUser.Email,
                Id = CurrentUser.Id,
                IsBot = CurrentUser.IsBot,
                MfaEnabled = CurrentUser.MfaEnabled,
                Username = CurrentUser.Username,
                Verified = CurrentUser.Verified
            };

            CurrentUser.AvatarHash = user.AvatarHash;
            CurrentUser.Discriminator = user.Discriminator;
            CurrentUser.Email = user.Email;
            CurrentUser.Id = user.Id;
            CurrentUser.IsBot = user.IsBot;
            CurrentUser.MfaEnabled = user.MfaEnabled;
            CurrentUser.Username = user.Username;
            CurrentUser.Verified = user.Verified;

            var ea = new UserUpdateEventArgs(this)
            {
                UserAfter = CurrentUser,
                UserBefore = usr_old
            };
            await _userUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnVoiceStateUpdateEventAsync(JObject raw)
        {
            var gid = (ulong)raw["guild_id"];
            var uid = (ulong)raw["user_id"];
            var gld = _guilds[gid];

            var vstate_new = gld._voice_states.FirstOrDefault(xvs => xvs.UserId == uid);
            var vstate_old = vstate_new != null ? new DiscordVoiceState(vstate_new) : null;
            if (vstate_new == null)
            {
                vstate_new = raw.ToObject<DiscordVoiceState>();
                vstate_new.Discord = this;
                gld._voice_states.Add(vstate_new);
            }
            else
            {
                JsonConvert.PopulateObject(raw.ToString(), vstate_new); // TODO: Find a better way
            }

            var mbr = gld._members.FirstOrDefault(xm => xm.Id == uid);
            if (mbr != null)
            {
                mbr.IsMuted = vstate_new.IsServerMuted;
                mbr.IsDeafened = vstate_new.IsServerDeafened;
            }

            var ea = new VoiceStateUpdateEventArgs(this)
            {
                Guild = vstate_new.Guild,
                Channel = vstate_new.Channel,
                User = vstate_new.User,
                SessionId = vstate_new.SessionId,

                Before = vstate_old,
                After = vstate_new
            };
            await _voiceStateUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, DiscordGuild guild)
        {
            var ea = new VoiceServerUpdateEventArgs(this)
            {
                Endpoint = endpoint,
                VoiceToken = token,
                Guild = guild
            };
            await _voiceServerUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMembersChunkEventAsync(IEnumerable<TransportMember> members, DiscordGuild guild)
        {
            var ids = guild.Members.Select(xm => xm.Id);
            var mbrs = members.Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id })
                .Where(xm => !ids.Contains(xm.Id));

            guild._members.AddRange(mbrs);
            guild.MemberCount = guild._members.Count;

            var ea = new GuildMembersChunkEventArgs(this)
            {
                Guild = guild,
                Members = new ReadOnlyCollection<DiscordMember>(new List<DiscordMember>(mbrs))
            };
            await _guildMembersChunked.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnUnknownEventAsync(GatewayPayload payload)
        {
            var ea = new UnknownEventArgs(this) { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
            await _unknownEvent.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, DiscordChannel channel, DiscordEmoji emoji)
        {
            emoji.Discord = this;

            if (!UserCache.TryGetValue(userId, out var usr))
            {
                usr = new DiscordUser { Id = userId, Discord = this };
            }

            if (channel.Guild != null)
            {
                usr = channel.Guild.Members.FirstOrDefault(xm => xm.Id == userId) ?? new DiscordMember(usr) { Discord = this, _guild_id = channel.GuildId };
            }

            DiscordMessage msg = null;
            if (Configuration.MessageCacheSize == 0 || !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channel.Id,
                    Discord = this,
                    _reactions = new List<DiscordReaction>()
                };
            }

            var react = msg._reactions.FirstOrDefault(xr => xr.Emoji == emoji);
            if (react == null)
            {
                msg._reactions.Add(react = new DiscordReaction
                {
                    Count = 1,
                    Emoji = emoji,
                    IsMe = CurrentUser.Id == userId
                });
            }
            else
            {
                react.Count++;
                react.IsMe |= CurrentUser.Id == userId;
            }

            var ea = new MessageReactionAddEventArgs(this)
            {
                Message = msg,
                Channel = channel,
                User = usr,
                Emoji = emoji
            };
            await _messageReactionAdded.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, DiscordChannel channel, DiscordEmoji emoji)
        {
            emoji.Discord = this;

            if (!UserCache.TryGetValue(userId, out var usr))
            {
                usr = new DiscordUser { Id = userId, Discord = this };
            }

            if (channel.Guild != null)
            {
                usr = channel.Guild.Members.FirstOrDefault(xm => xm.Id == userId) ?? new DiscordMember(usr) { Discord = this, _guild_id = channel.GuildId };
            }

            DiscordMessage msg = null;
            if (Configuration.MessageCacheSize == 0 ||
                !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channel.Id,
                    Discord = this
                };
            }

            var react = msg._reactions?.FirstOrDefault(xr => xr.Emoji == emoji);
            if (react != null)
            {
                react.Count--;
                react.IsMe &= CurrentUser.Id != userId;

                if (msg._reactions != null && react.Count <= 0) // shit happens
                {
                    for (var i = 0; i < msg._reactions.Count; i++)
                    {
                        if (msg._reactions[i].Emoji == emoji)
                        {
                            msg._reactions.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            var ea = new MessageReactionRemoveEventArgs(this)
            {
                Message = msg,
                Channel = channel,
                User = usr,
                Emoji = emoji
            };
            await _messageReactionRemoved.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, DiscordChannel channel)
        {
            DiscordMessage msg = null;
            if (Configuration.MessageCacheSize == 0 ||
                !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channel.Id,
                    Discord = this
                };
            }

            msg._reactions?.Clear();

            var ea = new MessageReactionsClearEventArgs(this)
            {
                Message = msg,
                Channel = channel
            };
            await _messageReactionsCleared.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnWebhooksUpdateAsync(DiscordChannel channel, DiscordGuild guild)
        {
            var ea = new WebhooksUpdateEventArgs(this)
            {
                Channel = channel,
                Guild = guild
            };
            await _webhooksUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }
        #endregion

        internal async Task OnHeartbeatAsync(long seq)
        {
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received Heartbeat - Sending Ack.", DateTime.Now);
            await SendHeartbeatAsync(seq).ConfigureAwait(false);
        }

        internal async Task OnReconnectAsync()
        {
            DebugLogger.LogMessage(LogLevel.Info, "Websocket", "Received OP 7 - Reconnect. ", DateTime.Now);

            await ReconnectAsync().ConfigureAwait(false);
        }

        internal async Task OnInvalidateSessionAsync(bool data)
        {
            if (data)
            {
                DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received true in OP 9 - Waiting a few seconds and sending resume again.", DateTime.Now);
                await Task.Delay(6000).ConfigureAwait(false);
                await SendResumeAsync().ConfigureAwait(false);
            }
            else
            {
                DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received false in OP 9 - Starting a new session", DateTime.Now);
                _sessionId = "";
                await SendIdentifyAsync().ConfigureAwait(false);
            }
        }

        internal async Task OnHelloAsync(GatewayHello hello)
        {
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received OP 10 (HELLO) - Trying to either resume or identify", DateTime.Now);
            //this._waiting_for_ack = false;
            Interlocked.CompareExchange(ref _skippedHeartbeats, 0, 0);
            _heartbeatInterval = hello.HeartbeatInterval;
            _heartbeatTask = new Task(StartHeartbeating, _cancelToken, TaskCreationOptions.LongRunning);
            _heartbeatTask.Start();

            if (_sessionId == "")
            {
                await SendIdentifyAsync().ConfigureAwait(false);
            }
            else
            {
                await SendResumeAsync().ConfigureAwait(false);
            }

            _ = Task.Delay(5100).ContinueWith(t =>
            {
                ConnectionSemaphore.Release();
            }).ConfigureAwait(false);
        }

        internal async Task OnHeartbeatAckAsync()
        {
            //_waiting_for_ack = false;
            Interlocked.Decrement(ref _skippedHeartbeats);

            var ping = Volatile.Read(ref _ping);
            ping = (int)(DateTime.Now - _lastHeartbeat).TotalMilliseconds;

            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received WebSocket Heartbeat Ack", DateTime.Now);
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", $"Ping {ping.ToString(CultureInfo.InvariantCulture)}ms", DateTime.Now);

            Volatile.Write(ref _ping, ping);

            var args = new HeartbeatEventArgs(this)
            {
                Ping = Ping,
                Timestamp = DateTimeOffset.Now
            };

            await _heartbeated.InvokeAsync(args).ConfigureAwait(false);
        }

        //internal async Task StartHeartbeatingAsync()
        internal void StartHeartbeating()
        {
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Starting Heartbeat", DateTime.Now);
            var token = _cancelToken;
            try
            {
                while (true)
                {
                    SendHeartbeatAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    Task.Delay(_heartbeatInterval, _cancelToken).ConfigureAwait(false).GetAwaiter().GetResult();
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (NullReferenceException) { }
            catch (OperationCanceledException) { }
        }

        internal Task InternalUpdateStatusAsync(DiscordActivity activity, UserStatus? userStatus, DateTimeOffset? idleSince)
        {
            if (activity != null && activity.Name != null && activity.Name.Length > 128)
            {
                throw new Exception("Game name can't be longer than 128 characters!");
            }

            var since_unix = idleSince != null ? (long?)Utilities.GetUnixTime(idleSince.Value) : null;
            var act = activity ?? new DiscordActivity();

            var status = new StatusUpdate
            {
                Activity = new TransportActivity(act),
                IdleSince = since_unix,
                IsAFK = idleSince != null,
                Status = userStatus ?? UserStatus.Online
            };
            var status_update = new GatewayPayload
            {
                OpCode = GatewayOpCode.StatusUpdate,
                Data = status
            };
            var statusstr = JsonConvert.SerializeObject(status_update);

            _webSocketClient.SendMessage(statusstr);

            if (!_presences.ContainsKey(CurrentUser.Id))
            {
                _presences[CurrentUser.Id] = new DiscordPresence
                {
                    Discord = this,
                    Activity = act,
                    InternalStatus = userStatus?.ToString() ?? "online",
                    InternalUser = new TransportUser { Id = CurrentUser.Id }
                };
            }
            else
            {
                var pr = _presences[CurrentUser.Id];
                pr.Activity = act;
                pr.InternalStatus = userStatus?.ToString() ?? pr.InternalStatus;
            }

            return Task.Delay(0);
        }

        public Task SendHeartbeatAsync()
        {
            var _last_heartbeat = DateTimeOffset.Now;
            var _sequence = (long)(_last_heartbeat - DiscordEpoch).TotalMilliseconds;

            return SendHeartbeatAsync(_sequence);
        }

        internal async Task SendHeartbeatAsync(long seq)
        {
            var more_than_5 = Volatile.Read(ref _skippedHeartbeats) > 5;
            var guilds_comp = Volatile.Read(ref _guildDownloadCompleted);
            if (guilds_comp && more_than_5)
            {
                DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", "More than 5 heartbeats were skipped. Issuing reconnect.", DateTime.Now);
                await ReconnectAsync().ConfigureAwait(false);
                return;
            }
            else if (!guilds_comp && more_than_5)
            {
                DebugLogger.LogMessage(LogLevel.Warning, "DSharpPlus", "More than 5 heartbeats were skipped while the guild download is running.", DateTime.Now);
            }

            Volatile.Write(ref _lastSequence, seq);
            var _last_heartbeat = DateTimeOffset.Now;
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Sending Heartbeat", DateTime.Now);
            var heartbeat = new GatewayPayload
            {
                OpCode = GatewayOpCode.Heartbeat,
                Data = seq
            };
            var heartbeat_str = JsonConvert.SerializeObject(heartbeat);
            _webSocketClient.SendMessage(heartbeat_str);

            _lastHeartbeat = DateTimeOffset.Now;

            //_waiting_for_ack = true;
            Interlocked.Increment(ref _skippedHeartbeats);
        }

        internal Task SendIdentifyAsync()
        {
            var identify = new GatewayIdentify
            {
                Token = Utilities.GetFormattedToken(this),
                Compress = Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Payload,
                LargeThreshold = Configuration.LargeThreshold,
                ShardInfo = new ShardInfo
                {
                    ShardId = Configuration.ShardId,
                    ShardCount = Configuration.ShardCount
                }
            };
            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Identify,
                Data = identify
            };
            var payloadstr = JsonConvert.SerializeObject(payload);
            _webSocketClient.SendMessage(payloadstr);
            return Task.Delay(0);
        }

        internal Task SendResumeAsync()
        {
            var resume = new GatewayResume
            {
                Token = Utilities.GetFormattedToken(this),
                SessionId = _sessionId,
                SequenceNumber = Volatile.Read(ref _lastSequence)
            };
            var resume_payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Resume,
                Data = resume
            };
            var resumestr = JsonConvert.SerializeObject(resume_payload);

            _webSocketClient.SendMessage(resumestr);
            return Task.Delay(0);
        }

        internal Task SendGuildSyncAsync()
        {
            return SyncGuildsAsync(_guilds.Values.ToArray());
        }
        #endregion

        // LINQ :^)
        internal DiscordChannel InternalGetCachedChannel(ulong channelId)
            => Guilds.Values.SelectMany(xg => xg.Channels)
                .Concat(_privateChannels)
                .FirstOrDefault(xc => xc.Id == channelId);

        internal void UpdateCachedGuild(DiscordGuild newGuild, JArray rawMembers)
        {
            if (!_guilds.ContainsKey(newGuild.Id))
            {
                _guilds[newGuild.Id] = newGuild;
            }

            var guild = _guilds[newGuild.Id];

            if (newGuild._channels != null && newGuild._channels.Any())
            {
                var _c = newGuild._channels.Where(xc => !guild._channels.Any(xxc => xxc.Id == xc.Id));
                foreach (var xc in _c)
                {
                    foreach (var xo in xc._permission_overwrites)
                    {
                        xo.Discord = this;
                        xo._channel_id = xc.Id;
                    }
                }

                guild._channels.AddRange(_c);
            }

            var _e = newGuild._emojis.Where(xe => !guild._emojis.Any(xxe => xxe.Id == xe.Id));
            guild._emojis.AddRange(_e);

            var _r = newGuild._roles.Where(xr => !guild._roles.Any(xxr => xxr.Id == xr.Id));
            foreach (var xr in _r)
            {
                xr._guild_id = guild.Id;
            }
            guild._roles.AddRange(_r);

            if (rawMembers != null)
            {
                foreach (var xj in rawMembers)
                {
                    var xtm = xj.ToObject<TransportMember>();

                    var xu = new DiscordUser(xtm.User) { Discord = this };
                    xu = UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
                    {
                        old.Username = xu.Username;
                        old.Discriminator = xu.Discriminator;
                        old.AvatarHash = xu.AvatarHash;
                        return old;
                    });

                    DiscordMember newmb = new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id };
                    guild._members.AddOrUpdate(newmb, (old, n) =>
                    {
                        old.Nickname = newmb.Nickname;

                        old._role_ids.Clear();
                        old._role_ids.AddRange(newmb._role_ids);

                        old.InvokePropertyChanged("Roles");
                        old.InvokePropertyChanged("Color");
                        old.InvokePropertyChanged("ColorBrush");

                    }, (a, b) => a.Id == b.Id);
                }
            }

            guild.Name = newGuild.Name;
            guild.AfkChannelId = newGuild.AfkChannelId;
            guild.AfkTimeout = newGuild.AfkTimeout;
            guild.DefaultMessageNotifications = newGuild.DefaultMessageNotifications;
            guild.EmbedChannelId = newGuild.EmbedChannelId;
            guild.EmbedEnabled = newGuild.EmbedEnabled;
            guild.Features = newGuild.Features;
            guild.IconHash = newGuild.IconHash;
            guild.MfaLevel = newGuild.MfaLevel;
            guild.OwnerId = newGuild.OwnerId;
            guild.VoiceRegionId = newGuild.VoiceRegionId;
            guild.SplashHash = newGuild.SplashHash;
            guild.VerificationLevel = newGuild.VerificationLevel;
            guild.ExplicitContentFilter = newGuild.ExplicitContentFilter;

            // fields not sent for update:
            // - guild.Channels
            // - voice states
            // - guild.JoinedAt = new_guild.JoinedAt;
            // - guild.Large = new_guild.Large;
            // - guild.MemberCount = Math.Max(new_guild.MemberCount, guild._members.Count);
            // - guild.Unavailable = new_guild.Unavailable;
        }

        internal async Task InternalUpdateGatewayAsync()
        {
            var headers = Utilities.GetBaseHeaders();

            var route = Endpoints.GATEWAY;
            if (Configuration.TokenType == TokenType.Bot)
            {
                route += Endpoints.BOT;
            }

            var bucket = ApiClient.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var request = new RestRequest(this, bucket, url, RestRequestMethod.GET, headers);
            DebugLogger.LogTaskFault(ApiClient.Rest.ExecuteRequestAsync(request), LogLevel.Error, "DSharpPlus", "Error while executing request: ");
            var response = await request.WaitForCompletionAsync().ConfigureAwait(false);

            var jo = JObject.Parse(response.Response);
            _gatewayUri = new Uri(jo.Value<string>("url"));
            if (jo["shards"] != null)
            {
                _shardCount = jo.Value<int>("shards");
            }
        }

        ~DiscordClient()
        {
            Dispose();
        }

        private bool disposed;
        /// <summary>
        /// Disposes your DiscordClient.
        /// </summary>
        public override void Dispose()
        {
            if (disposed)
            {
                return;
            }

            GC.SuppressFinalize(this);

            DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            CurrentUser = null;

            _cancelTokenSource?.Cancel();
            _guilds = null;
            _heartbeatTask = null;
            _extensions = null;
            _privateChannels = null;
            _webSocketClient?.DisconnectAsync(null).ConfigureAwait(false).GetAwaiter().GetResult();
            _webSocketClient?.Dispose();

            disposed = true;
        }

        #region Events
        /// <summary>
        /// Fired whenever an error occurs within an event handler.
        /// </summary>
        public event AsyncEventHandler<ClientErrorEventArgs> ClientErrored
        {
            add { _clientErrored.Register(value); }
            remove { _clientErrored.Unregister(value); }
        }
        private AsyncEvent<ClientErrorEventArgs> _clientErrored;

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> SocketErrored
        {
            add { _socketErrored.Register(value); }
            remove { _socketErrored.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _socketErrored;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add { _socketOpened.Register(value); }
            remove { _socketOpened.Unregister(value); }
        }
        private AsyncEvent _socketOpened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<SocketCloseEventArgs> SocketClosed
        {
            add { _socketClosed.Register(value); }
            remove { _socketClosed.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _socketClosed;

        /// <summary>
        /// Fired when the client enters ready state.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Ready
        {
            add { _ready.Register(value); }
            remove { _ready.Unregister(value); }
        }
        private AsyncEvent<ReadyEventArgs> _ready;

        /// <summary>
        /// Fired whenever a session is resumed.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Resumed
        {
            add { _resumed.Register(value); }
            remove { _resumed.Unregister(value); }
        }
        private AsyncEvent<ReadyEventArgs> _resumed;

        /// <summary>
        /// Fired when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<ChannelCreateEventArgs> ChannelCreated
        {
            add { _channelCreated.Register(value); }
            remove { _channelCreated.Unregister(value); }
        }
        private AsyncEvent<ChannelCreateEventArgs> _channelCreated;

        /// <summary>
        /// Fired when a new direct message channel is created.
        /// </summary>
        public event AsyncEventHandler<DmChannelCreateEventArgs> DmChannelCreated
        {
            add { _dmChannelCreated.Register(value); }
            remove { _dmChannelCreated.Unregister(value); }
        }
        private AsyncEvent<DmChannelCreateEventArgs> _dmChannelCreated;

        /// <summary>
        /// Fired when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add { _channelUpdated.Register(value); }
            remove { _channelUpdated.Unregister(value); }
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channelUpdated;

        /// <summary>
        /// Fired when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add { _channelDeleted.Register(value); }
            remove { _channelDeleted.Unregister(value); }
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channelDeleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add { _dmChannelDeleted.Register(value); }
            remove { _dmChannelDeleted.Unregister(value); }
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dmChannelDeleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add { _channelPinsUpdated.Register(value); }
            remove { _channelPinsUpdated.Unregister(value); }
        }
        private AsyncEvent<ChannelPinsUpdateEventArgs> _channelPinsUpdated;

        /// <summary>
        /// Fired when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add { _guildCreated.Register(value); }
            remove { _guildCreated.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guildCreated;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add { _guildAvailable.Register(value); }
            remove { _guildAvailable.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guildAvailable;

        /// <summary>
        /// Fired when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add { _guildUpdated.Register(value); }
            remove { _guildUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildUpdateEventArgs> _guildUpdated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add { _guildDeleted.Register(value); }
            remove { _guildDeleted.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildDeleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add { _guildUnavailable.Register(value); }
            remove { _guildUnavailable.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildUnavailable;

        /// <summary>
        /// Fired when all guilds finish streaming from Discord.
        /// </summary>
        public event AsyncEventHandler<GuildDownloadCompletedEventArgs> GuildDownloadCompleted
        {
            add { _guildDownloadCompletedEv.Register(value); }
            remove { _guildDownloadCompletedEv.Unregister(value); }
        }
        private AsyncEvent<GuildDownloadCompletedEventArgs> _guildDownloadCompletedEv;

        /// <summary>
        /// Fired when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add { _messageCreated.Register(value); }
            remove { _messageCreated.Unregister(value); }
        }
        private AsyncEvent<MessageCreateEventArgs> _messageCreated;

        /// <summary>
        /// Fired when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdated
        {
            add { _presenceUpdated.Register(value); }
            remove { _presenceUpdated.Unregister(value); }
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presenceUpdated;

        /// <summary>
        /// Fired when a relationship is added (block/pending request)
        /// </summary>
        public event AsyncEventHandler<RelationshipEventArgs> RelationshipAdded
        {
            add { _relationshipAdded.Register(value); }
            remove { _relationshipAdded.Unregister(value); }
        }
        private AsyncEvent<RelationshipEventArgs> _relationshipAdded;

        /// <summary>
        /// Fired when a relationship is removed (unfriend)
        /// </summary>
        public event AsyncEventHandler<RelationshipEventArgs> RelationshipRemoved
        {
            add { _relationshipRemoved.Register(value); }
            remove { _relationshipRemoved.Unregister(value); }
        }
        private AsyncEvent<RelationshipEventArgs> _relationshipRemoved;

        /// <summary>
        /// Fired when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdded
        {
            add { _guildBanAdded.Register(value); }
            remove { _guildBanAdded.Unregister(value); }
        }
        private AsyncEvent<GuildBanAddEventArgs> _guildBanAdded;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add { _guildBanRemoved.Register(value); }
            remove { _guildBanRemoved.Unregister(value); }
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guildBanRemoved;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add { _guildEmojisUpdated.Register(value); }
            remove { _guildEmojisUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guildEmojisUpdated;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add { _guildIntegrationsUpdated.Register(value); }
            remove { _guildIntegrationsUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdated;

        /// <summary>
        /// Fired when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdded
        {
            add { _guildMemberAdded.Register(value); }
            remove { _guildMemberAdded.Unregister(value); }
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guildMemberAdded;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add { _guildMemberRemoved.Register(value); }
            remove { _guildMemberRemoved.Unregister(value); }
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guildMemberRemoved;

        /// <summary>
        /// Fired when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add { _guildMemberUpdated.Register(value); }
            remove { _guildMemberUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guildMemberUpdated;

        /// <summary>
        /// Fired when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add { _guildRoleCreated.Register(value); }
            remove { _guildRoleCreated.Unregister(value); }
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guildRoleCreated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add { _guildRoleUpdated.Register(value); }
            remove { _guildRoleUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guildRoleUpdated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add { _guildRoleDeleted.Register(value); }
            remove { _guildRoleDeleted.Unregister(value); }
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guildRoleDeleted;

        /// <summary>
        /// Fired when message is acknowledged by the user.
        /// </summary>
        public event AsyncEventHandler<MessageAcknowledgeEventArgs> MessageAcknowledged
        {
            add { _messageAcknowledged.Register(value); }
            remove { _messageAcknowledged.Unregister(value); }
        }
        private AsyncEvent<MessageAcknowledgeEventArgs> _messageAcknowledged;

        /// <summary>
        /// Fired when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdated
        {
            add { _messageUpdated.Register(value); }
            remove { _messageUpdated.Unregister(value); }
        }
        private AsyncEvent<MessageUpdateEventArgs> _messageUpdated;

        /// <summary>
        /// Fired when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDeleted
        {
            add { _messageDeleted.Register(value); }
            remove { _messageDeleted.Unregister(value); }
        }
        private AsyncEvent<MessageDeleteEventArgs> _messageDeleted;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add { _messagesBulkDeleted.Register(value); }
            remove { _messagesBulkDeleted.Unregister(value); }
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _messagesBulkDeleted;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStarted
        {
            add { _typingStarted.Register(value); }
            remove { _typingStarted.Unregister(value); }
        }
        private AsyncEvent<TypingStartEventArgs> _typingStarted;

        /// <summary>
        /// Fired when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add { _userSettingsUpdated.Register(value); }
            remove { _userSettingsUpdated.Unregister(value); }
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _userSettingsUpdated;

        /// <summary>
        /// Fired when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdated
        {
            add { _userUpdated.Register(value); }
            remove { _userUpdated.Unregister(value); }
        }
        private AsyncEvent<UserUpdateEventArgs> _userUpdated;

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add { _voiceStateUpdated.Register(value); }
            remove { _voiceStateUpdated.Unregister(value); }
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voiceStateUpdated;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add { _voiceServerUpdated.Register(value); }
            remove { _voiceServerUpdated.Unregister(value); }
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voiceServerUpdated;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add { _guildMembersChunked.Register(value); }
            remove { _guildMembersChunked.Unregister(value); }
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guildMembersChunked;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add { _unknownEvent.Register(value); }
            remove { _unknownEvent.Unregister(value); }
        }
        private AsyncEvent<UnknownEventArgs> _unknownEvent;

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdded
        {
            add { _messageReactionAdded.Register(value); }
            remove { _messageReactionAdded.Unregister(value); }
        }
        private AsyncEvent<MessageReactionAddEventArgs> _messageReactionAdded;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add { _messageReactionRemoved.Register(value); }
            remove { _messageReactionRemoved.Unregister(value); }
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _messageReactionRemoved;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add { _messageReactionsCleared.Register(value); }
            remove { _messageReactionsCleared.Unregister(value); }
        }
        private AsyncEvent<MessageReactionsClearEventArgs> _messageReactionsCleared;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add { _webhooksUpdated.Register(value); }
            remove { _webhooksUpdated.Unregister(value); }
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooksUpdated;

        /// <summary>
        /// Fired on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<HeartbeatEventArgs> Heartbeated
        {
            add { _heartbeated.Register(value); }
            remove { _heartbeated.Unregister(value); }
        }
        private AsyncEvent<HeartbeatEventArgs> _heartbeated;
        private DiscordUserSettings _userSettings;

        internal void EventErrorHandler(string evname, Exception ex)
        {
            DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"An {ex.GetType()} occured in {evname}.", DateTime.Now);
            _clientErrored.InvokeAsync(new ClientErrorEventArgs(this) { EventName = evname, Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void Goof(string evname, Exception ex)
        {
            DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", $"An {ex.GetType()} occured in the exception handler.", DateTime.Now);
        }
        #endregion
    }
}
