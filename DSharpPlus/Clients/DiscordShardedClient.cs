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
    
    #region Error Handling

    internal void EventErrorHandler<TArgs>(AsyncEvent<DiscordClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<DiscordClient, TArgs> handler, DiscordClient sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs
    {
        this.Logger.LogError(LoggerEvents.EventHandlerException, ex, "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaringType})", asyncEvent.Name, handler.Method, handler.Method.DeclaringType);
        this._clientErrored.InvokeAsync(sender, new ClientErrorEventArgs { EventName = asyncEvent.Name, Exception = ex }).Wait();
    }

    private void Goof<TArgs>(AsyncEvent<DiscordClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<DiscordClient, TArgs> handler, DiscordClient sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs => this.Logger.LogCritical(LoggerEvents.EventHandlerException, ex, "Exception event handler {Method} (defined in {DeclaringType}) threw an exception", handler.Method, handler.Method.DeclaringType);

    #endregion
    
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
