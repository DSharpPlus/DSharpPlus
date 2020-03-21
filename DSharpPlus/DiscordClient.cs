#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public sealed class DiscordClient : BaseDiscordClient
    {
        #region Internal Variables
        internal static DateTimeOffset DiscordEpoch = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

        internal CancellationTokenSource _cancelTokenSource;
        internal CancellationToken _cancelToken;

        internal List<BaseExtension> _extensions = new List<BaseExtension>();

        internal IWebSocketClient _webSocketClient;
        internal PayloadDecompressor _payloadDecompressor;
        internal string _sessionToken = "";
        internal string _sessionId = "";
        internal int _heartbeatInterval;
        internal Task _heartbeatTask;
        internal DateTimeOffset _lastHeartbeat;
        internal long _lastSequence;
        internal int _skippedHeartbeats = 0;
        internal bool _guildDownloadCompleted = false;

        internal RingBuffer<DiscordMessage> MessageCache { get; }
        internal StatusUpdate _status = null;

        #endregion

        #region Public Variables
        /// <summary>
        /// Gets the gateway protocol version.
        /// </summary>
        public int GatewayVersion
            => this._gatewayVersion;

        internal int _gatewayVersion;

        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        public Uri GatewayUri
            => this._gatewayUri;

        internal Uri _gatewayUri;

        /// <summary>
        /// Gets the total number of shards the bot is connected to.
        /// </summary>
        public int ShardCount
            => this.Configuration.ShardCount;

        internal int _shardCount = 1;

        /// <summary>
        /// Gets the currently connected shard ID.
        /// </summary>
        public int ShardId
            => this.Configuration.ShardId;

        /// <summary>
        /// Gets a dictionary of DM channels that have been cached by this client. The dictionary's key is the channel
        /// ID.
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordDmChannel> PrivateChannels { get; }
        internal ConcurrentDictionary<ulong, DiscordDmChannel> _privateChannels = new ConcurrentDictionary<ulong, DiscordDmChannel>();

        /// <summary>
        /// Gets a dictionary of guilds that this client is in. The dictionary's key is the guild ID. Note that the
        /// guild objects in this dictionary will not be filled in if the specific guilds aren't available (the
        /// <see cref="GuildAvailable"/> or <see cref="GuildDownloadCompleted"/> events haven't been fired yet)
        /// </summary>
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }
        internal ConcurrentDictionary<ulong, DiscordGuild> _guilds = new ConcurrentDictionary<ulong, DiscordGuild>();

        /// <summary>
        /// Gets the WS latency for this client.
        /// </summary>
        public int Ping
            => Volatile.Read(ref this._ping);

        private int _ping;

        /// <summary>
        /// Gets the collection of presences held by this client.
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordPresence> Presences
            => this._presencesLazy.Value;

        internal Dictionary<ulong, DiscordPresence> _presences = new Dictionary<ulong, DiscordPresence>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> _presencesLazy;
        #endregion

        #region Connection semaphore
        internal static ConcurrentDictionary<ulong, SocketLock> SocketLocks { get; } = new ConcurrentDictionary<ulong, SocketLock>();
        private ManualResetEventSlim ConnectionLock { get; } = new ManualResetEventSlim(true);
        private ManualResetEventSlim SessionLock { get; } = new ManualResetEventSlim(true);
        #endregion

        /// <summary>
        /// Initializes a new instance of DiscordClient.
        /// </summary>
        /// <param name="config">Specifies configuration parameters.</param>
        public DiscordClient(DiscordConfiguration config)
            : base(config)
        {
            if (this.Configuration.MessageCacheSize > 0)
                this.MessageCache = new RingBuffer<DiscordMessage>(this.Configuration.MessageCacheSize);

            InternalSetup();

            this.Guilds = new ReadOnlyConcurrentDictionary<ulong, DiscordGuild>(this._guilds);
            this.PrivateChannels = new ReadOnlyConcurrentDictionary<ulong, DiscordDmChannel>(this._privateChannels);
        }

        internal void InternalSetup()
        {
            this._clientErrored = new AsyncEvent<ClientErrorEventArgs>(this.Goof, "CLIENT_ERRORED");
            this._socketErrored = new AsyncEvent<SocketErrorEventArgs>(this.Goof, "SOCKET_ERRORED");
            this._socketOpened = new AsyncEvent(this.EventErrorHandler, "SOCKET_OPENED");
            this._socketClosed = new AsyncEvent<SocketCloseEventArgs>(this.EventErrorHandler, "SOCKET_CLOSED");
            this._ready = new AsyncEvent<ReadyEventArgs>(this.EventErrorHandler, "READY");
            this._resumed = new AsyncEvent<ReadyEventArgs>(this.EventErrorHandler, "RESUMED");
            this._channelCreated = new AsyncEvent<ChannelCreateEventArgs>(this.EventErrorHandler, "CHANNEL_CREATED");
            this._dmChannelCreated = new AsyncEvent<DmChannelCreateEventArgs>(this.EventErrorHandler, "DM_CHANNEL_CREATED");
            this._channelUpdated = new AsyncEvent<ChannelUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_UPDATED");
            this._channelDeleted = new AsyncEvent<ChannelDeleteEventArgs>(this.EventErrorHandler, "CHANNEL_DELETED");
            this._dmChannelDeleted = new AsyncEvent<DmChannelDeleteEventArgs>(this.EventErrorHandler, "DM_CHANNEL_DELETED");
            this._channelPinsUpdated = new AsyncEvent<ChannelPinsUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_PINS_UPDATEED");
            this._guildCreated = new AsyncEvent<GuildCreateEventArgs>(this.EventErrorHandler, "GUILD_CREATED");
            this._guildAvailable = new AsyncEvent<GuildCreateEventArgs>(this.EventErrorHandler, "GUILD_AVAILABLE");
            this._guildUpdated = new AsyncEvent<GuildUpdateEventArgs>(this.EventErrorHandler, "GUILD_UPDATED");
            this._guildDeleted = new AsyncEvent<GuildDeleteEventArgs>(this.EventErrorHandler, "GUILD_DELETED");
            this._guildUnavailable = new AsyncEvent<GuildDeleteEventArgs>(this.EventErrorHandler, "GUILD_UNAVAILABLE");
            this._guildDownloadCompletedEv = new AsyncEvent<GuildDownloadCompletedEventArgs>(this.EventErrorHandler, "GUILD_DOWNLOAD_COMPLETED");
            this._inviteCreated = new AsyncEvent<InviteCreateEventArgs>(this.EventErrorHandler, "INVITE_CREATED");
            this._inviteDeleted = new AsyncEvent<InviteDeleteEventArgs>(this.EventErrorHandler, "INVITE_DELETED");
            this._messageCreated = new AsyncEvent<MessageCreateEventArgs>(this.EventErrorHandler, "MESSAGE_CREATED");
            this._presenceUpdated = new AsyncEvent<PresenceUpdateEventArgs>(this.EventErrorHandler, "PRESENCE_UPDATED");
            this._guildBanAdded = new AsyncEvent<GuildBanAddEventArgs>(this.EventErrorHandler, "GUILD_BAN_ADD");
            this._guildBanRemoved = new AsyncEvent<GuildBanRemoveEventArgs>(this.EventErrorHandler, "GUILD_BAN_REMOVED");
            this._guildEmojisUpdated = new AsyncEvent<GuildEmojisUpdateEventArgs>(this.EventErrorHandler, "GUILD_EMOJI_UPDATED");
            this._guildIntegrationsUpdated = new AsyncEvent<GuildIntegrationsUpdateEventArgs>(this.EventErrorHandler, "GUILD_INTEGRATIONS_UPDATED");
            this._guildMemberAdded = new AsyncEvent<GuildMemberAddEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_ADD");
            this._guildMemberRemoved = new AsyncEvent<GuildMemberRemoveEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_REMOVED");
            this._guildMemberUpdated = new AsyncEvent<GuildMemberUpdateEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_UPDATED");
            this._guildRoleCreated = new AsyncEvent<GuildRoleCreateEventArgs>(this.EventErrorHandler, "GUILD_ROLE_CREATED");
            this._guildRoleUpdated = new AsyncEvent<GuildRoleUpdateEventArgs>(this.EventErrorHandler, "GUILD_ROLE_UPDATED");
            this._guildRoleDeleted = new AsyncEvent<GuildRoleDeleteEventArgs>(this.EventErrorHandler, "GUILD_ROLE_DELETED");
            this._messageAcknowledged = new AsyncEvent<MessageAcknowledgeEventArgs>(this.EventErrorHandler, "MESSAGE_ACKNOWLEDGED");
            this._messageUpdated = new AsyncEvent<MessageUpdateEventArgs>(this.EventErrorHandler, "MESSAGE_UPDATED");
            this._messageDeleted = new AsyncEvent<MessageDeleteEventArgs>(this.EventErrorHandler, "MESSAGE_DELETED");
            this._messagesBulkDeleted = new AsyncEvent<MessageBulkDeleteEventArgs>(this.EventErrorHandler, "MESSAGE_BULK_DELETED");
            this._typingStarted = new AsyncEvent<TypingStartEventArgs>(this.EventErrorHandler, "TYPING_STARTED");
            this._userSettingsUpdated = new AsyncEvent<UserSettingsUpdateEventArgs>(this.EventErrorHandler, "USER_SETTINGS_UPDATED");
            this._userUpdated = new AsyncEvent<UserUpdateEventArgs>(this.EventErrorHandler, "USER_UPDATED");
            this._voiceStateUpdated = new AsyncEvent<VoiceStateUpdateEventArgs>(this.EventErrorHandler, "VOICE_STATE_UPDATED");
            this._voiceServerUpdated = new AsyncEvent<VoiceServerUpdateEventArgs>(this.EventErrorHandler, "VOICE_SERVER_UPDATED");
            this._guildMembersChunked = new AsyncEvent<GuildMembersChunkEventArgs>(this.EventErrorHandler, "GUILD_MEMBERS_CHUNKED");
            this._unknownEvent = new AsyncEvent<UnknownEventArgs>(this.EventErrorHandler, "UNKNOWN_EVENT");
            this._messageReactionAdded = new AsyncEvent<MessageReactionAddEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_ADDED");
            this._messageReactionRemoved = new AsyncEvent<MessageReactionRemoveEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_REMOVED");
            this._messageReactionsCleared = new AsyncEvent<MessageReactionsClearEventArgs>(this.EventErrorHandler, "MESSAGE_REACTIONS_CLEARED");
            this._messageReactionRemovedEmoji = new AsyncEvent<MessageReactionRemoveEmojiEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_REMOVED_EMOJI");
            this._webhooksUpdated = new AsyncEvent<WebhooksUpdateEventArgs>(this.EventErrorHandler, "WEBHOOKS_UPDATED");
            this._heartbeated = new AsyncEvent<HeartbeatEventArgs>(this.EventErrorHandler, "HEARTBEATED");

            this._guilds.Clear();

            this._presencesLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordPresence>>(() => new ReadOnlyDictionary<ulong, DiscordPresence>(this._presences));

            if (this.Configuration.UseInternalLogHandler)
                this.DebugLogger.LogMessageReceived += (sender, e) => this.DebugLogger.LogHandler(sender, e);
        }

        /// <summary>
        /// Registers an extension with this client.
        /// </summary>
        /// <param name="ext">Extension to register.</param>
        /// <returns></returns>
        public void AddExtension(BaseExtension ext)
        {
            ext.Setup(this);
            this._extensions.Add(ext);
        }

        /// <summary>
        /// Retrieves a previously-registered extension from this client.
        /// </summary>
        /// <typeparam name="T">Type of extension to retrieve.</typeparam>
        /// <returns>The requested extension.</returns>
        public T GetExtension<T>() where T : BaseExtension
            => this._extensions.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

        /// <summary>
        /// Connects to the gateway
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync(DiscordActivity activity = null, UserStatus? status = null, DateTimeOffset? idlesince = null)
        {
            // Check if connection lock is already set, and set it if it isn't
            if (!this.ConnectionLock.Wait(0))
                throw new InvalidOperationException("This client is already connected.");
            this.ConnectionLock.Reset();

            var w = 7500;
            var i = 5;
            var s = false;
            Exception cex = null;

            if (activity == null && status == null && idlesince == null)
                this._status = null;
            else
            {
                var since_unix = idlesince != null ? (long?)Utilities.GetUnixTime(idlesince.Value) : null;
                this._status = new StatusUpdate()
                {
                    Activity = new TransportActivity(activity),
                    Status = status ?? UserStatus.Online,
                    IdleSince = since_unix,
                    IsAFK = idlesince != null,
                    _activity = activity

                };
            }

            if (this.Configuration.TokenType != TokenType.Bot)
                this.DebugLogger.LogMessage(LogLevel.Warning, "DSharpPlus", "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.", DateTime.Now);
            this.DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", $"DSharpPlus, version {this.VersionString}", DateTime.Now);

            while (i-- > 0 || this.Configuration.ReconnectIndefinitely)
            {
                try
                {
                    await this.InternalConnectAsync().ConfigureAwait(false);
                    s = true;
                    break;
                }
                catch (UnauthorizedException e)
                {
                    FailConnection(this.ConnectionLock);
                    throw new Exception("Authentication failed. Check your token and try again.", e);
                }
                catch (PlatformNotSupportedException)
                {
                    FailConnection(this.ConnectionLock);
                    throw;
                }
                catch (NotImplementedException)
                {
                    FailConnection(this.ConnectionLock);
                    throw;
                }
                catch (Exception ex)
                {
                    FailConnection(null);

                    cex = ex;
                    if (i <= 0 && !this.Configuration.ReconnectIndefinitely) break;

                    this.DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"Connection attempt failed, retrying in {w / 1000}s", DateTime.Now, ex);
                    await Task.Delay(w).ConfigureAwait(false);

                    if (i > 0)
                        w *= 2;
                }
            }

            if (!s && cex != null)
            {
                this.ConnectionLock.Set();
                throw new Exception("Could not connect to Discord.", cex);
            }

            // non-closure, hence args
            void FailConnection(ManualResetEventSlim cl)
            {
                // unlock this (if applicable) so we can let others attempt to connect
                cl?.Set();
            }
        }

        public Task ReconnectAsync(bool startNewSession = false)
        {
            if (startNewSession)
                this._sessionId = "";

            return this._webSocketClient.DisconnectAsync();
        }

        internal Task InternalReconnectAsync()
            => this.ConnectAsync();

        internal async Task InternalConnectAsync()
        {
            SocketLock socketLock = null;
            try
            {
                await this.InternalUpdateGatewayAsync().ConfigureAwait(false);
                await this.InitializeAsync().ConfigureAwait(false);

                socketLock = this.GetSocketLock();
                await socketLock.LockAsync().ConfigureAwait(false);
            }
            catch
            {
                socketLock?.UnlockAfter(TimeSpan.Zero);
                throw;
            }

            if (!this.Presences.ContainsKey(this.CurrentUser.Id))
            {
                this._presences[this.CurrentUser.Id] = new DiscordPresence
                {
                    Discord = this,
                    RawActivity = new TransportActivity(),
                    Activity = new DiscordActivity(),
                    Status = UserStatus.Online,
                    InternalUser = new TransportUser
                    {
                        Id = this.CurrentUser.Id,
                        Username = this.CurrentUser.Username,
                        Discriminator = this.CurrentUser.Discriminator,
                        AvatarHash = this.CurrentUser.AvatarHash
                    }
                };
            }
            else
            {
                var pr = this._presences[this.CurrentUser.Id];
                pr.RawActivity = new TransportActivity();
                pr.Activity = new DiscordActivity();
                pr.Status = UserStatus.Online;
            }

            Volatile.Write(ref this._skippedHeartbeats, 0);

            this._webSocketClient = this.Configuration.WebSocketClientFactory(this.Configuration.Proxy);
            this._payloadDecompressor = this.Configuration.GatewayCompressionLevel != GatewayCompressionLevel.None 
                ? new PayloadDecompressor(this.Configuration.GatewayCompressionLevel)
                : null;

            this._cancelTokenSource = new CancellationTokenSource();
            this._cancelToken = this._cancelTokenSource.Token;

            this._webSocketClient.Connected += SocketOnConnect;
            this._webSocketClient.Disconnected += SocketOnDisconnect;
            this._webSocketClient.MessageReceived += SocketOnMessage;
            this._webSocketClient.ExceptionThrown += SocketOnException;

            var gwuri = new UriBuilder(this._gatewayUri)
            {
                Query = this.Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Stream ? "v=6&encoding=json&compress=zlib-stream" : "v=6&encoding=json"
            };

            await this._webSocketClient.ConnectAsync(gwuri.Uri).ConfigureAwait(false);

            Task SocketOnConnect()
                => this._socketOpened.InvokeAsync();

            async Task SocketOnMessage(SocketMessageEventArgs e)
            {
                string msg = null;
                if (e is SocketTextMessageEventArgs etext)
                {
                    msg = etext.Message;
                }
                else if (e is SocketBinaryMessageEventArgs ebin) // :DDDD
                {
                    using (var ms = new MemoryStream())
                    {
                        if (!this._payloadDecompressor.TryDecompress(new ArraySegment<byte>(ebin.Message), ms))
                        {
                            this.DebugLogger.LogMessage(LogLevel.Error, "Websocket", "Payload decompression failed", DateTime.Now);
                            return;
                        }

                        ms.Position = 0;
                        using (var sr = new StreamReader(ms, Utilities.UTF8))
                            msg = sr.ReadToEnd();
                    }
                }

                try
                {
                    await this.HandleSocketMessageAsync(msg);
                }
                catch (Exception ex)
                {
                    this.DebugLogger.LogMessage(LogLevel.Error, "Websocket", "Socket swallowed an exception", DateTime.Now, ex);
                }
            }

            Task SocketOnException(SocketErrorEventArgs e)
                => this._socketErrored.InvokeAsync(new SocketErrorEventArgs(this) { Exception = e.Exception });

            async Task SocketOnDisconnect(SocketCloseEventArgs e)
            {
                // release session and connection
                this.ConnectionLock.Set();
                this.SessionLock.Set();

                this._cancelTokenSource.Cancel();

                this.DebugLogger.LogMessage(LogLevel.Debug, "Websocket", $"Connection closed. ({e.CloseCode.ToString(CultureInfo.InvariantCulture)}, '{e.CloseMessage}')", DateTime.Now);
                await this._socketClosed.InvokeAsync(new SocketCloseEventArgs(this) { CloseCode = e.CloseCode, CloseMessage = e.CloseMessage }).ConfigureAwait(false);

                if (this.Configuration.AutoReconnect)
                {
                    this.DebugLogger.LogMessage(LogLevel.Critical, "Websocket", $"Socket connection terminated ({e.CloseCode.ToString(CultureInfo.InvariantCulture)}, '{e.CloseMessage}'). Reconnecting.", DateTime.Now);

                    if (this._status == null)
                        await this.ConnectAsync().ConfigureAwait(false);
                    else
                        if (this._status.IdleSince.HasValue)
                            await this.ConnectAsync(this._status._activity, this._status.Status, Utilities.GetDateTimeOffset(this._status.IdleSince.Value)).ConfigureAwait(false);
                        else
                            await this.ConnectAsync(this._status._activity, this._status.Status).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Disconnects from the gateway
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            this.Configuration.AutoReconnect = false;
            if (this._webSocketClient != null)
                await this._webSocketClient.DisconnectAsync().ConfigureAwait(false);
        }

        #region Public Functions
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public async Task<DiscordUser> GetUserAsync(ulong userId)
        {
            if (this.TryGetCachedUserInternal(userId, out var usr))
                return usr;

            usr = await this.ApiClient.GetUserAsync(userId).ConfigureAwait(false);
            usr = this.UserCache.AddOrUpdate(userId, usr, (id, old) =>
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
            => this.InternalGetCachedChannel(id) ?? await this.ApiClient.GetChannelAsync(id).ConfigureAwait(false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="isTTS"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content = null, bool isTTS = false, DiscordEmbed embed = null)
            => this.ApiClient.CreateMessageAsync(channel.Id, content, isTTS, embed);

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
            var iconb64 = Optional.FromNoValue<string>();
            if (icon.HasValue && icon.Value != null)
                using (var imgtool = new ImageTool(icon.Value))
                    iconb64 = imgtool.GetBase64();
            else if (icon.HasValue)
                iconb64 = null;

            return this.ApiClient.CreateGuildAsync(name, region, iconb64, verificationLevel, defaultMessageNotifications);
        }

        /// <summary>
        /// Gets a guild
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> GetGuildAsync(ulong id)
        {
            if (this._guilds.TryGetValue(id, out var guild))
                return guild;
            
            guild = await this.ApiClient.GetGuildAsync(id).ConfigureAwait(false);
            var channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id).ConfigureAwait(false);
            foreach (var channel in channels) guild._channels[channel.Id] = channel;

            return guild;
        }

        /// <summary>
        /// Gets a guild preview
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong id) 
            => this.ApiClient.GetGuildPreviewAsync(id);

        /// <summary>
        /// Gets an invite.
        /// </summary>
        /// <param name="code">The invite code.</param>
        /// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
        /// <returns></returns>
        public Task<DiscordInvite> GetInviteByCodeAsync(string code, bool? withCounts = null)
            => this.ApiClient.GetInviteAsync(code, withCounts);

        /// <summary>
        /// Gets a list of connections
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordConnection>> GetConnectionsAsync()
            => this.ApiClient.GetUsersConnectionsAsync();

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookAsync(ulong id)
            => this.ApiClient.GetWebhookAsync(id);

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token)
            => this.ApiClient.GetWebhookWithTokenAsync(id, token);

        /// <summary>
        /// Updates current user's activity and status.
        /// </summary>
        /// <param name="activity">Activity to set.</param>
        /// <param name="userStatus">Status of the user.</param>
        /// <param name="idleSince">Since when is the client performing the specified activity.</param>
        /// <returns></returns>
        public Task UpdateStatusAsync(DiscordActivity activity = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null)
            => this.InternalUpdateStatusAsync(activity, userStatus, idleSince);

        /// <summary>
        /// Edits current user.
        /// </summary>
        /// <param name="username">New username.</param>
        /// <param name="avatar">New avatar.</param>
        /// <returns></returns>
        public async Task<DiscordUser> UpdateCurrentUserAsync(string username = null, Optional<Stream> avatar = default)
        {
            var av64 = Optional.FromNoValue<string>();
            if (avatar.HasValue && avatar.Value != null)
                using (var imgtool = new ImageTool(avatar.Value))
                    av64 = imgtool.GetBase64();
            else if (avatar.HasValue)
                av64 = null;

            var usr = await this.ApiClient.ModifyCurrentUserAsync(username, av64).ConfigureAwait(false);

            this.CurrentUser.Username = usr.Username;
            this.CurrentUser.Discriminator = usr.Discriminator;
            this.CurrentUser.AvatarHash = usr.AvatarHash;
            return this.CurrentUser;
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
                    this.DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown OP-Code: {((int)payload.OpCode).ToString(CultureInfo.InvariantCulture)}\n{payload.Data}", DateTime.Now);
                    break;
            }
        }

        internal async Task HandleDispatchAsync(GatewayPayload payload)
        {
            if (!(payload.Data is JObject dat))
            {
                DebugLogger.LogMessage(LogLevel.Warning, "Websocket:Dispatch", $"Invalid payload body, you can probably ignore this message: {payload.OpCode}:{payload.EventName}\n{payload.Data}", DateTime.Now);
                return;
            }

            DiscordChannel chn;
            ulong gid;
            ulong cid;
            TransportUser usr;

            switch (payload.EventName.ToLowerInvariant())
            {
                case "ready":
                    var glds = (JArray)dat["guilds"];
                    var dmcs = (JArray)dat["private_channels"];
                    await OnReadyEventAsync(dat.ToObject<ReadyPayload>(), glds, dmcs).ConfigureAwait(false);
                    break;

                case "resumed":
                    await OnResumedAsync().ConfigureAwait(false);
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
                    var ts = (string)dat["last_pin_timestamp"];
                    await this.OnChannelPinsUpdate((ulong?)dat["guild_id"], this.InternalGetCachedChannel(cid), ts != null ? DateTimeOffset.Parse(ts, CultureInfo.InvariantCulture) : default(DateTimeOffset?)).ConfigureAwait(false);
                    break;

                case "gift_code_update": //Not supposed to be dispatched to bots
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
                    await this.OnGuildSyncEventAsync(this._guilds[gid], (bool)dat["large"], (JArray)dat["members"], dat["presences"].ToObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
                    break;

                case "guild_ban_add":
                    usr = dat["user"].ToObject<TransportUser>();
                    gid = (ulong)dat["guild_id"];
                    await OnGuildBanAddEventAsync(usr, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_ban_remove":
                    usr = dat["user"].ToObject<TransportUser>();
                    gid = (ulong)dat["guild_id"];
                    await OnGuildBanRemoveEventAsync(usr, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_emojis_update":
                    gid = (ulong)dat["guild_id"];
                    var ems = dat["emojis"].ToObject<IEnumerable<DiscordEmoji>>();
                    await OnGuildEmojisUpdateEventAsync(this._guilds[gid], ems).ConfigureAwait(false);
                    break;

                case "guild_integrations_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildIntegrationsUpdateEventAsync(this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_member_add":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMemberAddEventAsync(dat.ToObject<TransportMember>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_member_remove":
                    gid = (ulong)dat["guild_id"];
                    if (!this._guilds.ContainsKey(gid))
                    {
                        this.DebugLogger.LogMessage(LogLevel.Error, "Websocket:Dispatch", $"Could not find {gid.ToString(CultureInfo.InvariantCulture)} in guild cache.", DateTime.Now);
                        return;
                    }
                    await OnGuildMemberRemoveEventAsync(dat["user"].ToObject<TransportUser>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_member_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMemberUpdateEventAsync(dat["user"].ToObject<TransportUser>(), this._guilds[gid], dat["roles"].ToObject<IEnumerable<ulong>>(), (string)dat["nick"]).ConfigureAwait(false);
                    break;

                case "guild_members_chunk":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMembersChunkEventAsync(dat["members"].ToObject<IEnumerable<TransportMember>>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_role_create":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleCreateEventAsync(dat["role"].ToObject<DiscordRole>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_role_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleUpdateEventAsync(dat["role"].ToObject<DiscordRole>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_role_delete":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "invite_create":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await OnInviteCreateEventAsync(cid, gid, dat.ToObject<DiscordInvite>()).ConfigureAwait(false);
                    break;

                case "invite_delete":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await OnInviteDeleteEventAsync(cid, gid, dat).ConfigureAwait(false);
                    break;

                case "message_ack":
                    cid = (ulong)dat["channel_id"];
                    var mid = (ulong)dat["message_id"];
                    await OnMessageAckEventAsync(this.InternalGetCachedChannel(cid), mid).ConfigureAwait(false);
                    break;

                case "message_create":
                    await OnMessageCreateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"].ToObject<TransportUser>()).ConfigureAwait(false);
                    break;

                case "message_update":
                    await OnMessageUpdateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"]?.ToObject<TransportUser>()).ConfigureAwait(false);
                    break;

                // delete event does *not* include message object 
                case "message_delete":
                    await OnMessageDeleteEventAsync((ulong)dat["id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "message_delete_bulk":
                    await OnMessageBulkDeleteEventAsync(dat["ids"].ToObject<ulong[]>(), (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "presence_update":
                    await OnPresenceUpdateEventAsync(dat, (JObject)dat["user"]).ConfigureAwait(false);
                    break;

                case "typing_start":
                    cid = (ulong)dat["channel_id"];
                    await OnTypingStartEventAsync((ulong)dat["user_id"], this.InternalGetCachedChannel(cid), (ulong?)dat["guild_id"], Utilities.GetDateTimeOffset((long)dat["timestamp"])).ConfigureAwait(false);
                    break;

                case "user_settings_update":
                    await OnUserSettingsUpdateEventAsync(dat.ToObject<TransportUser>()).ConfigureAwait(false);
                    break;

                case "user_update":
                    await OnUserUpdateEventAsync(dat.ToObject<TransportUser>()).ConfigureAwait(false);
                    break;

                case "voice_state_update":
                    await OnVoiceStateUpdateEventAsync(dat).ConfigureAwait(false);
                    break;

                case "voice_server_update":
                    gid = (ulong)dat["guild_id"];
                    await OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "message_reaction_add":
                    await OnMessageReactionAddAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], dat["emoji"].ToObject<DiscordEmoji>()).ConfigureAwait(false);
                    break;

                case "message_reaction_remove":
                    await OnMessageReactionRemoveAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], dat["emoji"].ToObject<DiscordEmoji>()).ConfigureAwait(false);
                    break;

                case "message_reaction_remove_all":
                    await OnMessageReactionRemoveAllAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "message_reaction_remove_emoji":
                    await OnMessageReactionRemoveEmojiAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong)dat["guild_id"], dat["emoji"]).ConfigureAwait(false);
                    break;

                case "webhooks_update":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await OnWebhooksUpdateAsync(this._guilds[gid].GetChannel(cid), this._guilds[gid]).ConfigureAwait(false);
                    break;

                default:
                    await OnUnknownEventAsync(payload).ConfigureAwait(false);
                    this.DebugLogger.LogMessage(LogLevel.Warning, "Websocket:Dispatch", $"Unknown event: {payload.EventName}\n{payload.Data}", DateTime.Now);
                    break;
            }
        }

        #region Events
        internal async Task OnReadyEventAsync(ReadyPayload ready, JArray rawGuilds, JArray rawDmChannels)
        {
            //ready.CurrentUser.Discord = this;

            var rusr = ready.CurrentUser;
            this.CurrentUser.Username = rusr.Username;
            this.CurrentUser.Discriminator = rusr.Discriminator;
            this.CurrentUser.AvatarHash = rusr.AvatarHash;
            this.CurrentUser.MfaEnabled = rusr.MfaEnabled;
            this.CurrentUser.Verified = rusr.Verified;
            this.CurrentUser.IsBot = rusr.IsBot;

            this._gatewayVersion = ready.GatewayVersion;
            this._sessionId = ready.SessionId;
            var raw_guild_index = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

            this._privateChannels.Clear();
            foreach (var rawChannel in rawDmChannels)
            {
                var channel = rawChannel.ToObject<DiscordDmChannel>();

                channel.Discord = this;

                //xdc._recipients = 
                //    .Select(xtu => this.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this })
                //    .ToList();

                var recips_raw = rawChannel["recipients"].ToObject<IEnumerable<TransportUser>>();
                channel._recipients = new List<DiscordUser>();
                foreach (var xr in recips_raw)
                {
                    var xu = new DiscordUser(xr) { Discord = this };
                    xu = this.UserCache.AddOrUpdate(xr.Id, xu, (id, old) =>
                    {
                        old.Username = xu.Username;
                        old.Discriminator = xu.Discriminator;
                        old.AvatarHash = xu.AvatarHash;
                        return old;
                    });

                    channel._recipients.Add(xu);
                }

                this._privateChannels[channel.Id] = channel;
            }

            this._guilds.Clear();
            foreach (var guild in ready.Guilds)
            {
                guild.Discord = this;

                if (guild._channels == null)
                    guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();

                foreach (var xc in guild.Channels.Values)
                {
                    xc.GuildId = guild.Id;
                    xc.Discord = this;
                    foreach (var xo in xc._permissionOverwrites)
                    {
                        xo.Discord = this;
                        xo._channel_id = xc.Id;
                    }
                }

                if (guild._roles == null)
                    guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();

                foreach (var xr in guild.Roles.Values)
                {
                    xr.Discord = this;
                    xr._guild_id = guild.Id;
                }

                var raw_guild = raw_guild_index[guild.Id];
                var raw_members = (JArray)raw_guild["members"];

                if (guild._members != null)
                    guild._members.Clear();
                else
                    guild._members = new ConcurrentDictionary<ulong, DiscordMember>();

                if (raw_members != null)
                {
                    foreach (var xj in raw_members)
                    {
                        var xtm = xj.ToObject<TransportMember>();

                        var xu = new DiscordUser(xtm.User) {Discord = this};
                        xu = this.UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
                        {
                            old.Username = xu.Username;
                            old.Discriminator = xu.Discriminator;
                            old.AvatarHash = xu.AvatarHash;
                            return old;
                        });

                        guild._members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id };
                    }
                }

                if (guild._emojis == null)
                    guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();

                foreach (var xe in guild.Emojis.Values)
                    xe.Discord = this;

                if (guild._voiceStates == null)
                    guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();

                foreach (var xvs in guild.VoiceStates.Values)
                    xvs.Discord = this;

                this._guilds[guild.Id] = guild;
            }

            await this._ready.InvokeAsync(new ReadyEventArgs(this)).ConfigureAwait(false);
        }

        internal Task OnResumedAsync()
        {
            this.DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", "Session resumed.", DateTime.Now);
            return this._resumed.InvokeAsync(new ReadyEventArgs(this));
        }

        internal async Task OnChannelCreateEventAsync(DiscordChannel channel, JArray rawRecipients)
        {
            channel.Discord = this;

            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var dmChannel = channel as DiscordDmChannel;

                var recips = rawRecipients.ToObject<IEnumerable<TransportUser>>()
                    .Select(xtu => this.TryGetCachedUserInternal(xtu.Id, out var usr) ? usr : new DiscordUser(xtu) { Discord = this });
                dmChannel._recipients = recips.ToList();

                this._privateChannels[dmChannel.Id] = dmChannel;

                await this._dmChannelCreated.InvokeAsync(new DmChannelCreateEventArgs(this) { Channel = dmChannel }).ConfigureAwait(false);
            }
            else
            {
                channel.Discord = this;
                foreach (var xo in channel._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = channel.Id;
                }

                this._guilds[channel.GuildId]._channels[channel.Id] = channel;

                await this._channelCreated.InvokeAsync(new ChannelCreateEventArgs(this) { Channel = channel, Guild = channel.Guild }).ConfigureAwait(false);
            }
        }

        internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
        {
            if (channel == null)
                return;

            channel.Discord = this;

            var gld = channel.Guild;

            var channel_new = this.InternalGetCachedChannel(channel.Id);
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
                    _permissionOverwrites = new List<DiscordOverwrite>(channel_new._permissionOverwrites),
                    Position = channel_new.Position,
                    Topic = channel_new.Topic,
                    Type = channel_new.Type,
                    UserLimit = channel_new.UserLimit,
                    ParentId = channel_new.ParentId,
                    IsNSFW = channel_new.IsNSFW,
                    PerUserRateLimit = channel_new.PerUserRateLimit
                };
            }
            else
            {
                gld._channels[channel.Id] = channel;
            }

            channel_new.Bitrate = channel.Bitrate;
            channel_new.Name = channel.Name;
            channel_new.Position = channel.Position;
            channel_new.Topic = channel.Topic;
            channel_new.UserLimit = channel.UserLimit;
            channel_new.ParentId = channel.ParentId;
            channel_new.IsNSFW = channel.IsNSFW;
            channel_new.PerUserRateLimit = channel.PerUserRateLimit;

            channel_new._permissionOverwrites.Clear();

            foreach (var po in channel._permissionOverwrites)
            {
                po.Discord = this;
                po._channel_id = channel.Id;
            }

            channel_new._permissionOverwrites.AddRange(channel._permissionOverwrites);

            await this._channelUpdated.InvokeAsync(new ChannelUpdateEventArgs(this) { ChannelAfter = channel_new, Guild = gld, ChannelBefore = channel_old }).ConfigureAwait(false);
        }

        internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
        {
            if (channel == null)
                return;

            channel.Discord = this;

            //if (channel.IsPrivate)
            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var dmChannel = channel as DiscordDmChannel;

                if (this._privateChannels.TryRemove(dmChannel.Id, out var cachedDmChannel)) dmChannel = cachedDmChannel;

                await this._dmChannelDeleted.InvokeAsync(new DmChannelDeleteEventArgs(this) { Channel = dmChannel }).ConfigureAwait(false);
            }
            else
            {
                var gld = channel.Guild;

                if (gld._channels.TryRemove(channel.Id, out var cachedChannel)) channel = cachedChannel;

                await this._channelDeleted.InvokeAsync(new ChannelDeleteEventArgs(this) { Channel = channel, Guild = gld }).ConfigureAwait(false);
            }
        }

        internal async Task OnChannelPinsUpdate(ulong? guildId, DiscordChannel channel, DateTimeOffset? lastPinTimestamp)
        {
            if (channel == null)
                return;

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new ChannelPinsUpdateEventArgs(this)
            {
                Guild = guild,
                Channel = channel,
                LastPinTimestamp = lastPinTimestamp
            };
            await this._channelPinsUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            if (presences != null)
            {
                foreach (var xp in presences)
                {
                    xp.Discord = this;
                    xp.GuildId = guild.Id;
                    xp.Activity = new DiscordActivity(xp.RawActivity);
                    if (xp.RawActivities != null)
                    {
                        xp.InternalActivities = new DiscordActivity[xp.RawActivities.Length];
                        for (int i = 0; i < xp.RawActivities.Length; i++)
                            xp.InternalActivities[i] = new DiscordActivity(xp.RawActivities[i]);
                    }
                    this._presences[xp.InternalUser.Id] = xp;
                }
            }

            var exists = this._guilds.TryGetValue(guild.Id, out var foundGuild);

            guild.Discord = this;
            guild.IsUnavailable = false;
            var eventGuild = guild;
            if (exists)
                guild = foundGuild;

            if (guild._channels == null)
                guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
            if (guild._roles == null)
                guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
            if (guild._emojis == null)
                guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
            if (guild._voiceStates == null)
                guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
            if (guild._members == null)
                guild._members = new ConcurrentDictionary<ulong, DiscordMember>();

            this.UpdateCachedGuild(eventGuild, rawMembers);

            guild.JoinedAt = eventGuild.JoinedAt;
            guild.IsLarge = eventGuild.IsLarge;
            guild.MemberCount = Math.Max(eventGuild.MemberCount, guild._members.Count);
            guild.IsUnavailable = eventGuild.IsUnavailable;
            guild.PremiumSubscriptionCount = eventGuild.PremiumSubscriptionCount;
            guild.PremiumTier = eventGuild.PremiumTier;
            guild.Banner = eventGuild.Banner;
            guild.VanityUrlCode = eventGuild.VanityUrlCode;
            guild.Description = eventGuild.Description;

            foreach (var kvp in eventGuild._voiceStates) guild._voiceStates[kvp.Key] = kvp.Value;

            foreach (var xc in guild._channels.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach (var xe in guild._emojis.Values)
                xe.Discord = this;
            foreach (var xvs in guild._voiceStates.Values)
                xvs.Discord = this;
            foreach (var xr in guild._roles.Values)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }

            var old = Volatile.Read(ref this._guildDownloadCompleted);
            var dcompl = this._guilds.Values.All(xg => !xg.IsUnavailable);
            Volatile.Write(ref this._guildDownloadCompleted, dcompl);

            if (exists)
                await this._guildAvailable.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild }).ConfigureAwait(false);
            else
                await this._guildCreated.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild }).ConfigureAwait(false);

            if (dcompl && !old)
                await this._guildDownloadCompletedEv.InvokeAsync(new GuildDownloadCompletedEventArgs(this)).ConfigureAwait(false);
        }

        internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray rawMembers)
        {
            DiscordGuild oldGuild;

            if (!this._guilds.ContainsKey(guild.Id))
            {
                this._guilds[guild.Id] = guild;
                oldGuild = null;
            }
            else
            {
                var gld = this._guilds[guild.Id];

                oldGuild = new DiscordGuild
                {
                    Discord = gld.Discord,
                    Name = gld.Name,
                    AfkChannelId = gld.AfkChannelId,
                    AfkTimeout = gld.AfkTimeout,
                    DefaultMessageNotifications = gld.DefaultMessageNotifications,
                    EmbedChannelId = gld.EmbedChannelId,
                    EmbedEnabled = gld.EmbedEnabled,
                    ExplicitContentFilter = gld.ExplicitContentFilter,
                    Features = gld.Features,
                    IconHash = gld.IconHash,
                    Id = gld.Id,
                    IsLarge = gld.IsLarge,
                    IsSynced = gld.IsSynced,
                    IsUnavailable = gld.IsUnavailable,
                    JoinedAt = gld.JoinedAt,
                    MemberCount = gld.MemberCount,
                    MaxMembers = gld.MaxMembers,
                    MaxPresences = gld.MaxPresences,
                    DiscoverySplashHash = gld.DiscoverySplashHash,
                    PreferredLocale = gld.PreferredLocale,
                    MfaLevel = gld.MfaLevel,
                    OwnerId = gld.OwnerId,
                    SplashHash = gld.SplashHash,
                    SystemChannelId = gld.SystemChannelId,
                    SystemChannelFlags = gld.SystemChannelFlags,
                    WidgetEnabled = gld.WidgetEnabled,
                    WidgetChannelId = gld.WidgetChannelId,
                    VerificationLevel = gld.VerificationLevel,
                    RulesChannelId = gld.RulesChannelId,
                    PublicUpdatesChannelId = gld.PublicUpdatesChannelId,
                    VoiceRegionId = gld.VoiceRegionId,   
                    _channels = new ConcurrentDictionary<ulong, DiscordChannel>(),
                    _emojis = new ConcurrentDictionary<ulong, DiscordEmoji>(),
                    _members = new ConcurrentDictionary<ulong, DiscordMember>(),
                    _roles = new ConcurrentDictionary<ulong, DiscordRole>(),
                    _voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>()
                };

                foreach (var kvp in gld._channels) oldGuild._channels[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._emojis) oldGuild._emojis[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._roles) oldGuild._roles[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._voiceStates) oldGuild._voiceStates[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._members) oldGuild._members[kvp.Key] = kvp.Value;
            }

            guild.Discord = this;
            guild.IsUnavailable = false;
            var eventGuild = guild;
            guild = this._guilds[eventGuild.Id];

            if (guild._channels == null)
                guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
            if (guild._roles == null)
                guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
            if (guild._emojis == null)
                guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
            if (guild._voiceStates == null)
                guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
            if (guild._members == null)
                guild._members = new ConcurrentDictionary<ulong, DiscordMember>();

            this.UpdateCachedGuild(eventGuild, rawMembers);

            foreach (var xc in guild._channels.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach (var xe in guild._emojis.Values)
                xe.Discord = this;
            foreach (var xvs in guild._voiceStates.Values)
                xvs.Discord = this;
            foreach (var xr in guild._roles.Values)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }

            await this._guildUpdated.InvokeAsync(new GuildUpdateEventArgs(this) { GuildBefore = oldGuild, GuildAfter = guild }).ConfigureAwait(false);
        }

        internal async Task OnGuildDeleteEventAsync(DiscordGuild guild, JArray rawMembers)
        {
            if (guild.IsUnavailable)
            {
                if (!this._guilds.TryGetValue(guild.Id, out var gld))
                    return;

                gld.IsUnavailable = true;

                await this._guildUnavailable.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = guild, Unavailable = true }).ConfigureAwait(false);
            }
            else
            {
                if (!this._guilds.TryRemove(guild.Id, out var gld))
                    return;

                await this._guildDeleted.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = gld }).ConfigureAwait(false);
            }
        }

        internal async Task OnGuildSyncEventAsync(DiscordGuild guild, bool isLarge, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            presences = presences.Select(xp => { xp.Discord = this; xp.Activity = new DiscordActivity(xp.RawActivity); return xp; });
            foreach (var xp in presences)
                this._presences[xp.InternalUser.Id] = xp;

            guild.IsSynced = true;
            guild.IsLarge = isLarge;

            this.UpdateCachedGuild(guild, rawMembers);

            await this._guildAvailable.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild }).ConfigureAwait(false);
        }

        internal async Task OnPresenceUpdateEventAsync(JObject rawPresence, JObject rawUser)
        {
            var uid = (ulong)rawUser["id"];
            DiscordPresence old = null;

            if (this._presences.TryGetValue(uid, out var presence))
            {
                old = new DiscordPresence(presence);
                DiscordJson.PopulateObject(rawPresence, presence);

                if (rawPresence["game"] == null || rawPresence["game"].Type == JTokenType.Null)
                    presence.RawActivity = null;

                if (presence.Activity != null)
                    presence.Activity.UpdateWith(presence.RawActivity);
                else
                    presence.Activity = new DiscordActivity(presence.RawActivity);
            }
            else
            {
                presence = rawPresence.ToObject<DiscordPresence>();
                presence.Discord = this;
                presence.Activity = new DiscordActivity(presence.RawActivity);
                this._presences[presence.InternalUser.Id] = presence;
            }

            // reuse arrays / avoid linq (this is a hot zone)
            if (presence.Activities == null || rawPresence["activities"] == null)
            {
                presence.InternalActivities = Array.Empty<DiscordActivity>();
            }
            else
            {
                if (presence.InternalActivities.Length != presence.RawActivities.Length)
                    presence.InternalActivities = new DiscordActivity[presence.RawActivities.Length];

                for (var i = 0; i < presence.InternalActivities.Length; i++)
                    presence.InternalActivities[i] = new DiscordActivity(presence.RawActivities[i]);
            }

            if (this.UserCache.TryGetValue(uid, out var usr))
            {
                if (old != null)
                {
                    old.InternalUser.Username = usr.Username;
                    old.InternalUser.Discriminator = usr.Discriminator;
                    old.InternalUser.AvatarHash = usr.AvatarHash;
                }

                if (rawUser["username"] is object)
                    usr.Username = (string)rawUser["username"];
                if (rawUser["discriminator"] is object)
                    usr.Discriminator = (string)rawUser["discriminator"];
                if (rawUser["avatar"] is object)
                    usr.AvatarHash = (string)rawUser["avatar"];

                presence.InternalUser.Username = usr.Username;
                presence.InternalUser.Discriminator = usr.Discriminator;
                presence.InternalUser.AvatarHash = usr.AvatarHash;
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
            await this._presenceUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildBanAddEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanAddEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildBanAdded.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildBanRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanRemoveEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildBanRemoved.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
        {
            var oldEmojis = new ConcurrentDictionary<ulong, DiscordEmoji>(guild._emojis);
            guild._emojis.Clear();

            foreach (var emoji in newEmojis)
            {
                emoji.Discord = this;
                guild._emojis[emoji.Id] = emoji;
            }

            var ea = new GuildEmojisUpdateEventArgs(this)
            {
                Guild = guild,
                EmojisAfter = guild.Emojis,
                EmojisBefore = new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(oldEmojis)
            };
            await this._guildEmojisUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild guild)
        {
            var ea = new GuildIntegrationsUpdateEventArgs(this)
            {
                Guild = guild
            };
            await this._guildIntegrationsUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
        {
            var usr = new DiscordUser(member.User) { Discord = this };
            usr = this.UserCache.AddOrUpdate(member.User.Id, usr, (id, old) =>
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

            guild._members[mbr.Id] = mbr;
            guild.MemberCount++;

            var ea = new GuildMemberAddEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildMemberAdded.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            if (!guild._members.TryRemove(user.Id, out var mbr))
                mbr = new DiscordMember(new DiscordUser(user)) {Discord = this, _guild_id = guild.Id};
            guild.MemberCount--;

            var ea = new GuildMemberRemoveEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildMemberRemoved.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMemberUpdateEventAsync(TransportUser user, DiscordGuild guild, IEnumerable<ulong> roles, string nick)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };

            var nick_old = mbr.Nickname;
            var roles_old = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles));

            mbr.Nickname = nick;
            mbr._role_ids.Clear();
            mbr._role_ids.AddRange(roles);

            var ea = new GuildMemberUpdateEventArgs(this)
            {
                Guild = guild,
                Member = mbr,

                NicknameAfter = mbr.Nickname,
                RolesAfter = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles)),

                NicknameBefore = nick_old,
                RolesBefore = roles_old
            };
            await this._guildMemberUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildRoleCreateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            role.Discord = this;
            role._guild_id = guild.Id;

            guild._roles[role.Id] = role;

            var ea = new GuildRoleCreateEventArgs(this)
            {
                Guild = guild,
                Role = role
            };
            await this._guildRoleCreated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            var newRole = guild.GetRole(role.Id);
            var oldRole = new DiscordRole
            {
                _guild_id = guild.Id,
                _color = newRole._color,
                Discord = this,
                IsHoisted = newRole.IsHoisted,
                Id = newRole.Id,
                IsManaged = newRole.IsManaged,
                IsMentionable = newRole.IsMentionable,
                Name = newRole.Name,
                Permissions = newRole.Permissions,
                Position = newRole.Position
            };

            newRole._guild_id = guild.Id;
            newRole._color = role._color;
            newRole.IsHoisted = role.IsHoisted;
            newRole.IsManaged = role.IsManaged;
            newRole.IsMentionable = role.IsMentionable;
            newRole.Name = role.Name;
            newRole.Permissions = role.Permissions;
            newRole.Position = role.Position;

            var ea = new GuildRoleUpdateEventArgs(this)
            {
                Guild = guild,
                RoleAfter = newRole,
                RoleBefore = oldRole
            };
            await this._guildRoleUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildRoleDeleteEventAsync(ulong roleId, DiscordGuild guild)
        {
            if (!guild._roles.TryRemove(roleId, out var role))
                throw new InvalidOperationException("Attempted to delete a nonexistent role.");

            var ea = new GuildRoleDeleteEventArgs(this)
            {
                Guild = guild,
                Role = role
            };
            await this._guildRoleDeleted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnInviteCreateEventAsync(ulong channelId, ulong guildId, DiscordInvite invite)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId);

            invite.Discord = this;            

            guild._invites[invite.Code] = invite;

            var ea = new InviteCreateEventArgs(this)
            {
                Client = this,
                Channel = channel,
                Guild = guild,
                Invite = invite
            };
            await this._inviteCreated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnInviteDeleteEventAsync(ulong channelId, ulong guildId, JToken dat)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId);

            if (!guild._invites.TryRemove(dat["code"].ToString(), out var invite))
            {
                invite = dat.ToObject<DiscordInvite>();
                invite.IsRevoked = true;
                invite.Discord = this;
            }

            var ea = new InviteDeleteEventArgs(this)
            {
                Client = this,
                Channel = channel,
                Guild = guild,
                Invite = invite
            };
            await this._inviteDeleted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageAckEventAsync(DiscordChannel chn, ulong messageId)
        {
            DiscordMessage msg = null;
            if (this.MessageCache?.TryGet(xm => xm.Id == messageId && xm.ChannelId == chn.Id, out msg) != true)
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = chn.Id,
                    Discord = this,
                };

            await this._messageAcknowledged.InvokeAsync(new MessageAcknowledgeEventArgs(this) { Message = msg }).ConfigureAwait(false);
        }

        internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author)
        {
            message.Discord = this;

            if (message.Channel == null)
                this.DebugLogger.LogMessage(LogLevel.Warning, "Event", "Could not find channel last message belonged to.", DateTime.Now);
            else
                message.Channel.LastMessageId = message.Id;

            var guild = message.Channel?.Guild;

            var usr = new DiscordUser(author) { Discord = this };
            usr = this.UserCache.AddOrUpdate(author.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (guild != null)
            {
                if (!guild.Members.TryGetValue(author.Id, out var mbr))
                    mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
                message.Author = mbr;
            }
            else
            {
                message.Author = usr;
            }

            var mentionedUsers = new List<DiscordUser>();
            var mentionedRoles = guild != null ? new List<DiscordRole>() : null;
            var mentionedChannels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentionedUsers = Utilities.GetUserMentions(message).Select(xid => guild._members.TryGetValue(xid, out var member) ? member : new DiscordUser { Id = xid, Discord = this }).Cast<DiscordUser>().ToList();
                    mentionedRoles = Utilities.GetRoleMentions(message).Select(xid => guild.GetRole(xid)).ToList();
                    mentionedChannels = Utilities.GetChannelMentions(message).Select(xid => guild.GetChannel(xid)).ToList();
                }
                else
                {
                    mentionedUsers = Utilities.GetUserMentions(message).Select(this.GetCachedOrEmptyUserInternal).ToList();
                }
            }

            message._mentionedUsers = mentionedUsers;
            message._mentionedRoles = mentionedRoles;
            message._mentionedChannels = mentionedChannels;

            if (message._reactions == null)
                message._reactions = new List<DiscordReaction>();
            foreach (var xr in message._reactions)
                xr.Emoji.Discord = this;

            if (this.Configuration.MessageCacheSize > 0 && message.Channel != null)
                this.MessageCache.Add(message);

            MessageCreateEventArgs ea = new MessageCreateEventArgs(this)
            {
                Message = message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentionedUsers),
                MentionedRoles = mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(mentionedRoles) : null,
                MentionedChannels = mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(mentionedChannels) : null
            };
            await this._messageCreated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author)
        {
            DiscordGuild guild;

            message.Discord = this;
            var event_message = message;

            DiscordMessage oldmsg = null;
            if (this.Configuration.MessageCacheSize == 0 || !this.MessageCache.TryGet(xm => xm.Id == event_message.Id && xm.ChannelId == event_message.ChannelId, out message))
            {
                message = event_message;
                guild = message.Channel?.Guild;

                if (author != null)
                {
                    var usr = new DiscordUser(author) { Discord = this };
                    usr = this.UserCache.AddOrUpdate(author.Id, usr, (id, old) =>
                    {
                        old.Username = usr.Username;
                        old.Discriminator = usr.Discriminator;
                        old.AvatarHash = usr.AvatarHash;
                        return old;
                    });

                    if (guild != null)
                    {
                        if (!guild.Members.TryGetValue(author.Id, out var mbr))
                            mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
                        message.Author = mbr;
                    }
                    else
                    {
                        message.Author = usr;
                    }
                }

                if (message._reactions == null)
                    message._reactions = new List<DiscordReaction>();
                foreach (var xr in message._reactions)
                    xr.Emoji.Discord = this;
            }
            else
            {
                oldmsg = new DiscordMessage(message);

                guild = message.Channel?.Guild;
                message.EditedTimestampRaw = event_message.EditedTimestampRaw;
                if (event_message.Content != null)
                    message.Content = event_message.Content;
                message._embeds.Clear();
                message._embeds.AddRange(event_message._embeds);
                message.Pinned = event_message.Pinned;
                message.IsTTS = event_message.IsTTS;
            }

            var mentioned_users = new List<DiscordUser>();
            var mentioned_roles = guild != null ? new List<DiscordRole>() : null;
            var mentioned_channels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentioned_users = Utilities.GetUserMentions(message).Select(xid => guild._members.TryGetValue(xid, out var member) ? member : null).Cast<DiscordUser>().ToList();
                    mentioned_roles = Utilities.GetRoleMentions(message).Select(xid => guild.GetRole(xid)).ToList();
                    mentioned_channels = Utilities.GetChannelMentions(message).Select(xid => guild.GetChannel(xid)).ToList();
                }
                else
                {
                    mentioned_users = Utilities.GetUserMentions(message).Select(this.GetCachedOrEmptyUserInternal).ToList();
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
            await this._messageUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageDeleteEventAsync(ulong messageId, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId);
            var guild = this.InternalGetCachedGuild(guildId);

            if (channel == null || this.Configuration.MessageCacheSize == 0 ||
                !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this,
                };
            }

            if (this.Configuration.MessageCacheSize > 0)
                this.MessageCache.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channelId);

            var ea = new MessageDeleteEventArgs(this)
            {
                Channel = channel,
                Message = msg,
                Guild = guild
            };
            await this._messageDeleted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageBulkDeleteEventAsync(ulong[] messageIds, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId);

            var msgs = new List<DiscordMessage>(messageIds.Length);
            foreach (var messageId in messageIds)
            {
                if (channel == null || this.Configuration.MessageCacheSize == 0 ||
                    !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
                {
                    msg = new DiscordMessage
                    {
                        Id = messageId,
                        ChannelId = channelId,
                        Discord = this,
                    };
                }
                if (this.Configuration.MessageCacheSize > 0)
                    this.MessageCache.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channelId);
                msgs.Add(msg);
            }

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new MessageBulkDeleteEventArgs(this)
            {
                Channel = channel,
                Messages = new ReadOnlyCollection<DiscordMessage>(msgs),
                Guild = guild
            };
            await this._messagesBulkDeleted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnTypingStartEventAsync(ulong userId, DiscordChannel channel, ulong? guildId, DateTimeOffset started)
        {
            if (channel == null)
                return;

            if (!this.UserCache.TryGetValue(userId, out var user))
                user = new DiscordUser { Id = userId, Discord = this };

            if (channel.Guild != null)
                user = channel.Guild.Members.TryGetValue(userId, out var member)
                    ? member
                    : new DiscordMember(user) { Discord = this, _guild_id = channel.GuildId };

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new TypingStartEventArgs(this)
            {
                Channel = channel,
                User = user,
                Guild = guild,
                StartedAt = started
            };
            await this._typingStarted.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnUserSettingsUpdateEventAsync(TransportUser user)
        {
            var usr = new DiscordUser(user) { Discord = this };

            var ea = new UserSettingsUpdateEventArgs(this)
            {
                User = usr
            };
            await this._userSettingsUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnUserUpdateEventAsync(TransportUser user)
        {
            var usr_old = new DiscordUser
            {
                AvatarHash = this.CurrentUser.AvatarHash,
                Discord = this,
                Discriminator = this.CurrentUser.Discriminator,
                Email = this.CurrentUser.Email,
                Id = this.CurrentUser.Id,
                IsBot = this.CurrentUser.IsBot,
                MfaEnabled = this.CurrentUser.MfaEnabled,
                Username = this.CurrentUser.Username,
                Verified = this.CurrentUser.Verified
            };

            this.CurrentUser.AvatarHash = user.AvatarHash;
            this.CurrentUser.Discriminator = user.Discriminator;
            this.CurrentUser.Email = user.Email;
            this.CurrentUser.Id = user.Id;
            this.CurrentUser.IsBot = user.IsBot;
            this.CurrentUser.MfaEnabled = user.MfaEnabled;
            this.CurrentUser.Username = user.Username;
            this.CurrentUser.Verified = user.Verified;

            var ea = new UserUpdateEventArgs(this)
            {
                UserAfter = this.CurrentUser,
                UserBefore = usr_old
            };
            await this._userUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnVoiceStateUpdateEventAsync(JObject raw)
        {
            var gid = (ulong)raw["guild_id"];
            var uid = (ulong)raw["user_id"];
            var gld = this._guilds[gid];

            var vstateHasNew = gld._voiceStates.TryGetValue(uid, out var vstateNew);
            DiscordVoiceState vstateOld;
            if (vstateHasNew)
            {
                vstateOld = new DiscordVoiceState(vstateNew);
                DiscordJson.PopulateObject(raw, vstateNew);
            }
            else
            {
                vstateOld = null;
                vstateNew = raw.ToObject<DiscordVoiceState>();
                vstateNew.Discord = this;
                gld._voiceStates[vstateNew.UserId] = vstateNew;
            }

            if (gld._members.TryGetValue(uid, out var mbr))
            {
                mbr.IsMuted = vstateNew.IsServerMuted;
                mbr.IsDeafened = vstateNew.IsServerDeafened;
            }

            var ea = new VoiceStateUpdateEventArgs(this)
            {
                Guild = vstateNew.Guild,
                Channel = vstateNew.Channel,
                User = vstateNew.User,
                SessionId = vstateNew.SessionId,

                Before = vstateOld,
                After = vstateNew
            };
            await this._voiceStateUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, DiscordGuild guild)
        {
            var ea = new VoiceServerUpdateEventArgs(this)
            {
                Endpoint = endpoint,
                VoiceToken = token,
                Guild = guild
            };
            await this._voiceServerUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnGuildMembersChunkEventAsync(IEnumerable<TransportMember> members, DiscordGuild guild)
        {
            var mbrs = new HashSet<DiscordMember>();

            foreach (var xtm in members)
            {
                var mbr = new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id };
                guild._members[mbr.Id] = mbr;
                mbrs.Add(mbr);
            }
            guild.MemberCount = guild._members.Count;

            var ea = new GuildMembersChunkEventArgs(this)
            {
                Guild = guild,
                Members = new ReadOnlySet<DiscordMember>(mbrs)
            };
            await this._guildMembersChunked.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnUnknownEventAsync(GatewayPayload payload)
        {
            var ea = new UnknownEventArgs(this) { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
            await this._unknownEvent.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, DiscordEmoji emoji)
        {
            var channel = this.InternalGetCachedChannel(channelId);

            var guild = this.InternalGetCachedGuild(guildId);

            emoji.Discord = this;

            if (!this.UserCache.TryGetValue(userId, out var usr))
                usr = new DiscordUser { Id = userId, Discord = this };

            if (guild != null)
                usr = channel.Guild.Members.TryGetValue(userId, out var member)
                    ? member
                    : new DiscordMember(usr) { Discord = this, _guild_id = channel.GuildId };

            if (channel == null || this.Configuration.MessageCacheSize == 0 ||
                !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
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
                    IsMe = this.CurrentUser.Id == userId
                });
            }
            else
            {
                react.Count++;
                react.IsMe |= this.CurrentUser.Id == userId;
            }

            var ea = new MessageReactionAddEventArgs(this)
            {
                Message = msg,
                User = usr,
                Guild = guild,
                Emoji = emoji
            };
            await this._messageReactionAdded.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, DiscordEmoji emoji)
        {
            var channel = this.InternalGetCachedChannel(channelId);

            emoji.Discord = this;

            if (!this.UserCache.TryGetValue(userId, out var usr))
                usr = new DiscordUser { Id = userId, Discord = this };

            if (channel?.Guild != null)
                usr = channel.Guild.Members.TryGetValue(userId, out var member)
                    ? member
                    : new DiscordMember(usr) { Discord = this, _guild_id = channel.GuildId };

            if (channel == null || this.Configuration.MessageCacheSize == 0 ||
                !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            var react = msg._reactions?.FirstOrDefault(xr => xr.Emoji == emoji);
            if (react != null)
            {
                react.Count--;
                react.IsMe &= this.CurrentUser.Id != userId;

                if (msg._reactions != null && react.Count <= 0) // shit happens
                    for (var i = 0; i < msg._reactions.Count; i++)
                        if (msg._reactions[i].Emoji == emoji)
                        {
                            msg._reactions.RemoveAt(i);
                            break;
                        }
            }

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new MessageReactionRemoveEventArgs(this)
            {
                Message = msg,
                User = usr,
                Guild = guild,
                Emoji = emoji
            };
            await this._messageReactionRemoved.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId);

            if (channel == null || this.Configuration.MessageCacheSize == 0 ||
                !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            msg._reactions?.Clear();

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new MessageReactionsClearEventArgs(this)
            {
                Message = msg,
                Guild = guild
            };

            await this._messageReactionsCleared.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnMessageReactionRemoveEmojiAsync(ulong messageId, ulong channelId, ulong guildId, JToken dat)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId);

            if (channel == null || this.Configuration.MessageCacheSize == 0 ||
                !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            var partialEmoji = dat.ToObject<DiscordEmoji>();

            if (!guild._emojis.TryGetValue(partialEmoji.Id, out var emoji))
            {
                emoji = partialEmoji;
                emoji.Discord = this;
            }

            msg._reactions?.RemoveAll(r => r.Emoji.Equals(emoji));

            var ea = new MessageReactionRemoveEmojiEventArgs(this)
            {
                Channel = channel,
                Guild = guild,
                Message = msg,
                Emoji = emoji
            };

            await this._messageReactionRemovedEmoji.InvokeAsync(ea).ConfigureAwait(false);
        }

        internal async Task OnWebhooksUpdateAsync(DiscordChannel channel, DiscordGuild guild)
        {
            var ea = new WebhooksUpdateEventArgs(this)
            {
                Channel = channel,
                Guild = guild
            };
            await this._webhooksUpdated.InvokeAsync(ea).ConfigureAwait(false);
        }
        #endregion

        internal async Task OnHeartbeatAsync(long seq)
        {
            this.DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received Heartbeat - Sending Ack.", DateTime.Now);
            await SendHeartbeatAsync(seq).ConfigureAwait(false);
        }

        internal async Task OnReconnectAsync()
        {
            this.DebugLogger.LogMessage(LogLevel.Info, "Websocket", "Received OP 7 - Reconnect.", DateTime.Now);

            await ReconnectAsync().ConfigureAwait(false);
        }

        internal async Task OnInvalidateSessionAsync(bool data)
        {
            // begin a session if one is not open already
            if (this.SessionLock.Wait(0))
                this.SessionLock.Reset();

            // we are sending a fresh resume/identify, so lock the socket
            var socketLock = this.GetSocketLock();
            await socketLock.LockAsync().ConfigureAwait(false);
            socketLock.UnlockAfter(TimeSpan.FromSeconds(5));

            if (data)
            {
                this.DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received true in OP 9 - Waiting a few seconds and sending resume again.", DateTime.Now);
                await Task.Delay(6000).ConfigureAwait(false);
                await SendResumeAsync().ConfigureAwait(false);
            }
            else
            {
                this.DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received false in OP 9 - Starting a new session.", DateTime.Now);
                this._sessionId = "";
                await SendIdentifyAsync(this._status).ConfigureAwait(false);
            }
        }

        internal async Task OnHelloAsync(GatewayHello hello)
        {
            this.DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received OP 10 (HELLO) - Trying to either resume or identify.", DateTime.Now);

            if (this.SessionLock.Wait(0))
            {
                this.SessionLock.Reset();
                this.GetSocketLock().UnlockAfter(TimeSpan.FromSeconds(5));
            }
            else
            {
                this.DebugLogger.LogMessage(LogLevel.Warning, "DSharpPlus", "Session start attempt was made while another session is active", DateTime.Now);
                return;
            }

            Interlocked.CompareExchange(ref this._skippedHeartbeats, 0, 0);
            this._heartbeatInterval = hello.HeartbeatInterval;
            this._heartbeatTask = new Task(StartHeartbeating, _cancelToken, TaskCreationOptions.LongRunning);
            this._heartbeatTask.Start();

            if (this._sessionId == "")
                await SendIdentifyAsync(_status).ConfigureAwait(false);
            else
                await SendResumeAsync().ConfigureAwait(false);
        }

        internal async Task OnHeartbeatAckAsync()
        {
            Interlocked.Decrement(ref this._skippedHeartbeats);

            var ping = (int)(DateTime.Now - this._lastHeartbeat).TotalMilliseconds;

            this.DebugLogger.LogMessage(LogLevel.Debug, "Websocket", $"Received WebSocket Heartbeat Ack. Ping: {ping.ToString(CultureInfo.InvariantCulture)}ms", DateTime.Now);

            Volatile.Write(ref this._ping, ping);

            var args = new HeartbeatEventArgs(this)
            {
                Ping = this.Ping,
                Timestamp = DateTimeOffset.Now
            };

            await _heartbeated.InvokeAsync(args).ConfigureAwait(false);
        }

        //internal async Task StartHeartbeatingAsync()
        internal void StartHeartbeating()
        {
            this.DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Starting Heartbeat.", DateTime.Now);
            var token = this._cancelToken;
            try
            {
                while (true)
                {
                    SendHeartbeatAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    Task.Delay(_heartbeatInterval, _cancelToken).ConfigureAwait(false).GetAwaiter().GetResult();
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException) { }
        }

        internal async Task InternalUpdateStatusAsync(DiscordActivity activity, UserStatus? userStatus, DateTimeOffset? idleSince)
        {
            if (activity != null && activity.Name != null && activity.Name.Length > 128)
                throw new Exception("Game name can't be longer than 128 characters!");

            var since_unix = idleSince != null ? (long?)Utilities.GetUnixTime(idleSince.Value) : null;
            var act = activity ?? new DiscordActivity();

            var status = new StatusUpdate
            {
                Activity = new TransportActivity(act),
                IdleSince = since_unix,
                IsAFK = idleSince != null,
                Status = userStatus ?? UserStatus.Online
            };

            // Solution to have status persist between sessions
            this._status = status;
            var status_update = new GatewayPayload
            {
                OpCode = GatewayOpCode.StatusUpdate,
                Data = status
            };
            var statusstr = JsonConvert.SerializeObject(status_update);

            await this._webSocketClient.SendMessageAsync(statusstr).ConfigureAwait(false);

            if (!this._presences.ContainsKey(this.CurrentUser.Id))
            {
                this._presences[this.CurrentUser.Id] = new DiscordPresence
                {
                    Discord = this,
                    Activity = act,
                    Status = userStatus ?? UserStatus.Online,
                    InternalUser = new TransportUser { Id = this.CurrentUser.Id }
                };
            }
            else
            {
                var pr = this._presences[this.CurrentUser.Id];
                pr.Activity = act;
                pr.Status = userStatus ?? pr.Status;
            }
        }

        internal Task SendHeartbeatAsync()
        {
            var _last_heartbeat = DateTimeOffset.Now;
            var _sequence = (long)(_last_heartbeat - DiscordEpoch).TotalMilliseconds;

            return this.SendHeartbeatAsync(_sequence);
        }

        internal async Task SendHeartbeatAsync(long seq)
        {
            var more_than_5 = Volatile.Read(ref this._skippedHeartbeats) > 5;
            var guilds_comp = Volatile.Read(ref this._guildDownloadCompleted);
            if (guilds_comp && more_than_5)
            {
                this.DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", "More than 5 heartbeats were skipped. Issuing reconnect.", DateTime.Now);
                await ReconnectAsync().ConfigureAwait(false);
                return;
            }
            else if (!guilds_comp && more_than_5)
            {
                this.DebugLogger.LogMessage(LogLevel.Warning, "DSharpPlus", "More than 5 heartbeats were skipped while the guild download is running.", DateTime.Now);
            }

            Volatile.Write(ref this._lastSequence, seq);
            var _last_heartbeat = DateTimeOffset.Now;
            this.DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Sending Heartbeat.", DateTime.Now);
            var heartbeat = new GatewayPayload
            {
                OpCode = GatewayOpCode.Heartbeat,
                Data = seq
            };
            var heartbeat_str = JsonConvert.SerializeObject(heartbeat);
            await this._webSocketClient.SendMessageAsync(heartbeat_str).ConfigureAwait(false);

            this._lastHeartbeat = DateTimeOffset.Now;

            Interlocked.Increment(ref this._skippedHeartbeats);
        }

        internal async Task SendIdentifyAsync(StatusUpdate status)
        {
            var identify = new GatewayIdentify
            {
                Token = Utilities.GetFormattedToken(this),
                Compress = this.Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Payload,
                LargeThreshold = this.Configuration.LargeThreshold,
                ShardInfo = new ShardInfo
                {
                    ShardId = this.Configuration.ShardId,
                    ShardCount = this.Configuration.ShardCount
                },
                Presence = status
            };
            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Identify,
                Data = identify
            };
            var payloadstr = JsonConvert.SerializeObject(payload);
            await this._webSocketClient.SendMessageAsync(payloadstr).ConfigureAwait(false);
        }

        internal async Task SendResumeAsync()
        {
            var resume = new GatewayResume
            {
                Token = Utilities.GetFormattedToken(this),
                SessionId = this._sessionId,
                SequenceNumber = Volatile.Read(ref this._lastSequence)
            };
            var resume_payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Resume,
                Data = resume
            };
            var resumestr = JsonConvert.SerializeObject(resume_payload);

            await this._webSocketClient.SendMessageAsync(resumestr).ConfigureAwait(false);
        }
        #endregion

        internal DiscordChannel InternalGetCachedChannel(ulong channelId)
        {
            if (this._privateChannels.TryGetValue(channelId, out var foundDmChannel))
                return foundDmChannel;

            foreach (var guild in this.Guilds.Values)
                if (guild.Channels.TryGetValue(channelId, out var foundChannel))
                    return foundChannel;

            return null;
        }

        internal DiscordGuild InternalGetCachedGuild(ulong? guildId)
        {
            if (guildId.HasValue)
                if (this._guilds.TryGetValue(guildId.Value, out var foundGuild))
                    return foundGuild;

            return null;
        }

        internal void UpdateCachedGuild(DiscordGuild newGuild, JArray rawMembers)
        {
            if (!this._guilds.ContainsKey(newGuild.Id))
                this._guilds[newGuild.Id] = newGuild;

            var guild = this._guilds[newGuild.Id];

            if (newGuild._channels != null && newGuild._channels.Count > 0)
            {
                foreach (var channel in newGuild._channels.Values)
                {
                    if (guild._channels.TryGetValue(channel.Id, out _)) continue;

                    foreach (var overwrite in channel._permissionOverwrites)
                    {
                        overwrite.Discord = this;
                        overwrite._channel_id = channel.Id;
                    }

                    guild._channels[channel.Id] = channel;
                }
            }

            foreach (var newEmoji in newGuild._emojis.Values)
                _ = guild._emojis.GetOrAdd(newEmoji.Id, _ => newEmoji);

            if (rawMembers != null)
            {
                guild._members.Clear();

                foreach (var xj in rawMembers)
                {
                    var xtm = xj.ToObject<TransportMember>();

                    var xu = new DiscordUser(xtm.User) { Discord = this };
                    _ = this.UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
                    {
                        old.Username = xu.Username;
                        old.Discriminator = xu.Discriminator;
                        old.AvatarHash = xu.AvatarHash;
                        old.PremiumType = xu.PremiumType;
                        return old;
                    });

                    guild._members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id };
                }
            }

            foreach (var role in newGuild._roles.Values)
            {
                if (guild._roles.TryGetValue(role.Id, out _)) continue;

                role._guild_id = guild.Id;
                guild._roles[role.Id] = role;
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
            guild.WidgetEnabled = newGuild.WidgetEnabled;
            guild.WidgetChannelId = newGuild.WidgetChannelId;
            guild.ExplicitContentFilter = newGuild.ExplicitContentFilter;
            guild.PremiumTier = newGuild.PremiumTier;
            guild.PremiumSubscriptionCount = newGuild.PremiumSubscriptionCount;
            guild.Banner = newGuild.Banner;
            guild.Description = newGuild.Description;
            guild.VanityUrlCode = newGuild.VanityUrlCode;
            guild.Banner = newGuild.Banner;
            guild.SystemChannelId = newGuild.SystemChannelId;
            guild.SystemChannelFlags = newGuild.SystemChannelFlags;
            guild.DiscoverySplashHash = newGuild.DiscoverySplashHash;
            guild.MaxMembers = newGuild.MaxMembers;
            guild.MaxPresences = newGuild.MaxPresences;
            guild.PreferredLocale = newGuild.PreferredLocale;
            guild.RulesChannelId = newGuild.RulesChannelId;
            guild.PublicUpdatesChannelId = newGuild.PublicUpdatesChannelId;

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
                route += Endpoints.BOT;
            var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path);
            var request = new RestRequest(this, bucket, url, RestRequestMethod.GET, headers);
            DebugLogger.LogTaskFault(this.ApiClient.Rest.ExecuteRequestAsync(request), LogLevel.Error, "DSharpPlus", "Error while executing request: ");
            var response = await request.WaitForCompletionAsync().ConfigureAwait(false);

            var jo = JObject.Parse(response.Response);
            this._gatewayUri = new Uri(jo.Value<string>("url"));
            if (jo["shards"] != null)
                this._shardCount = jo.Value<int>("shards");
        }

        private SocketLock GetSocketLock()
            => SocketLocks.GetOrAdd(this.CurrentApplication.Id, appId => new SocketLock(appId));

        ~DiscordClient()
        {
            this.Dispose();
        }

        private bool _disposed;
        /// <summary>
        /// Disposes your DiscordClient.
        /// </summary>
        public override void Dispose()
        {
            if (_disposed)
                return;

            this._disposed = true;
            GC.SuppressFinalize(this);

            this.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            this.CurrentUser = null;

            var extensions = this._extensions; // prevent _extensions being modified during dispose
            this._extensions = null;
            foreach (var extension in extensions)
                if (extension is IDisposable disposable) 
                    disposable.Dispose();

            try
            {
                this._cancelTokenSource?.Cancel();
                this._cancelTokenSource?.Dispose();
            }
            catch { }

            this._guilds = null;
            this._heartbeatTask = null;
            this._privateChannels = null;
            this._webSocketClient.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            this._webSocketClient.Dispose();
        }

        #region Events
        /// <summary>
        /// Fired whenever an error occurs within an event handler.
        /// </summary>
        public event AsyncEventHandler<ClientErrorEventArgs> ClientErrored
        {
            add => this._clientErrored.Register(value);
            remove => this._clientErrored.Unregister(value);
        }
        private AsyncEvent<ClientErrorEventArgs> _clientErrored;

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> SocketErrored
        {
            add => this._socketErrored.Register(value);
            remove => this._socketErrored.Unregister(value);
        }
        private AsyncEvent<SocketErrorEventArgs> _socketErrored;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add => this._socketOpened.Register(value);
            remove => this._socketOpened.Unregister(value);
        }
        private AsyncEvent _socketOpened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<SocketCloseEventArgs> SocketClosed
        {
            add => this._socketClosed.Register(value);
            remove => this._socketClosed.Unregister(value);
        }
        private AsyncEvent<SocketCloseEventArgs> _socketClosed;

        /// <summary>
        /// Fired when the client enters ready state.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Ready
        {
            add => this._ready.Register(value);
            remove => this._ready.Unregister(value);
        }
        private AsyncEvent<ReadyEventArgs> _ready;

        /// <summary>
        /// Fired whenever a session is resumed.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Resumed
        {
            add => this._resumed.Register(value);
            remove => this._resumed.Unregister(value);
        }
        private AsyncEvent<ReadyEventArgs> _resumed;

        /// <summary>
        /// Fired when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<ChannelCreateEventArgs> ChannelCreated
        {
            add => this._channelCreated.Register(value);
            remove => this._channelCreated.Unregister(value);
        }
        private AsyncEvent<ChannelCreateEventArgs> _channelCreated;

        /// <summary>
        /// Fired when a new direct message channel is created.
        /// </summary>
        public event AsyncEventHandler<DmChannelCreateEventArgs> DmChannelCreated
        {
            add => this._dmChannelCreated.Register(value);
            remove => this._dmChannelCreated.Unregister(value);
        }
        private AsyncEvent<DmChannelCreateEventArgs> _dmChannelCreated;

        /// <summary>
        /// Fired when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add => this._channelUpdated.Register(value);
            remove => this._channelUpdated.Unregister(value);
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channelUpdated;

        /// <summary>
        /// Fired when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add => this._channelDeleted.Register(value);
            remove => this._channelDeleted.Unregister(value);
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channelDeleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add => this._dmChannelDeleted.Register(value);
            remove => this._dmChannelDeleted.Unregister(value);
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dmChannelDeleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add => this._channelPinsUpdated.Register(value);
            remove => this._channelPinsUpdated.Unregister(value);
        }
        private AsyncEvent<ChannelPinsUpdateEventArgs> _channelPinsUpdated;

        /// <summary>
        /// Fired when the user joins a new guild.
        /// </summary>
        /// <remarks>[alias="GuildJoined"][alias="JoinedGuild"]</remarks>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add => this._guildCreated.Register(value);
            remove => this._guildCreated.Unregister(value);
        }
        private AsyncEvent<GuildCreateEventArgs> _guildCreated;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add => this._guildAvailable.Register(value);
            remove => this._guildAvailable.Unregister(value);
        }
        private AsyncEvent<GuildCreateEventArgs> _guildAvailable;

        /// <summary>
        /// Fired when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add => this._guildUpdated.Register(value);
            remove => this._guildUpdated.Unregister(value);
        }
        private AsyncEvent<GuildUpdateEventArgs> _guildUpdated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add => this._guildDeleted.Register(value);
            remove => this._guildDeleted.Unregister(value);
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildDeleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add => this._guildUnavailable.Register(value);
            remove => this._guildUnavailable.Unregister(value);
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildUnavailable;

        /// <summary>
        /// Fired when all guilds finish streaming from Discord.
        /// </summary>
        public event AsyncEventHandler<GuildDownloadCompletedEventArgs> GuildDownloadCompleted
        {
            add => this._guildDownloadCompletedEv.Register(value);
            remove => this._guildDownloadCompletedEv.Unregister(value);
        }
        private AsyncEvent<GuildDownloadCompletedEventArgs> _guildDownloadCompletedEv;

        /// <summary>
        /// Fired when an invite is created.
        /// </summary>
        public event AsyncEventHandler<InviteCreateEventArgs> InviteCreated
        {
            add => this._inviteCreated.Register(value);
            remove => this._inviteCreated.Unregister(value);
        }
        private AsyncEvent<InviteCreateEventArgs> _inviteCreated;

        /// <summary>
        /// Fired when an invite is deleted.
        /// </summary>
        public event AsyncEventHandler<InviteDeleteEventArgs> InviteDeleted
        {
            add => this._inviteDeleted.Register(value);
            remove => this._inviteDeleted.Unregister(value);
        }
        private AsyncEvent<InviteDeleteEventArgs> _inviteDeleted;
        /// <summary>
        /// Fired when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add => this._messageCreated.Register(value);
            remove => this._messageCreated.Unregister(value);
        }
        private AsyncEvent<MessageCreateEventArgs> _messageCreated;

        /// <summary>
        /// Fired when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdated
        {
            add => this._presenceUpdated.Register(value);
            remove => this._presenceUpdated.Unregister(value);
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presenceUpdated;

        /// <summary>
        /// Fired when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdded
        {
            add => this._guildBanAdded.Register(value);
            remove => this._guildBanAdded.Unregister(value);
        }
        private AsyncEvent<GuildBanAddEventArgs> _guildBanAdded;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add => this._guildBanRemoved.Register(value);
            remove => this._guildBanRemoved.Unregister(value);
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guildBanRemoved;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add => this._guildEmojisUpdated.Register(value);
            remove => this._guildEmojisUpdated.Unregister(value);
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guildEmojisUpdated;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add => this._guildIntegrationsUpdated.Register(value);
            remove => this._guildIntegrationsUpdated.Unregister(value);
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdated;

        /// <summary>
        /// Fired when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdded
        {
            add => this._guildMemberAdded.Register(value);
            remove => this._guildMemberAdded.Unregister(value);
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guildMemberAdded;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add => this._guildMemberRemoved.Register(value);
            remove => this._guildMemberRemoved.Unregister(value);
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guildMemberRemoved;

        /// <summary>
        /// Fired when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add => this._guildMemberUpdated.Register(value);
            remove => this._guildMemberUpdated.Unregister(value);
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guildMemberUpdated;

        /// <summary>
        /// Fired when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add => this._guildRoleCreated.Register(value);
            remove => this._guildRoleCreated.Unregister(value);
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guildRoleCreated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add => this._guildRoleUpdated.Register(value);
            remove => this._guildRoleUpdated.Unregister(value);
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guildRoleUpdated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add => this._guildRoleDeleted.Register(value);
            remove => this._guildRoleDeleted.Unregister(value);
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guildRoleDeleted;

        /// <summary>
        /// Fired when message is acknowledged by the user.
        /// </summary>
        public event AsyncEventHandler<MessageAcknowledgeEventArgs> MessageAcknowledged
        {
            add => this._messageAcknowledged.Register(value);
            remove => this._messageAcknowledged.Unregister(value);
        }
        private AsyncEvent<MessageAcknowledgeEventArgs> _messageAcknowledged;

        /// <summary>
        /// Fired when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdated
        {
            add => this._messageUpdated.Register(value);
            remove => this._messageUpdated.Unregister(value);
        }
        private AsyncEvent<MessageUpdateEventArgs> _messageUpdated;

        /// <summary>
        /// Fired when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDeleted
        {
            add => this._messageDeleted.Register(value);
            remove => this._messageDeleted.Unregister(value);
        }
        private AsyncEvent<MessageDeleteEventArgs> _messageDeleted;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add => this._messagesBulkDeleted.Register(value);
            remove => this._messagesBulkDeleted.Unregister(value);
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _messagesBulkDeleted;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStarted
        {
            add => this._typingStarted.Register(value);
            remove => this._typingStarted.Unregister(value);
        }
        private AsyncEvent<TypingStartEventArgs> _typingStarted;

        /// <summary>
        /// Fired when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add => this._userSettingsUpdated.Register(value);
            remove => this._userSettingsUpdated.Unregister(value);
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _userSettingsUpdated;

        /// <summary>
        /// Fired when properties about the current user change.
        /// </summary>
        /// <remarks>
        /// NB: This event only applies for changes to the <b>current user</b>, the client that is connected to Discord.
        /// </remarks>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdated
        {
            add => this._userUpdated.Register(value);
            remove => this._userUpdated.Unregister(value);
        }
        private AsyncEvent<UserUpdateEventArgs> _userUpdated;

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add => this._voiceStateUpdated.Register(value);
            remove => this._voiceStateUpdated.Unregister(value);
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voiceStateUpdated;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add => this._voiceServerUpdated.Register(value);
            remove => this._voiceServerUpdated.Unregister(value);
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voiceServerUpdated;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add => this._guildMembersChunked.Register(value);
            remove => this._guildMembersChunked.Unregister(value);
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guildMembersChunked;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add => this._unknownEvent.Register(value);
            remove => this._unknownEvent.Unregister(value);
        }
        private AsyncEvent<UnknownEventArgs> _unknownEvent;

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdded
        {
            add => this._messageReactionAdded.Register(value);
            remove => this._messageReactionAdded.Unregister(value);
        }
        private AsyncEvent<MessageReactionAddEventArgs> _messageReactionAdded;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add => this._messageReactionRemoved.Register(value);
            remove => this._messageReactionRemoved.Unregister(value);
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _messageReactionRemoved;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add => this._messageReactionsCleared.Register(value);
            remove => this._messageReactionsCleared.Unregister(value);
        }
        private AsyncEvent<MessageReactionsClearEventArgs> _messageReactionsCleared;

        /// <summary>
        /// Fired when all reactions of a specific reaction are removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEmojiEventArgs> MessageReactionRemovedEmoji
        {
            add => this._messageReactionRemovedEmoji.Register(value);
            remove => this._messageReactionRemovedEmoji.Unregister(value);
        }
        private AsyncEvent<MessageReactionRemoveEmojiEventArgs> _messageReactionRemovedEmoji;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add => this._webhooksUpdated.Register(value);
            remove => this._webhooksUpdated.Unregister(value);
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooksUpdated;

        /// <summary>
        /// Fired on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<HeartbeatEventArgs> Heartbeated
        {
            add => this._heartbeated.Register(value);
            remove => this._heartbeated.Unregister(value);
        }
        private AsyncEvent<HeartbeatEventArgs> _heartbeated;

        internal void EventErrorHandler(string evname, Exception ex)
        {
            this.DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"An {ex.GetType()} occurred in {evname}.", DateTime.Now, ex);
            this._clientErrored.InvokeAsync(new ClientErrorEventArgs(this) { EventName = evname, Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void Goof(string evname, Exception ex)
        {
            this.DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", $"An {ex.GetType()} occurred in the exception handler.", DateTime.Now, ex);
        }
        #endregion
    }
}
