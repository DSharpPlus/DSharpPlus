#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    protected internal DiscordRestApiClient ApiClient { get; internal set; }
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
        
        Assembly assembly = typeof(DiscordClient).GetTypeInfo().Assembly;

        AssemblyInformationalVersionAttribute? versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (versionAttribute != null)
        {
            this.VersionString = versionAttribute.InformationalVersion;
        }
        else
        {
            Version? version = assembly.GetName().Version;
            string versionString = version.ToString(3);

            if (version.Revision > 0)
            {
                this.VersionString = $"{versionString}, CI build {version.Revision}";
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
        Net.Abstractions.TransportApplication transportApplication = await this.ApiClient.GetCurrentApplicationInfoAsync();
        return new DiscordApplication(transportApplication, this);
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
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
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
            IReadOnlyList<DiscordVoiceRegion> voiceRegions = await ListVoiceRegionsAsync();
            foreach (DiscordVoiceRegion voiceRegion in voiceRegions)
            {
                this.InternalVoiceRegions.TryAdd(voiceRegion.Id, voiceRegion);
            }
        }
    }

    /// <summary>
    /// Gets the current gateway info.
    /// </summary>
    /// <returns>A gateway info object.</returns>
    public async Task<GatewayInfo> GetGatewayInfoAsync() 
        => await this.ApiClient.GetGatewayInfoAsync();

    internal DiscordUser GetCachedOrEmptyUserInternal(ulong userId)
    {
        TryGetCachedUserInternal(userId, out DiscordUser? user);
        return user;
    }

    internal bool TryGetCachedUserInternal(ulong userId, [NotNullWhen(true)] out DiscordUser? user)
    {
        if (this.UserCache.TryGetValue(userId, out user))
        {
            return true;
        }

        user = new DiscordUser { Id = userId, Discord = this };
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
