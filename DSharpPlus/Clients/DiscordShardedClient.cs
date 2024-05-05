#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

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
        => this.voiceRegionsLazy?.Value;

    #endregion

    #region Private Properties/Fields

    private DiscordConfiguration Configuration { get; }

    /// <summary>
    /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
    /// </summary>
    private ConcurrentDictionary<string, DiscordVoiceRegion> internalVoiceRegions;

    private readonly ConcurrentDictionary<int, DiscordClient> shards = new();
    private Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> voiceRegionsLazy;
    private bool isStarted;
    private readonly bool manuallySharding;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes new auto-sharding Discord client.
    /// </summary>
    /// <param name="config">Configuration to use.</param>
    public DiscordShardedClient(DiscordConfiguration config) : this()
    {
        if (config.ShardCount > 1)
        {
            this.manuallySharding = true;
        }

        this.Configuration = config;
        this.ShardClients = new ReadOnlyConcurrentDictionary<int, DiscordClient>(this.shards);

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
        if (this.isStarted)
        {
            throw new InvalidOperationException("This client has already been started.");
        }

        this.isStarted = true;

        try
        {
            if (this.Configuration.TokenType != TokenType.Bot)
            {
                this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
            }

            this.Logger.LogInformation(LoggerEvents.Startup, "DSharpPlus, version {Version}", this.versionString.Value);

            int shardc = await InitializeShardsAsync();
            List<Task> connectTasks = [];
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
                    await ConnectShardAsync(i);
                }
                else
                {
                    //Concurrent login.
                    connectTasks.Add(ConnectShardAsync(i));

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
            await InternalStopAsync(false);

            string message = "Shard initialization failed, check inner exceptions for details: ";
            this.Logger.LogCritical(LoggerEvents.ShardClientError, ex, "{Message}", message);
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
        int index = this.manuallySharding ? GetShardIdFromGuilds(guildId) : Utilities.GetShardId(guildId, this.ShardClients.Count);

        return index != -1 ? this.shards[index] : null;
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
    public async Task UpdateStatusAsync(DiscordActivity activity = null, DiscordUserStatus? userStatus = null, DateTimeOffset? idleSince = null)
    {
        List<Task> tasks = [];
        foreach (DiscordClient client in this.shards.Values)
        {
            tasks.Add(client.UpdateStatusAsync(activity, userStatus, idleSince));
        }

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Internal Methods

    public async Task<int> InitializeShardsAsync()
    {
        if (!this.shards.IsEmpty)
        {
            return this.shards.Count;
        }

        ShardedLoggerFactory loggerFactory = new(this.Configuration.LoggerFactory);
        RestClient restClient = new(this.Configuration, loggerFactory.CreateLogger<RestClient>());
        DiscordApiClient apiClient = new(restClient);

        this.GatewayInfo = await apiClient.GetGatewayInfoAsync();
        int shardCount = this.Configuration.ShardCount == 1 ? this.GatewayInfo.ShardCount : this.Configuration.ShardCount;

        for (int i = 0; i < shardCount; i++)
        {
            DiscordConfiguration cfg = new(this.Configuration)
            {
                ShardId = i,
                ShardCount = shardCount,
                LoggerFactory = loggerFactory
            };

            DiscordClient client = new(cfg, restClient);
            if (!this.shards.TryAdd(i, client))
            {
                throw new InvalidOperationException("Could not initialize shards.");
            }
        }

        return shardCount;
    }

    #endregion

    #region Private Methods/Version Property

    private readonly Lazy<string> versionString = new(() =>
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
        if (!this.shards.TryGetValue(i, out DiscordClient? client))
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

        if (this.internalVoiceRegions != null)
        {
            client.InternalVoiceRegions = this.internalVoiceRegions;
            client.voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(client.InternalVoiceRegions));
        }

        HookEventHandlers(client);

        client.isShard = true;
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

        if (this.internalVoiceRegions == null)
        {
            this.internalVoiceRegions = client.InternalVoiceRegions;
            this.voiceRegionsLazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.internalVoiceRegions));
        }
    }

    private Task InternalStopAsync(bool enableLogger = true)
    {
        if (!this.isStarted)
        {
            throw new InvalidOperationException("This client has not been started.");
        }

        this.isStarted = false;

        if (enableLogger)
        {
            this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disposing {ShardCount} shards.", this.shards.Count);
        }

        this.voiceRegionsLazy = null;
        this.GatewayInfo = null;
        this.CurrentUser = null;
        this.CurrentApplication = null;

        for (int i = 0; i < this.shards.Count; i++)
        {
            if (this.shards.TryGetValue(i, out DiscordClient? client))
            {
                UnhookEventHandlers(client);

                client.Dispose();

                if (enableLogger)
                {
                    this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disconnected shard {Shard}.", i);
                }
            }
        }

        this.shards.Clear();

        return Task.CompletedTask;
    }

    #endregion

    #region Event Handler Initialization/Registering

    #region Error Handling

    internal void EventErrorHandler<TArgs>(AsyncEvent<DiscordClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<DiscordClient, TArgs> handler, DiscordClient sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs
    {
        this.Logger.LogError(LoggerEvents.EventHandlerException, ex, "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaringType})", asyncEvent.Name, handler.Method, handler.Method.DeclaringType);
        this.clientErrored.InvokeAsync(sender, new ClientErrorEventArgs { EventName = asyncEvent.Name, Exception = ex }).Wait();
    }

    private void Goof<TArgs>(AsyncEvent<DiscordClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<DiscordClient, TArgs> handler, DiscordClient sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs => this.Logger.LogCritical(LoggerEvents.EventHandlerException, ex, "Exception event handler {Method} (defined in {DeclaringType}) threw an exception", handler.Method, handler.Method.DeclaringType);

    #endregion

    private int GetShardIdFromGuilds(ulong id)
    {
        foreach (DiscordClient s in this.shards.Values)
        {
            if (s.guilds.TryGetValue(id, out _))
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
        if (this.isStarted)
        {
            InternalStopAsync(false).GetAwaiter().GetResult();
        }
    }

    #endregion
}
