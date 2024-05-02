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
    public DiscordShardedClient(DiscordConfiguration config) : this()
    {
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

            int shardc = await InitializeShardsAsync();
            List<Task> connectTasks = [];
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
                    await ConnectShardAsync(i);
                }
                else
                {
                    //Concurrent login.
                    connectTasks.Add(ConnectShardAsync(i));

                    if (connectTasks.Count == GatewayInfo.SessionBucket.MaxConcurrency)
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
            Logger.LogCritical(LoggerEvents.ShardClientError, ex, "{Message}", message);
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
    public async Task UpdateStatusAsync(DiscordActivity activity = null, DiscordUserStatus? userStatus = null, DateTimeOffset? idleSince = null)
    {
        List<Task> tasks = [];
        foreach (DiscordClient client in _shards.Values)
        {
            tasks.Add(client.UpdateStatusAsync(activity, userStatus, idleSince));
        }

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Internal Methods

    public async Task<int> InitializeShardsAsync()
    {
        if (!_shards.IsEmpty)
        {
            return _shards.Count;
        }

        ShardedLoggerFactory loggerFactory = new(Configuration.LoggerFactory);
        RestClient restClient = new(Configuration, loggerFactory.CreateLogger<RestClient>());
        DiscordApiClient apiClient = new(restClient);

        GatewayInfo = await apiClient.GetGatewayInfoAsync();
        int shardCount = Configuration.ShardCount == 1 ? GatewayInfo.ShardCount : Configuration.ShardCount;

        for (int i = 0; i < shardCount; i++)
        {
            DiscordConfiguration cfg = new(Configuration)
            {
                ShardId = i,
                ShardCount = shardCount,
                LoggerFactory = loggerFactory
            };

            DiscordClient client = new(cfg, restClient);
            if (!_shards.TryAdd(i, client))
            {
                throw new InvalidOperationException("Could not initialize shards.");
            }
        }

        return shardCount;
    }

    #endregion

    #region Private Methods/Version Property

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
        await client.ConnectAsync();
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

        _isStarted = false;

        if (enableLogger)
        {
            Logger.LogInformation(LoggerEvents.ShardShutdown, "Disposing {ShardCount} shards.", _shards.Count);
        }

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

    #region Error Handling

    internal void EventErrorHandler<TArgs>(AsyncEvent<DiscordClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<DiscordClient, TArgs> handler, DiscordClient sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs
    {
        Logger.LogError(LoggerEvents.EventHandlerException, ex, "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaringType})", asyncEvent.Name, handler.Method, handler.Method.DeclaringType);
        _clientErrored.InvokeAsync(sender, new ClientErrorEventArgs { EventName = asyncEvent.Name, Exception = ex }).Wait();
    }

    private void Goof<TArgs>(AsyncEvent<DiscordClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<DiscordClient, TArgs> handler, DiscordClient sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs => Logger.LogCritical(LoggerEvents.EventHandlerException, ex, "Exception event handler {Method} (defined in {DeclaringType}) threw an exception", handler.Method, handler.Method.DeclaringType);

    #endregion

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
    {
        if (_isStarted)
        {
            InternalStopAsync(false).GetAwaiter().GetResult();
        }
    }

    #endregion
}
