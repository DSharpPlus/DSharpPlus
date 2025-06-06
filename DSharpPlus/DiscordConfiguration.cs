using System;
using DSharpPlus.Net.Udp;

namespace DSharpPlus;

/// <summary>
/// Represents configuration for <see cref="DiscordClient"/>.
/// </summary>
public sealed class DiscordConfiguration
{

    /// <summary>
    /// Sets whether the client should attempt to cache members if exclusively using unprivileged intents.
    /// <para>
    ///     This will only take effect if there are no <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.GuildPresences"/>
    ///     intents specified. Otherwise, this will always be overwritten to true.
    /// </para>
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool AlwaysCacheMembers { internal get; set; } = true;

    /// <summary>
    /// Sets the default absolute expiration time for cached messages.
    /// </summary>
    public TimeSpan AbsoluteMessageCacheExpiration { internal get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// Sets the default sliding expiration time for cached messages. This is refreshed every time the message is
    /// accessed.
    /// </summary>
    public TimeSpan SlidingMessageCacheExpiration { internal get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// <para>Sets the factory method used to create instances of UDP clients.</para>
    /// <para>Use <see cref="DspUdpClient.CreateNew"/> and equivalents on other implementations to switch out client implementations.</para>
    /// <para>Defaults to <see cref="DspUdpClient.CreateNew"/>.</para>
    /// </summary>
    public UdpClientFactoryDelegate UdpClientFactory
    {
        internal get => this.udpClientFactory;
        set => this.udpClientFactory = value ?? throw new InvalidOperationException("You need to supply a valid UDP client factory method.");
    }
    private UdpClientFactoryDelegate udpClientFactory = DspUdpClient.CreateNew;

    /// <summary>
    /// Whether to log unknown events or not. Defaults to true.
    /// </summary>
    public bool LogUnknownEvents { internal get; set; } = true;

    /// <summary>
    /// Whether to log unknown auditlog types and change keys or not. Defaults to true.
    /// </summary>
    public bool LogUnknownAuditlogs { internal get; set; } = true;

    /// <summary>
    /// Creates a new configuration with default values.
    /// </summary>
    public DiscordConfiguration()
    { }

    /// <summary>
    /// Creates a clone of another discord configuration.
    /// </summary>
    /// <param name="other">Client configuration to clone.</param>
    public DiscordConfiguration(DiscordConfiguration other)
    {
        this.UdpClientFactory = other.UdpClientFactory;
        this.LogUnknownEvents = other.LogUnknownEvents;
        this.LogUnknownAuditlogs = other.LogUnknownAuditlogs;
    }
}
