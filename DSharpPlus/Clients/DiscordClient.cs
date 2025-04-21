using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus.Clients;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Gateway;
using DSharpPlus.Net.InboundWebhooks;
using DSharpPlus.Net.Models;
using DSharpPlus.Net.Serialization;
using DSharpPlus.Net.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

namespace DSharpPlus;

/// <summary>
/// A Discord API wrapper.
/// </summary>
public sealed partial class DiscordClient : BaseDiscordClient
{
    internal static readonly DateTimeOffset discordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly ConcurrentDictionary<ulong, SocketLock> socketLocks = [];

    internal bool isShard = false;
    internal IMessageCacheProvider? MessageCache { get; }
    private readonly IClientErrorHandler errorHandler;
    private readonly IShardOrchestrator orchestrator;
    private readonly ChannelReader<GatewayPayload> eventReader;
    private readonly ChannelReader<DiscordWebhookEvent> webhookEventReader;
    private readonly ChannelReader<DiscordHttpInteractionPayload> interactionEventReader;
    private readonly IEventDispatcher dispatcher;

    private readonly ConcurrentDictionary<Int128, Channel<GuildMembersChunkedEventArgs>> guildMembersChunkedEvents = [];

    private StatusUpdate? status = null;
    private readonly string token;

    private readonly ManualResetEventSlim connectionLock = new(true);

    /// <summary>
    /// Gets the service provider used within this Discord application.
    /// </summary>
    public IServiceProvider ServiceProvider { get; internal set; }

    /// <summary>
    /// Gets whether this client is connected to the gateway.
    /// </summary>
    public bool AllShardsConnected
        => this.orchestrator.AllShardsConnected;

    /// <summary>
    /// Gets a dictionary of DM channels that have been cached by this client. The dictionary's key is the channel
    /// ID.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordDmChannel> PrivateChannels => this.privateChannels;
    internal ConcurrentDictionary<ulong, DiscordDmChannel> privateChannels = new();

    /// <summary>
    /// Gets a dictionary of guilds that this client is in. The dictionary's key is the guild ID. Note that the
    /// guild objects in this dictionary will not be filled in if the specific guilds aren't available (the
    /// <c>GuildAvailable</c> or <c>GuildDownloadCompleted</c> events haven't been fired yet)
    /// </summary>
    public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds => this.guilds;
    internal ConcurrentDictionary<ulong, DiscordGuild> guilds = new();

    /// <summary>
    /// Gets the latency in the connection to a specific guild.
    /// </summary>
    public TimeSpan GetConnectionLatency(ulong guildId)
        => this.orchestrator.GetConnectionLatency(guildId);

    /// <summary>
    /// Gets the collection of presences held by this client.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordPresence> Presences
        => this.presences;

    internal Dictionary<ulong, DiscordPresence> presences = [];

    [ActivatorUtilitiesConstructor]
    public DiscordClient
    (
        ILogger<DiscordClient> logger,
        DiscordApiClient apiClient,
        IMessageCacheProvider messageCacheProvider,
        IServiceProvider serviceProvider,
        IEventDispatcher eventDispatcher,
        IClientErrorHandler errorHandler,
        IOptions<DiscordConfiguration> configuration,
        IOptions<TokenContainer> token,
        IShardOrchestrator shardOrchestrator,
        IOptions<GatewayClientOptions> gatewayOptions,

        [FromKeyedServices("DSharpPlus.Gateway.EventChannel")]
        Channel<GatewayPayload> eventChannel,

        [FromKeyedServices("DSharpPlus.Webhooks.EventChannel")]
        Channel<DiscordWebhookEvent> webhookEventChannel,

        [FromKeyedServices("DSharpPlus.Interactions.EventChannel")]
        Channel<DiscordHttpInteractionPayload> interactionEventChannel
    )
        : base()
    {
        this.Logger = logger;
        this.MessageCache = messageCacheProvider;
        this.ServiceProvider = serviceProvider;
        this.ApiClient = apiClient;
        this.errorHandler = errorHandler;
        this.Configuration = configuration.Value;
        this.token = token.Value.GetToken();
        this.orchestrator = shardOrchestrator;
        this.eventReader = eventChannel.Reader;
        this.dispatcher = eventDispatcher;
        this.webhookEventReader = webhookEventChannel.Reader;
        this.interactionEventReader = interactionEventChannel.Reader;

        this.ApiClient.SetClient(this);
        this.Intents = gatewayOptions.Value.Intents;

        this.guilds.Clear();
    }

    #region Public Connection Methods

