#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Metrics;
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

    /// <summary>
    /// Gets the cached guilds for this client.
    /// </summary>
    public abstract IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }

    /// <summary>
    /// Gets the cached users for this client.
    /// </summary>
    protected internal ConcurrentDictionary<ulong, DiscordUser> UserCache { get; }

    /// <summary>
    /// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
    /// </summary>
    public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions
        => _voice_regions_lazy.Value;

    /// <summary>
    /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
    /// </summary>
    protected internal ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }
    internal Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> _voice_regions_lazy;

    /// <summary>
    /// Initializes this Discord API client.
    /// </summary>
    /// <param name="config">Configuration for this client.</param>
    /// <param name="rest_client">Restclient which will be used for the underlying ApiClients</param>
    internal BaseDiscordClient(DiscordConfiguration config, RestClient? rest_client = null)
    {
        Configuration = new DiscordConfiguration(config);

        if (Configuration.LoggerFactory == null)
        {
            Configuration.LoggerFactory = new DefaultLoggerFactory();
            Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
        }
        Logger = Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();

        ApiClient = new DiscordApiClient(this, rest_client);
        UserCache = new ConcurrentDictionary<ulong, DiscordUser>();
        InternalVoiceRegions = new ConcurrentDictionary<string, DiscordVoiceRegion>();
        _voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(InternalVoiceRegions));

        Assembly a = typeof(DiscordClient).GetTypeInfo().Assembly;

        AssemblyInformationalVersionAttribute? iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (iv != null)
        {
            VersionString = iv.InformationalVersion;
        }
        else
        {
            Version? v = a.GetName().Version;
            string vs = v.ToString(3);

            if (v.Revision > 0)
            {
                VersionString = $"{vs}, CI build {v.Revision}";
            }
        }
    }

    /// <inheritdoc cref="RestClient.GetRequestMetrics(bool)"/>
    public RequestMetricsCollection GetRequestMetrics(bool sinceLastCall = false)
        => ApiClient.GetRequestMetrics(sinceLastCall);

    /// <summary>
    /// Gets the current API application.
    /// </summary>
    /// <returns>Current API application.</returns>
    public async Task<DiscordApplication> GetCurrentApplicationAsync()
    {
        Net.Abstractions.TransportApplication tapp = await ApiClient.GetCurrentApplicationInfoAsync();
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
        => await ApiClient.ListVoiceRegionsAsync();

    /// <summary>
    /// Initializes this client. This method fetches information about current user, application, and voice regions.
    /// </summary>
    /// <returns></returns>
    public virtual async Task InitializeAsync()
    {
        if (CurrentUser == null)
        {
            CurrentUser = await ApiClient.GetCurrentUserAsync();
            UpdateUserCache(CurrentUser);
        }

        if (Configuration.TokenType == TokenType.Bot && CurrentApplication == null)
        {
            CurrentApplication = await GetCurrentApplicationAsync();
        }

        if (Configuration.TokenType != TokenType.Bearer && InternalVoiceRegions.Count == 0)
        {
            IReadOnlyList<DiscordVoiceRegion> vrs = await ListVoiceRegionsAsync();
            foreach (DiscordVoiceRegion xvr in vrs)
            {
                InternalVoiceRegions.TryAdd(xvr.Id, xvr);
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
        if (Configuration.TokenType != TokenType.Bot)
        {
            throw new InvalidOperationException("Only bot tokens can access this info.");
        }

        if (string.IsNullOrEmpty(Configuration.Token))
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Could not locate a valid token.");
            }

            Configuration.Token = token;

            GatewayInfo res = await ApiClient.GetGatewayInfoAsync();
            Configuration.Token = null;
            return res;
        }

        return await ApiClient.GetGatewayInfoAsync();
    }

    internal DiscordUser GetCachedOrEmptyUserInternal(ulong user_id)
    {
        TryGetCachedUserInternal(user_id, out DiscordUser? user);
        return user;
    }

    internal bool TryGetCachedUserInternal(ulong user_id, out DiscordUser user)
    {
        if (UserCache.TryGetValue(user_id, out user))
        {
            return true;
        }

        user = new DiscordUser { Id = user_id, Discord = this };
        return false;
    }

    // This previously set properties on the old user and re-injected into the cache.
    // That's terrible. Instead, insert the new reference and let the old one get GC'd.
    // End-users are more likely to be holding a reference to the new object via an event or w/e
    // anyways.
    // Furthermore, setting properties requires keeping track of where we update cache and updating repeat code.
    internal DiscordUser UpdateUserCache(DiscordUser newUser)
        => UserCache.AddOrUpdate(newUser.Id, newUser, (_, _) => newUser);

    /// <summary>
    /// Disposes this client.
    /// </summary>
    public abstract void Dispose();
}
