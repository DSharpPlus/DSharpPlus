#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using DSharpPlus.Net;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus
{
    /// <summary>
    /// A Discord API wrapper.
    /// </summary>
    public sealed partial class DiscordClient : BaseDiscordClient
    {
        #region Internal Fields/Properties

        internal bool _isShard = false;
        internal RingBuffer<DiscordMessage> MessageCache { get; }

        private List<BaseExtension> _extensions = new List<BaseExtension>();
        private StatusUpdate _status = null;

        private ManualResetEventSlim ConnectionLock { get; } = new ManualResetEventSlim(true);

        #endregion
        
        #region Public Fields/Properties
        /// <summary>
        /// Gets the gateway protocol version.
        /// </summary>
        public int GatewayVersion { get; internal set; }

        /// <summary>
        /// Gets the gateway session information for this client.
        /// </summary>
        public GatewayInfo GatewayInfo { get; internal set; }

        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        public Uri GatewayUri { get; internal set; }

        /// <summary>
        /// Gets the total number of shards the bot is connected to.
        /// </summary>
        public int ShardCount => this.GatewayInfo != null
            ? this.GatewayInfo.ShardCount
            : this.Configuration.ShardCount;

        /// <summary>
        /// Gets the currently connected shard ID.
        /// </summary>
        public int ShardId
            => this.Configuration.ShardId;

        /// <summary>
        /// Gets the intents configured for this client.
        /// </summary>
        public DiscordIntents? Intents
            => this.Configuration.Intents;

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

        internal Dictionary<ulong, DiscordPresence> _presences = new 
            Dictionary<ulong, DiscordPresence>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> _presencesLazy;
        #endregion

        #region Constructor/Internal Setup

        /// <summary>
        /// Initializes a new instance of DiscordClient.
        /// </summary>
        /// <param name="config">Specifies configuration parameters.</param>
        public DiscordClient(DiscordConfiguration config)
            : base(config)
        {
            if (this.Configuration.MessageCacheSize > 0)
            {
                var intents = this.Configuration.Intents;

                if (intents.HasValue)
                    this.MessageCache = intents.Value.HasIntent(DiscordIntents.GuildMessages) || intents.Value.HasIntent(DiscordIntents.DirectMessages)
                        ? new RingBuffer<DiscordMessage>(this.Configuration.MessageCacheSize)
                        : null;
                else
                    this.MessageCache = new RingBuffer<DiscordMessage>(this.Configuration.MessageCacheSize); //This will need to be changed once intents become mandatory.
            }

            this.InternalSetup();

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
        }

        #endregion

        #region Client Extension Methods

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

        #endregion

        #region Public Connection Methods

        /// <summary>
        /// Connects to the gateway
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when an invalid token was provided.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task ConnectAsync(DiscordActivity activity = null, UserStatus? status = null, DateTimeOffset? idlesince = null)
        {
            // Check if connection lock is already set, and set it if it isn't
            if (!this.ConnectionLock.Wait(0))
                throw new InvalidOperationException("This client is already connected.");
            this.ConnectionLock.Set();

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

            if (!this._isShard)
            {
                if (this.Configuration.TokenType != TokenType.Bot)
                    this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
                this.Logger.LogInformation(LoggerEvents.Startup, "DSharpPlus, version {0}", this.VersionString);
            }

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

                    this.Logger.LogError(LoggerEvents.ConnectionFailure, ex, "Connection attempt failed, retrying in {0}s", w / 1000);
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
            => this.InternalReconnectAsync(startNewSession, code: startNewSession ? 1000 : 4002);

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

        #endregion

        #region Public REST Methods
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
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
        /// <param name="id">The id of the channel to get.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordChannel> GetChannelAsync(ulong id)
            => this.InternalGetCachedChannel(id) ?? await this.ApiClient.GetChannelAsync(id).ConfigureAwait(false);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel">Channel to send to.</param>
        /// <param name="content">Message content to send.</param>
        /// <param name="isTTS">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <param name="mentions">Allowed mentions in the message</param>
        /// <returns>The Discord Message that was sent.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the bot does not have the <see cref="Permissions.SendMessages"/> permission if <paramref name="isTTS"/> is false and <see cref="Permissions.SendTtsMessages"/> if <paramref name="tts"/> is true.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
        public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content = null, bool isTTS = false, DiscordEmbed embed = null, IEnumerable<IMention> mentions = null)
            => this.ApiClient.CreateMessageAsync(channel.Id, content, isTTS, embed, mentions);

        /// <summary>
        /// Creates a guild. This requires the bot to be in less than 10 guilds total.
        /// </summary>
        /// <param name="name">Name of the guild.</param>
        /// <param name="region">Voice region of the guild.</param>
        /// <param name="icon">Stream containing the icon for the guild.</param>
        /// <param name="verificationLevel">Verification level for the guild.</param>
        /// <param name="defaultMessageNotifications">Default message notification settings for the guild.</param>
        /// <returns>The created guild.</returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
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
        /// Gets a guild.
        /// <para>Setting <paramref name="withCounts"/> to true will make a REST request.</para>
        /// </summary>
        /// <param name="id">The guild ID to search for.</param>
        /// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
        /// <returns>The requested Guild.</returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the Guild does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
        public async Task<DiscordGuild> GetGuildAsync(ulong id, bool? withCounts = null)
        {
            if (this._guilds.TryGetValue(id, out var guild) && (!withCounts.HasValue || !withCounts.Value))
                return guild;
            
            guild = await this.ApiClient.GetGuildAsync(id, withCounts).ConfigureAwait(false);
            var channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id).ConfigureAwait(false);
            foreach (var channel in channels) guild._channels[channel.Id] = channel;

            return guild;
        }

        /// <summary>
        /// Gets a guild preview
        /// </summary>
        /// <param name="id">The guild ID.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the Guild does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
        public Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong id) 
            => this.ApiClient.GetGuildPreviewAsync(id);

        /// <summary>
        /// Gets an invite.
        /// </summary>
        /// <param name="code">The invite code.</param>
        /// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
        /// <returns>The requested Invite.</returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the invite does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
        public Task<DiscordInvite> GetInviteByCodeAsync(string code, bool? withCounts = null)
            => this.ApiClient.GetInviteAsync(code, withCounts);

        /// <summary>
        /// Gets a list of connections
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
        public Task<IReadOnlyList<DiscordConnection>> GetConnectionsAsync()
            => this.ApiClient.GetUsersConnectionsAsync();

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
        public Task<DiscordWebhook> GetWebhookAsync(ulong id)
            => this.ApiClient.GetWebhookAsync(id);

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
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
        /// <exception cref="Exceptions.NotFoundException">Thrown when the user does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
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

        #region Internal Caching Methods

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
            guild.ApproximateMemberCount = newGuild.ApproximateMemberCount;
            guild.ApproximatePresenceCount = newGuild.ApproximatePresenceCount;
            guild.MaxVideoChannelUsers = newGuild.MaxVideoChannelUsers;
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

        #endregion

        #region Disposal

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
        }

        #endregion
    }
}