    /// <summary>
    /// Connects to the gateway
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when an invalid token was provided.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ConnectAsync(DiscordActivity activity = null, DiscordUserStatus? status = null, DateTimeOffset? idlesince = null)
    {
        // method checks if its already initialized
        await InitializeAsync();

        // Check if connection lock is already set, and set it if it isn't
        if (!this.connectionLock.Wait(0))
        {
            throw new InvalidOperationException("This client is already connected.");
        }

        this.connectionLock.Set();

        if (activity == null && status == null && idlesince == null)
        {
            this.status = null;
        }
        else
        {
            long? since_unix = idlesince != null ? Utilities.GetUnixTime(idlesince.Value) : null;
            this.status = new StatusUpdate()
            {
                Activity = new TransportActivity(activity),
                Status = status ?? DiscordUserStatus.Online,
                IdleSince = since_unix,
                IsAFK = idlesince != null,
                activity = activity
            };
        }

        this.Logger.LogInformation(LoggerEvents.Startup, "DSharpPlus; version {Version}", this.VersionString);

        await this.dispatcher.DispatchAsync(this, new ClientStartedEventArgs());

        _ = ReceiveGatewayEventsAsync();
        _ = ReceiveWebhookEventsAsync();
        _ = ReceiveInteractionEventsAsync();

        await this.orchestrator.StartAsync(activity, status, idlesince);
    }

    /// <summary>
    /// Sends a raw payload to the gateway. This method is not recommended for use unless you know what you're doing.
    /// </summary>
    /// <param name="opCode">The opcode to send to the Discord gateway.</param>
    /// <param name="data">The data to serialize.</param>
    /// <param name="guildId">The guild this payload originates from. Pass 0 for shard-independent payloads.</param>
    /// <remarks>
    /// This method should not be used unless you know what you're doing. Instead, look towards the other
    /// explicitly implemented methods which come with client-side validation.
    /// </remarks>
    /// <returns>A task representing the payload being sent.</returns>
    [Experimental("DSP0004")]
    public async Task SendPayloadAsync(GatewayOpCode opCode, object? data, ulong guildId)
    {
        GatewayPayload payload = new()
        {
            OpCode = opCode,
            Data = data
        };

        string payloadString = DiscordJson.SerializeObject(payload);
        await this.orchestrator.SendOutboundEventAsync(Encoding.UTF8.GetBytes(payloadString), guildId);
    }

    /// <summary>
    /// Reconnects all shards to the gateway.
    /// </summary>
    public async Task ReconnectAsync()
        => await this.orchestrator.ReconnectAsync();

    /// <summary>
    /// Disconnects from the gateway
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync()
    {
        await this.orchestrator.StopAsync();
        await this.dispatcher.DispatchAsync(this, new ClientStoppedEventArgs());
    }

    #endregion

    #region Public REST Methods

    /// <summary>
    /// Gets a sticker.
    /// </summary>
    /// <param name="stickerId">The ID of the sticker.</param>
    /// <returns>The specified sticker</returns>
    public async Task<DiscordMessageSticker> GetStickerAsync(ulong stickerId)
        => await this.ApiClient.GetStickerAsync(stickerId);

    /// <summary>
    /// Gets a collection of sticker packs that may be used by nitro users.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordMessageStickerPack>> GetStickerPacksAsync()
        => await this.ApiClient.GetStickerPacksAsync();

    /// <summary>
    /// Gets a user
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="updateCache">Whether to always make a REST request and update cache. Passing true will update the user, updating stale properties such as <see cref="DiscordUser.BannerHash"/>.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordUser> GetUserAsync(ulong userId, bool updateCache = false)
    {
        if (!updateCache && TryGetCachedUserInternal(userId, out DiscordUser? usr))
        {
            return usr;
        }

        usr = await this.ApiClient.GetUserAsync(userId);

        // See BaseDiscordClient.UpdateUser for why this is done like this.
        this.UserCache.AddOrUpdate(userId, usr, (_, _) => usr);

        return usr;
    }

