using System;
using System.IO;

using DSharpPlus.Net.Udp;

namespace DSharpPlus;

/// <summary>
/// Represents configuration for <see cref="DiscordClient"/>.
/// </summary>
public sealed class DiscordConfiguration
{
    /// <summary>
    /// <para>Sets the level of compression for WebSocket traffic.</para>
    /// <para>Disabling this option will increase the amount of traffic sent via WebSocket. Setting <see cref="GatewayCompressionLevel.Payload"/> will enable compression for READY and GUILD_CREATE payloads. Setting <see cref="Stream"/> will enable compression for the entire WebSocket stream, drastically reducing amount of traffic.</para>
    /// <para>Defaults to <see cref="GatewayCompressionLevel.None"/>.</para>
    /// </summary>
    /// <remarks>This property's default has been changed from <see cref="GatewayCompressionLevel.Stream"/> due to a bug in the client wherein rapid reconnections causes payloads to be decompressed into a mangled string.</remarks>
    // Here be dragons, ye who use compression.
    public GatewayCompressionLevel GatewayCompressionLevel { internal get; set; } = GatewayCompressionLevel.None;

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
        this.GatewayCompressionLevel = other.GatewayCompressionLevel;
        this.UdpClientFactory = other.UdpClientFactory;
        this.LogUnknownEvents = other.LogUnknownEvents;
        this.LogUnknownAuditlogs = other.LogUnknownAuditlogs;
    }
}
