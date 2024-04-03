#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Caching;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

namespace DSharpPlus;

/// <summary>
/// Represents a common base for various Discord client implementations.
/// </summary>
public abstract class BaseDiscordClient : IDisposable
{
    protected internal DiscordApiClient ApiClient { get; }
    protected internal DiscordConfiguration Configuration { get; }

    /// <summary>
    /// Gets the instance of the logger for this client.
    /// </summary>
    public ILogger<BaseDiscordClient> Logger { get; }

    /// <summary>
    /// Gets the string representing the version of D#+.
    /// </summary>
    public string VersionString { get; }

    /// <summary>
    /// Gets the current user.
    /// </summary>
    public DiscordUser CurrentUser { get; internal set; }

    /// <summary>
    /// Gets the current application.
    /// </summary>
    public DiscordApplication CurrentApplication { get; internal set; }

    public IReadOnlyCollection<ulong> Guilds => new ReadOnlyCollection<ulong>(this._guildIds);

    /// <summary>
    /// Gets the cached guilds for this client.
    /// </summary>
    internal List<ulong> _guildIds { get; set; }
    
    public IDiscordCache Cache { get; }
    
    /// <summary>
    /// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
    /// </summary>
    public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions
        => this._voice_regions_lazy.Value;

    /// <summary>
    /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
    /// </summary>
    protected internal ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }
    internal Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> _voice_regions_lazy;

    /// <summary>
    /// Initializes this Discord API client.
    /// </summary>
    /// <param name="config">Configuration for this client.</param>
    /// <param name="restClient">Rest client to use for this client.</param>
    internal BaseDiscordClient(DiscordConfiguration config, RestClient? restClient = null)
    {
        this.Configuration = new DiscordConfiguration(config);

        if (this.Configuration.LoggerFactory is null)
        {
            this.Configuration.LoggerFactory = new DefaultLoggerFactory();
            this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
        }
        this.Logger = this.Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();

        this.ApiClient = new DiscordApiClient(this, restClient);
        this.InternalVoiceRegions = new ConcurrentDictionary<string, DiscordVoiceRegion>();
        this._voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.InternalVoiceRegions));

        Assembly a = typeof(DiscordClient).GetTypeInfo().Assembly;

        AssemblyInformationalVersionAttribute? iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (iv != null)
        {
            this.VersionString = iv.InformationalVersion;
        }
        else
        {
            Version? v = a.GetName().Version;
            string vs = v.ToString(3);

            if (v.Revision > 0)
            {
                this.VersionString = $"{vs}, CI build {v.Revision}";
            }
        }
        
        this.Cache = this.Configuration.CacheProvider is not null
            ? this.Configuration.CacheProvider
            : new DiscordMemoryCache(this.Configuration.CacheConfiguration);
    }

    /// <summary>
    /// Gets the current API application.
    /// </summary>
    /// <returns>Current API application.</returns>
    public async Task<DiscordApplication> GetCurrentApplicationAsync()
    {
        Net.Abstractions.TransportApplication tapp = await this.ApiClient.GetCurrentApplicationInfoAsync();
        DiscordApplication app = new DiscordApplication
        {
            Discord = this,
            Id = tapp.Id,
            Name = tapp.Name,
            Description = tapp.Description,
            Summary = tapp.Summary,
            IconHash = tapp.IconHash,
            TermsOfServiceUrl = tapp.TermsOfServiceUrl,
            PrivacyPolicyUrl = tapp.PrivacyPolicyUrl,
            RpcOrigins = tapp.RpcOrigins != null ? new ReadOnlyCollection<string>(tapp.RpcOrigins) : null,
            Flags = tapp.Flags,
            RequiresCodeGrant = tapp.BotRequiresCodeGrant,
            IsPublic = tapp.IsPublicBot,
            CoverImageHash = null
        };

        // do team and owners
        // tbh fuck doing this properly
        if (tapp.Team == null)
        {
            // singular owner

            app.Owners = new ReadOnlyCollection<DiscordUser>(new[] { new DiscordUser(tapp.Owner) });
            app.Team = null;
        }
        else
        {
            // team owner

            app.Team = new DiscordTeam(tapp.Team);

            DiscordTeamMember[] members = tapp.Team.Members
                .Select(x => new DiscordTeamMember(x) { Team = app.Team, User = new DiscordUser(x.User) })
                .ToArray();

            DiscordUser[] owners = members
                .Where(x => x.MembershipStatus == DiscordTeamMembershipStatus.Accepted)
                .Select(x => x.User)
                .ToArray();

            app.Owners = new ReadOnlyCollection<DiscordUser>(owners);
            app.Team.Owner = owners.FirstOrDefault(x => x.Id == tapp.Team.OwnerId);
            app.Team.Members = new ReadOnlyCollection<DiscordTeamMember>(members);
        }

        return app;
    }

    /// <summary>
    /// Gets a list of regions
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
        => await this.ApiClient.ListVoiceRegionsAsync();

    /// <summary>
    /// Initializes this client. This method fetches information about current user, application, and voice regions.
    /// </summary>
    /// <returns></returns>
    public virtual async Task InitializeAsync()
    {
        if (this.CurrentUser == null)
        {
            this.CurrentUser = await this.ApiClient.GetCurrentUserAsync();
            await this.Cache.AddUserAsync(this.CurrentUser);
        }

        if (this.Configuration.TokenType == TokenType.Bot && this.CurrentApplication == null)
        {
            this.CurrentApplication = await this.GetCurrentApplicationAsync();
        }

        if (this.Configuration.TokenType != TokenType.Bearer && this.InternalVoiceRegions.Count == 0)
        {
            IReadOnlyList<DiscordVoiceRegion> vrs = await this.ListVoiceRegionsAsync();
            foreach (DiscordVoiceRegion xvr in vrs)
            {
                this.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
            }
        }
    }

    /// <summary>
    /// Gets the current gateway info for the provided token.
    /// <para>If no value is provided, the configuration value will be used instead.</para>
    /// </summary>
    /// <returns>A gateway info object.</returns>
    public async Task<GatewayInfo> GetGatewayInfoAsync(string token = null)
    {
        if (this.Configuration.TokenType != TokenType.Bot)
        {
            throw new InvalidOperationException("Only bot tokens can access this info.");
        }

        if (string.IsNullOrEmpty(this.Configuration.Token))
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Could not locate a valid token.");
            }

            this.Configuration.Token = token;

            GatewayInfo res = await this.ApiClient.GetGatewayInfoAsync();
            this.Configuration.Token = null;
            return res;
        }

        return await this.ApiClient.GetGatewayInfoAsync();
    }
    
    internal async ValueTask<DiscordUser?> TryGetCachedUserInternalAsync(ulong userId)
    {
        ICacheKey key = ICacheKey.ForUser(userId);
        
        DiscordUser? user = await this.Cache.TryGet<DiscordUser>(key);

        return user;
    }

    #region Cached requests

    /// <summary>
    /// Gets a user
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="skipCache">Whether to always make a REST request and update cache. Passing true will update the user, updating stale properties such as <see cref="DiscordUser.BannerHash"/>.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async ValueTask<DiscordUser> GetUserAsync(ulong userId, bool skipCache = false)
    {
        if (!skipCache)
        {
            DiscordUser? cachedUser = await this.TryGetCachedUserInternalAsync(userId);
            if (cachedUser is not null)
            {
                return cachedUser;
            }
        }

        DiscordUser usr = await this.ApiClient.GetUserAsync(userId);
        
        await this.Cache.Set(usr, usr.GetCacheKey());

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
    public async ValueTask<DiscordChannel> GetChannelAsync(ulong id, bool skipCache = false)
    {
        if (skipCache)
        {
            return await this.ApiClient.GetChannelAsync(id);
        }

        DiscordChannel? channel = await this.Cache.TryGetChannelAsync(id);
        if (channel is not null)
        {
            return channel;
        }
        
        return await this.ApiClient.GetChannelAsync(id);
    }
    
    /// <summary>
    /// Gets a guild.
    /// <para>Setting <paramref name="withCounts"/> to true will always make a REST request.</para>
    /// </summary>
    /// <param name="id">The guild ID to search for.</param>
    /// <param name="withChannels">Whether to include guild channels. This will add an additional REST request</param>
    /// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
    /// <param name="skipCache">Whether to skip the cache and always excute a REST request</param>
    /// <returns>The requested Guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async ValueTask<DiscordGuild> GetGuildAsync(ulong id, bool withChannels = true, bool? withCounts = null, bool skipCache = false)
    {
        DiscordGuild guild;
        IReadOnlyList<DiscordChannel> channels;
        if (skipCache)
        {
            guild = await this.ApiClient.GetGuildAsync(id, withCounts);
            
            if (withChannels)
            {
                channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id);
                foreach (DiscordChannel channel in channels)
                {
                    guild._channels[channel.Id] = channel;
                }
            }
        }
        
        if (!withCounts.HasValue || !withCounts.Value || !withChannels)
        {
            DiscordGuild? cachedGuild = await this.Cache.TryGetGuildAsync(id);
            if (cachedGuild is not null)
            {
                return cachedGuild;
            }
        }

        guild = await this.ApiClient.GetGuildAsync(id, withCounts);
        if (withChannels)
        {
            channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id);
            foreach (DiscordChannel channel in channels)
            {
                guild._channels[channel.Id] = channel;
            }
        }

        return guild;
    }
    

    #endregion
    
    
    /// <summary>
    /// Disposes this client.
    /// </summary>
    public abstract void Dispose();
}