    /// <summary>
    /// Gets a channel
    /// </summary>
    /// <param name="id">The ID of the channel to get.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordChannel> GetChannelAsync(ulong id)
        => InternalGetCachedThread(id) ?? InternalGetCachedChannel(id) ?? await this.ApiClient.GetChannelAsync(id);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content)
        => await this.ApiClient.CreateMessageAsync(channel.Id, content, embeds: null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, DiscordEmbed embed)
        => await this.ApiClient.CreateMessageAsync(channel.Id, null, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content, DiscordEmbed embed)
        => await this.ApiClient.CreateMessageAsync(channel.Id, content, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="builder">The Discord Message builder.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.SendMessages"/> permission if TTS is false and <see cref="DiscordPermission.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, DiscordMessageBuilder builder)
        => await this.ApiClient.CreateMessageAsync(channel.Id, builder);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="action">The Discord Message builder.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.SendMessages"/> permission if TTS is false and <see cref="DiscordPermission.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, Action<DiscordMessageBuilder> action)
    {
        DiscordMessageBuilder builder = new();
        action(builder);

        return await this.ApiClient.CreateMessageAsync(channel.Id, builder);
    }

    /// <summary>
    /// Creates a guild. This requires the bot to be in less than 10 guilds total.
    /// </summary>
    /// <param name="name">Name of the guild.</param>
    /// <param name="region">Voice region of the guild.</param>
    /// <param name="icon">Stream containing the icon for the guild.</param>
    /// <param name="verificationLevel">Verification level for the guild.</param>
    /// <param name="defaultMessageNotifications">Default message notification settings for the guild.</param>
    /// <param name="systemChannelFlags">System channel flags fopr the guild.</param>
    /// <returns>The created guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuild> CreateGuildAsync
    (
        string name,
        string? region = null,
        Optional<Stream> icon = default,
        DiscordVerificationLevel? verificationLevel = null,
        DiscordDefaultMessageNotifications? defaultMessageNotifications = null,
        DiscordSystemChannelFlags? systemChannelFlags = null
    )
    {
        Optional<string?> iconb64 = Optional.FromNoValue<string?>();

        if (icon.HasValue && icon.Value != null)
        {
            using InlineMediaTool imgtool = new(icon.Value);
            iconb64 = imgtool.GetBase64();
        }
        else if (icon.HasValue)
        {
            iconb64 = null;
        }

        return await this.ApiClient.CreateGuildAsync(name, region, iconb64, verificationLevel, defaultMessageNotifications, systemChannelFlags);
    }

    /// <summary>
    /// Creates a guild from a template. This requires the bot to be in less than 10 guilds total.
    /// </summary>
    /// <param name="code">The template code.</param>
    /// <param name="name">Name of the guild.</param>
    /// <param name="icon">Stream containing the icon for the guild.</param>
    /// <returns>The created guild.</returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuild> CreateGuildFromTemplateAsync(string code, string name, Optional<Stream> icon = default)
    {
        Optional<string?> iconb64 = Optional.FromNoValue<string?>();

        if (icon.HasValue && icon.Value != null)
        {
            using InlineMediaTool imgtool = new(icon.Value);
            iconb64 = imgtool.GetBase64();
        }
        else if (icon.HasValue)
        {
            iconb64 = null;
        }

        return await this.ApiClient.CreateGuildFromTemplateAsync(code, name, iconb64);
    }

    /// <summary>
    /// Gets a guild.
    /// <para>Setting <paramref name="withCounts"/> to true will make a REST request.</para>
    /// </summary>
    /// <param name="id">The guild ID to search for.</param>
    /// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
    /// <returns>The requested Guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuild> GetGuildAsync(ulong id, bool? withCounts = null)
    {
        if (this.guilds.TryGetValue(id, out DiscordGuild? guild) && (!withCounts.HasValue || !withCounts.Value))
        {
            return guild;
        }

        guild = await this.ApiClient.GetGuildAsync(id, withCounts);
        IReadOnlyList<DiscordChannel> channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id);
        foreach (DiscordChannel channel in channels)
        {
            guild.channels[channel.Id] = channel;
        }

        return guild;
    }

    /// <summary>
    /// Gets a guild preview
    /// </summary>
    /// <param name="id">The guild ID.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong id)
        => await this.ApiClient.GetGuildPreviewAsync(id);

    /// <summary>
    /// Gets an invite.
    /// </summary>
    /// <param name="code">The invite code.</param>
    /// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
    /// <param name="withExpiration">Whether to include the expiration date in the returned invite.</param>
    /// <returns>The requested Invite.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the invite does not exists.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordInvite> GetInviteByCodeAsync(string code, bool? withCounts = null, bool? withExpiration = null)
        => await this.ApiClient.GetInviteAsync(code, withCounts, withExpiration);

    /// <summary>
    /// Gets a list of connections
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordConnection>> GetConnectionsAsync()
        => await this.ApiClient.GetUsersConnectionsAsync();

    /// <summary>
    /// Gets a webhook
    /// </summary>
    /// <param name="id">The ID of webhook to get.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordWebhook> GetWebhookAsync(ulong id)
        => await this.ApiClient.GetWebhookAsync(id);

    /// <summary>
    /// Gets a webhook
    /// </summary>
    /// <param name="id">The ID of webhook to get.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token)
        => await this.ApiClient.GetWebhookWithTokenAsync(id, token);

    /// <summary>
    /// Updates current user's activity and status.
    /// </summary>
    /// <param name="activity">Activity to set.</param>
    /// <param name="userStatus">Status of the user.</param>
    /// <param name="idleSince">Since when is the client performing the specified activity.</param>
    /// <param name="shardId">
    /// The ID of the shard whose status should be updated. Defaults to null, which will update all shards controlled by
    /// this DiscordClient.
    /// </param>
    public async Task UpdateStatusAsync(DiscordActivity activity = null, DiscordUserStatus? userStatus = null, DateTimeOffset? idleSince = null, int? shardId = null)
    {
        StatusUpdate update = new()
        {
            Activity = new(activity),
            IdleSince = idleSince?.ToUnixTimeMilliseconds()
        };

        if (userStatus is not null)
        {
            update.Status = userStatus.Value;
        }

        GatewayPayload gatewayPayload = new() {OpCode = GatewayOpCode.StatusUpdate, Data = update};

        string payload = DiscordJson.SerializeObject(gatewayPayload);

        if (shardId is null)
        {
            await this.orchestrator.BroadcastOutboundEventAsync(Encoding.UTF8.GetBytes(payload));
        }
        else
        {
            // this is a bit of a hack. x % n, for any x < n, will always return x, so we can just pass the shard ID
            // as guild ID. this won't be very graceful if you pass an invalid ID, though.
            await this.orchestrator.SendOutboundEventAsync(Encoding.UTF8.GetBytes(payload), (ulong)shardId.Value);
        }
    }

    /// <summary>
    /// Edits current user.
    /// </summary>
    /// <param name="username">New username.</param>
    /// <param name="avatar">New avatar.</param>
    /// <param name="banner">New banner.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordUser> ModifyCurrentUserAsync(string username = null, Optional<Stream> avatar = default, Optional<Stream> banner = default)
    {
        Optional<string> avatarBase64 = Optional.FromNoValue<string>();
        if (avatar.HasValue && avatar.Value != null)
        {
            using InlineMediaTool imgtool = new(avatar.Value);
            avatarBase64 = imgtool.GetBase64();
        }
        else if (avatar.HasValue)
        {
            avatarBase64 = null;
        }

        Optional<string> bannerBase64 = Optional.FromNoValue<string>();
        if (banner.HasValue && banner.Value != null)
        {
            using InlineMediaTool imgtool = new(banner.Value);
            bannerBase64 = imgtool.GetBase64();
        }
        else if (banner.HasValue)
        {
            bannerBase64 = null;
        }

        TransportUser usr = await this.ApiClient.ModifyCurrentUserAsync(username, avatarBase64, bannerBase64);

        this.CurrentUser.Username = usr.Username;
        this.CurrentUser.Discriminator = usr.Discriminator;
        this.CurrentUser.AvatarHash = usr.AvatarHash;
        return this.CurrentUser;
    }

    /// <summary>
    /// Gets a guild template by the code.
    /// </summary>
    /// <param name="code">The code of the template.</param>
    /// <returns>The guild template for the code.</returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
        => await this.ApiClient.GetTemplateAsync(code);

    /// <summary>
    /// Gets all the global application commands for this application.
    /// </summary>
    /// <param name="withLocalizations">Whether to include localizations in the response.</param>
    /// <returns>A list of global application commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(bool withLocalizations = false) =>
        await this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id, withLocalizations);

    /// <summary>
    /// Overwrites the existing global application commands. New commands are automatically created and missing commands are automatically deleted.
    /// </summary>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of global commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
        await this.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(this.CurrentApplication.Id, commands);

    /// <summary>
    /// Creates or overwrites a global application command.
    /// </summary>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(DiscordApplicationCommand command) =>
        await this.ApiClient.CreateGlobalApplicationCommandAsync(this.CurrentApplication.Id, command);

    /// <summary>
    /// Gets a global application command by its id.
    /// </summary>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong commandId) =>
        await this.ApiClient.GetGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

    /// <summary>
    /// Gets a global application command by its name.
    /// </summary>
    /// <param name="commandName">The name of the command to get.</param>
    /// <param name="withLocalizations">Whether to include localizations in the response.</param>
    /// <returns>The command with the name.</returns>
    public async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(string commandName, bool withLocalizations = false)
    {
        foreach (DiscordApplicationCommand command in await this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id, withLocalizations))
        {
            if (command.Name == commandName)
            {
                return command;
            }
        }

        return null;
    }

    /// <summary>
    /// Edits a global application command.
    /// </summary>
    /// <param name="commandId">The ID of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edited command.</returns>
    public async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel mdl = new();
        action(mdl);

        ulong applicationId = this.CurrentApplication?.Id ?? (await GetCurrentApplicationAsync()).Id;

        return await this.ApiClient.EditGlobalApplicationCommandAsync
        (
            applicationId,
            commandId,
            mdl.Name,
            mdl.Description,
            mdl.Options,
            mdl.DefaultPermission,
            mdl.NSFW,
            mdl.NameLocalizations,
            mdl.DescriptionLocalizations,
            mdl.AllowDMUsage,
            mdl.DefaultMemberPermissions,
            mdl.AllowedContexts,
            mdl.IntegrationTypes
        );
    }

    /// <summary>
    /// Deletes a global application command.
    /// </summary>
    /// <param name="commandId">The ID of the command to delete.</param>
    public async Task DeleteGlobalApplicationCommandAsync(ulong commandId) =>
        await this.ApiClient.DeleteGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

    /// <summary>
    /// Gets all the application commands for a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to get application commands for.</param>
    /// <param name="withLocalizations">Whether to include localizations in the response.</param>
    /// <returns>A list of application commands in the guild.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong guildId, bool withLocalizations = false) =>
        await this.ApiClient.GetGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, withLocalizations);

    /// <summary>
    /// Overwrites the existing application commands in a guild. New commands are automatically created and missing commands are automatically deleted.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of guild commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, IEnumerable<DiscordApplicationCommand> commands) =>
        await this.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, commands);

    /// <summary>
    /// Creates or overwrites a guild application command.
    /// </summary>
    /// <param name="guildId">The ID of the guild to create the application command in.</param>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong guildId, DiscordApplicationCommand command) =>
        await this.ApiClient.CreateGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, command);

    /// <summary>
    /// Gets a application command in a guild by its ID.
    /// </summary>
    /// <param name="guildId">The ID of the guild the application command is in.</param>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
         await this.ApiClient.GetGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Edits a application command in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild the application command is in.</param>
    /// <param name="commandId">The ID of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edited command.</returns>
    public async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong guildId, ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel mdl = new();
        action(mdl);

        ulong applicationId = this.CurrentApplication?.Id ?? (await GetCurrentApplicationAsync()).Id;

        return await this.ApiClient.EditGuildApplicationCommandAsync
        (
            applicationId,
            guildId,
            commandId,
            mdl.Name,
            mdl.Description,
            mdl.Options,
            mdl.DefaultPermission,
            mdl.NSFW,
            mdl.NameLocalizations,
            mdl.DescriptionLocalizations,
            mdl.AllowDMUsage,
            mdl.DefaultMemberPermissions,
            mdl.AllowedContexts,
            mdl.IntegrationTypes
        );
    }

    /// <summary>
    /// Deletes a application command in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to delete the application command in.</param>
    /// <param name="commandId">The ID of the command.</param>
    public async Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
        await this.ApiClient.DeleteGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Returns a list of guilds before a certain guild. This will execute one API request per 200 guilds.
    /// <param name="limit">The amount of guilds to fetch.</param>
    /// <param name="before">The ID of the guild before which we fetch the guilds</param>
    /// <param name="withCount">Whether to include approximate member and presence counts in the returned guilds.</param>
    /// <param name="cancellationToken">Cancels the enumeration before doing the next api request</param>
    /// </summary>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordGuild> GetGuildsBeforeAsync(ulong before, int limit = 200, bool? withCount = null, CancellationToken cancellationToken = default)
        => GetGuildsInternalAsync(limit, before, withCount: withCount, cancellationToken: cancellationToken);

    /// <summary>
    /// Returns a list of guilds after a certain guild. This will execute one API request per 200 guilds.
    /// <param name="limit">The amount of guilds to fetch.</param>
    /// <param name="after">The ID of the guild after which we fetch the guilds.</param>
    /// <param name="withCount">Whether to include approximate member and presence counts in the returned guilds.</param>
    /// <param name="cancellationToken">Cancels the enumeration before doing the next api request</param>
    /// </summary>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordGuild> GetGuildsAfterAsync(ulong after, int limit = 200, bool? withCount = null, CancellationToken cancellationToken = default)
        => GetGuildsInternalAsync(limit, after: after, withCount: withCount, cancellationToken: cancellationToken);

    /// <summary>
    /// Returns a list of guilds the bot is in. This will execute one API request per 200 guilds.
    /// <param name="limit">The amount of guilds to fetch.</param>
    /// <param name="withCount">Whether to include approximate member and presence counts in the returned guilds.</param>
    /// <param name="cancellationToken">Cancels the enumeration before doing the next api request</param>
    /// </summary>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordGuild> GetGuildsAsync(int limit = 200, bool? withCount = null, CancellationToken cancellationToken = default) =>
        GetGuildsInternalAsync(limit, withCount: withCount, cancellationToken: cancellationToken);

    /// <summary>
    /// Creates a new emoji owned by the current application.
    /// </summary>
    /// <param name="name">The name of the emoji.</param>
    /// <param name="image">The image of the emoji.</param>
    /// <returns>The created emoji.</returns>
    public async ValueTask<DiscordEmoji> CreateApplicationEmojiAsync(string name, Stream image)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        name = name.Trim();
        if (name.Length is < 2 or > 50)
        {
            throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.");
        }

        ArgumentNullException.ThrowIfNull(image);

        string? image64 = null;

        using (InlineMediaTool imgtool = new(image))
        {
            image64 = imgtool.GetBase64();
        }

        return await this.ApiClient.CreateApplicationEmojiAsync(this.CurrentApplication.Id, name, image64);
    }

    /// <summary>
    /// Gets an emoji owned by the current application.
    /// </summary>
    /// <param name="emojiId">The ID of the emoji</param>
    /// <param name="skipCache">Whether to skip the cache.</param>
    /// <returns>The emoji.</returns>
    public async ValueTask<DiscordEmoji> GetApplicationEmojiAsync(ulong emojiId, bool skipCache = false)
    {
        if (!skipCache && this.CurrentApplication.ApplicationEmojis.TryGetValue(emojiId, out DiscordEmoji? emoji))
        {
            return emoji;
        }
        
        DiscordEmoji result = await this.ApiClient.GetApplicationEmojiAsync(this.CurrentApplication.Id, emojiId);
        
        this.CurrentApplication.ApplicationEmojis[emojiId] = result;
        
        return result;
    }

    /// <summary>
    /// Gets all emojis created or owned by the current application.
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache.</param>
    /// <returns>All emojis associated with the current application.
    /// This includes emojis uploaded by the owner or members of the team the application is on, if applicable.</returns>
    public async ValueTask<IReadOnlyList<DiscordEmoji>> GetApplicationEmojisAsync(bool skipCache = false)
    {
        if (this.CurrentApplication.ApplicationEmojis.Count > 0 && !skipCache)
        {
            return this.CurrentApplication.ApplicationEmojis.Values.ToArray();
        }
        
        IReadOnlyList<DiscordEmoji> result = await this.ApiClient.GetApplicationEmojisAsync(this.CurrentApplication.Id);
        
        foreach (DiscordEmoji emoji in result)
        {
            this.CurrentApplication.ApplicationEmojis[emoji.Id] = emoji;
        }
        
        return result;
    }

    /// <summary>
    /// Modifies an existing application emoji.
    /// </summary>
    /// <param name="emojiId">The ID of the emoji.</param>
    /// <param name="name">The new name of the emoji.</param>
    /// <returns>The updated emoji.</returns>
    public async ValueTask<DiscordEmoji> ModifyApplicationEmojiAsync(ulong emojiId, string name)
        => await this.ApiClient.ModifyApplicationEmojiAsync(this.CurrentApplication.Id, emojiId, name);

    /// <summary>
    /// Deletes an emoji.
    /// </summary>
    /// <param name="emojiId">The ID of the emoji to delete.</param>
    public async ValueTask DeleteApplicationEmojiAsync(ulong emojiId)
        => await this.ApiClient.DeleteApplicationEmojiAsync(this.CurrentApplication.Id, emojiId);

    private async IAsyncEnumerable<DiscordGuild> GetGuildsInternalAsync
    (
        int limit = 200,
        ulong? before = null,
        ulong? after = null,
        bool? withCount = null,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default
    )
    {
        if (limit < 0)
        {
            throw new ArgumentException("Cannot get a negative number of guilds.");
        }

        if (limit == 0)
        {
            yield break;
        }

        int remaining = limit;
        ulong? last = null;
        bool isbefore = before != null;

        int lastCount;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            int fetchSize = remaining > 200 ? 200 : remaining;
            IReadOnlyList<DiscordGuild> fetchedGuilds = await this.ApiClient.GetGuildsAsync(fetchSize, isbefore ? last ?? before : null, !isbefore ? last ?? after : null, withCount);

            lastCount = fetchedGuilds.Count;
            remaining -= lastCount;

            //We sort the returned guilds by ID so that they are in order in case Discord switches the order AGAIN.
            DiscordGuild[] sortedGuildsArray = [.. fetchedGuilds];
            Array.Sort(sortedGuildsArray, (x, y) => x.Id.CompareTo(y.Id));

            if (!isbefore)
            {
                foreach (DiscordGuild guild in sortedGuildsArray)
                {
                    yield return guild;
                }
                last = sortedGuildsArray.LastOrDefault()?.Id;
            }
            else
            {
                for (int i = sortedGuildsArray.Length - 1; i >= 0; i--)
                {
                    yield return sortedGuildsArray[i];
                }
                last = sortedGuildsArray.FirstOrDefault()?.Id;
            }
        }
        while (remaining > 0 && lastCount is > 0 and 100);
    }
    #endregion

    [StackTraceHidden]
    internal ChannelReader<GuildMembersChunkedEventArgs> RegisterGuildMemberChunksEnumerator(ulong guildId, string? nonce)
    {
        Int128 nonceKey = new(guildId, (ulong)(nonce?.GetHashCode() ?? 0));
        Channel<GuildMembersChunkedEventArgs> channel = Channel.CreateUnbounded<GuildMembersChunkedEventArgs>(new UnboundedChannelOptions { SingleWriter = true, SingleReader = true });

        if (!this.guildMembersChunkedEvents.TryAdd(nonceKey, channel))
        {
            throw new InvalidOperationException("A guild member chunk request for the given guild and nonce has already been registered.");
        }

        return channel.Reader;
    }

    private async ValueTask DispatchGuildMembersChunkForIteratorsAsync(GuildMembersChunkedEventArgs eventArgs)
    {
        if (this.guildMembersChunkedEvents.Count is 0)
        {
            return;
        }

        Int128 code = new(eventArgs.Guild.Id, (ulong)(eventArgs.Nonce?.GetHashCode() ?? 0));

        if (!this.guildMembersChunkedEvents.TryGetValue(code, out Channel<GuildMembersChunkedEventArgs>? eventChannel))
        {
            return;
        }

        await eventChannel.Writer.WriteAsync(eventArgs);

        // Discord docs state that 0 <= chunk_index < chunk_count, so add one
        // Basically, chunks are zero-based.
        if (eventArgs.ChunkIndex + 1 == eventArgs.ChunkCount)
        {
            this.guildMembersChunkedEvents.Remove(code, out _);
            eventChannel.Writer.Complete();
        }
    }

    #region Internal Caching Methods

    internal DiscordThreadChannel? InternalGetCachedThread(ulong threadId)
    {
        foreach (DiscordGuild guild in this.Guilds.Values)
        {
            if (guild.Threads.TryGetValue(threadId, out DiscordThreadChannel? foundThread))
            {
                return foundThread;
            }
        }

        return null;
    }

    internal DiscordThreadChannel? InternalGetCachedThread(ulong threadId, ulong? guildId)
    {
        if (!guildId.HasValue)
        {
            return InternalGetCachedThread(threadId);
        }

        if (this.guilds.TryGetValue(guildId.Value, out DiscordGuild? guild))
        {
            return guild.Threads.GetValueOrDefault(threadId);
        }

        return null;
    }

    internal DiscordChannel InternalGetCachedChannel(ulong channelId)
    {
        if (this.privateChannels?.TryGetValue(channelId, out DiscordDmChannel? foundDmChannel) == true)
        {
            return foundDmChannel;
        }

        foreach (DiscordGuild guild in this.Guilds.Values)
        {
            if (guild.Channels.TryGetValue(channelId, out DiscordChannel? foundChannel))
            {
                return foundChannel;
            }
        }

        return null;
    }

    internal DiscordChannel? InternalGetCachedChannel(ulong channelId, ulong? guildId)
    {
        if (guildId is not ulong nonNullGuildID)
        {
            return this.privateChannels.GetValueOrDefault(channelId) ?? InternalGetCachedChannel(channelId);
        }

        if (this.guilds.TryGetValue(nonNullGuildID, out DiscordGuild? guild))
        {
            return guild.Channels.GetValueOrDefault(channelId);
        }

        return InternalGetCachedChannel(channelId);
    }

    internal DiscordGuild InternalGetCachedGuild(ulong? guildId)
    {
        if (this.guilds != null && guildId.HasValue)
        {
            if (this.guilds.TryGetValue(guildId.Value, out DiscordGuild? guild))
            {
                return guild;
            }
        }

        return null;
    }

    private void UpdateMessage(DiscordMessage message, TransportUser author, DiscordGuild guild, TransportMember member)
    {
        if (author != null)
        {
            DiscordUser usr = new(author) { Discord = this };

            if (member != null)
            {
                member.User = author;
            }

            message.Author = UpdateUser(usr, guild?.Id, guild, member);
        }

        DiscordChannel? channel = InternalGetCachedChannel(message.ChannelId, message.guildId) ?? InternalGetCachedThread(message.ChannelId, message.guildId);

        if (channel != null)
        {
            return;
        }

        channel = !message.guildId.HasValue
            ? new DiscordDmChannel
            {
                Id = message.ChannelId,
                Discord = this,
                Type = DiscordChannelType.Private,
                Recipients = [message.Author]
            }
            : new DiscordChannel
            {
                Id = message.ChannelId,
                GuildId = guild.Id,
                Discord = this
            };

        UpdateChannelCache(channel);

        message.Channel = channel;
    }

    private DiscordUser UpdateUser(DiscordUser usr, ulong? guildId, DiscordGuild guild, TransportMember mbr)
    {
        if (mbr != null)
        {
            if (mbr.User != null)
            {
                usr = new DiscordUser(mbr.User) { Discord = this };

                UpdateUserCache(usr);

                usr = new DiscordMember(mbr) { Discord = this, guild_id = guildId.Value };
            }

            DiscordIntents intents = this.Intents;

            DiscordMember member = default;

            if (!intents.HasAllPrivilegedIntents() || guild.IsLarge) // we have the necessary privileged intents, no need to worry about caching here unless guild is large.
            {
                if (guild?.members.TryGetValue(usr.Id, out member) == false)
                {
                    if (intents.HasIntent(DiscordIntents.GuildMembers) || this.Configuration.AlwaysCacheMembers) // member can be updated by events, so cache it
                    {
                        guild.members.TryAdd(usr.Id, (DiscordMember)usr);
                    }
                }
                else if (intents.HasIntent(DiscordIntents.GuildPresences) || this.Configuration.AlwaysCacheMembers) // we can attempt to update it if it's already in cache.
                {
                    if (!intents.HasIntent(DiscordIntents.GuildMembers)) // no need to update if we already have the member events
                    {
                        _ = guild?.members.TryUpdate(usr.Id, (DiscordMember)usr, member);
                    }
                }
            }
        }
        else if (usr.Username != null) // check if not a skeleton user
        {
            UpdateUserCache(usr);
        }

        return usr;
    }

    private void UpdateCachedGuild(DiscordGuild newGuild, JArray rawMembers)
    {
        if (this.disposed)
        {
            return;
        }

        if (!this.guilds.TryGetValue(newGuild.Id, out DiscordGuild guild))
        {
            guild = newGuild;
            this.guilds[newGuild.Id] = guild;
        }

        if (newGuild.channels != null && !newGuild.channels.IsEmpty)
        {
            foreach (DiscordChannel channel in newGuild.channels.Values)
            {
                if (guild.channels.TryGetValue(channel.Id, out _))
                {
                    continue;
                }

                foreach (DiscordOverwrite overwrite in channel.permissionOverwrites)
                {
                    overwrite.Discord = this;
                    overwrite.channelId = channel.Id;
                }

                guild.channels[channel.Id] = channel;
            }
        }
        if (newGuild.threads != null && !newGuild.threads.IsEmpty)
        {
            foreach (DiscordThreadChannel thread in newGuild.threads.Values)
            {
                if (guild.threads.TryGetValue(thread.Id, out _))
                {
                    continue;
                }

                guild.threads[thread.Id] = thread;
            }
        }

        foreach (DiscordEmoji newEmoji in newGuild.emojis.Values)
        {
            _ = guild.emojis.GetOrAdd(newEmoji.Id, _ => newEmoji);
        }

        foreach (DiscordMessageSticker newSticker in newGuild.stickers.Values)
        {
            _ = guild.stickers.GetOrAdd(newSticker.Id, _ => newSticker);
        }

        if (rawMembers != null)
        {
            guild.members.Clear();

            foreach (JToken xj in rawMembers)
            {
                TransportMember xtm = xj.ToDiscordObject<TransportMember>();

                DiscordUser xu = new(xtm.User) { Discord = this };
                UpdateUserCache(xu);

                guild.members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, guild_id = guild.Id };
            }
        }

        foreach (DiscordRole role in newGuild.roles.Values)
        {
            if (guild.roles.TryGetValue(role.Id, out _))
            {
                continue;
            }

            role.guild_id = guild.Id;
            guild.roles[role.Id] = role;
        }

        if (newGuild.stageInstances != null)
        {
            foreach (DiscordStageInstance newStageInstance in newGuild.stageInstances.Values)
            {
                _ = guild.stageInstances.GetOrAdd(newStageInstance.Id, _ => newStageInstance);
            }
        }

        guild.Name = newGuild.Name;
        guild.AfkChannelId = newGuild.AfkChannelId;
        guild.AfkTimeout = newGuild.AfkTimeout;
        guild.DefaultMessageNotifications = newGuild.DefaultMessageNotifications;
        guild.Features = newGuild.Features;
        guild.IconHash = newGuild.IconHash;
        guild.MfaLevel = newGuild.MfaLevel;
        guild.OwnerId = newGuild.OwnerId;
        guild.voiceRegionId = newGuild.voiceRegionId;
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
        guild.PremiumProgressBarEnabled = newGuild.PremiumProgressBarEnabled;

        // fields not sent for update:
        // - guild.Channels
        // - voice states
        // - guild.JoinedAt = new_guild.JoinedAt;
        // - guild.Large = new_guild.Large;
        // - guild.MemberCount = Math.Max(new_guild.MemberCount, guild.members.Count);
        // - guild.Unavailable = new_guild.Unavailable;
    }

    private void PopulateMessageReactionsAndCache(DiscordMessage message, TransportUser author, TransportMember member)
    {
        DiscordGuild guild = message.Channel?.Guild ?? InternalGetCachedGuild(message.guildId);
        UpdateMessage(message, author, guild, member);
        message.reactions ??= [];

        foreach (DiscordReaction xr in message.reactions)
        {
            xr.Emoji.Discord = this;
        }

        if (message.Channel is not null)
        {
            this.MessageCache?.Add(message);
        }
    }

    // Ensures the channel is cached:
    // - DM -> _privateChannels dict on DiscordClient
    // - Thread -> DiscordGuild#_threads
    // - _ -> DiscordGuild#_channels
    private void UpdateChannelCache(DiscordChannel? channel)
    {
        if (channel is null)
        {
            return;
        }

        switch (channel)
        {
            case DiscordDmChannel dmChannel:
                this.privateChannels.TryAdd(channel.Id, dmChannel);
                break;
            case DiscordThreadChannel threadChannel:
                if (this.guilds.TryGetValue(channel.GuildId!.Value, out DiscordGuild? guild))
                {
                    guild.threads.TryAdd(channel.Id, threadChannel);
                }
                break;
            default:
                if (this.guilds.TryGetValue(channel.GuildId!.Value, out guild))
                {
                    guild.channels.TryAdd(channel.Id, channel);
                }
                break;
        }
    }

    #endregion

    #region Disposal

    private bool disposed;

    /// <summary>
    /// Disposes your DiscordClient.
    /// </summary>
    public override void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        DisconnectAsync().GetAwaiter().GetResult();
        this.ApiClient?.rest?.Dispose();
        this.CurrentUser = null!;

        this.guilds = null!;
        this.privateChannels = null!;
    }

    #endregion
}
