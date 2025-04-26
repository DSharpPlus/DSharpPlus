#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    protected internal DiscordRestApiClient ApiClient { get; internal init; }
    protected internal DiscordConfiguration Configuration { get; internal init; }

    /// <summary>
    /// Gets the intents this client has.
    /// </summary>
    public DiscordIntents Intents { get; internal set; } = DiscordIntents.None;

    /// <summary>
    /// Gets the instance of the logger for this client.
    /// </summary>
    public ILogger Logger { get; internal init; }

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
        => this.InternalVoiceRegions;

    /// <summary>
    /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
    /// </summary>
    protected internal ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }

    /// <summary>
    /// Initializes this Discord API client.
    /// </summary>
    internal BaseDiscordClient()
    {
        this.UserCache = new ConcurrentDictionary<ulong, DiscordUser>();
        this.InternalVoiceRegions = new ConcurrentDictionary<string, DiscordVoiceRegion>();
        
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
    }

    /// <inheritdoc cref="RestClient.GetRequestMetrics(bool)"/>
    public RequestMetricsCollection GetRequestMetrics(bool sinceLastCall = false)
        => this.ApiClient.GetRequestMetrics(sinceLastCall);

    /// <summary>
    /// Gets the current API application.
    /// </summary>
    /// <returns>Current API application.</returns>
    public async Task<DiscordApplication> GetCurrentApplicationAsync()
    {
        Net.Abstractions.TransportApplication tapp = await this.ApiClient.GetCurrentApplicationInfoAsync();
        return new DiscordApplication(tapp, this);
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
        if (this.CurrentUser is null)
        {
            this.CurrentUser = await this.ApiClient.GetCurrentUserAsync();
            UpdateUserCache(this.CurrentUser);
        }

        if (this is DiscordClient && this.CurrentApplication is null)
        {
            this.CurrentApplication = await GetCurrentApplicationAsync();
        }

        if (this is DiscordClient && this.InternalVoiceRegions.IsEmpty)
        {
            IReadOnlyList<DiscordVoiceRegion> vrs = await ListVoiceRegionsAsync();
            foreach (DiscordVoiceRegion xvr in vrs)
            {
                this.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
            }
        }
    }

    /// <summary>
    /// Gets the current gateway info.
    /// </summary>
    /// <returns>A gateway info object.</returns>
    public async Task<GatewayInfo> GetGatewayInfoAsync() 
        => await this.ApiClient.GetGatewayInfoAsync();

    internal DiscordUser GetCachedOrEmptyUserInternal(ulong user_id)
    {
        TryGetCachedUserInternal(user_id, out DiscordUser? user);
        return user;
    }

    internal bool TryGetCachedUserInternal(ulong user_id, out DiscordUser user)
    {
        if (this.UserCache.TryGetValue(user_id, out user))
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
        => this.UserCache.AddOrUpdate(newUser.Id, newUser, (_, _) => newUser);

    /// <summary>
    /// Disposes this client.
    /// </summary>
    public abstract void Dispose();
}
