using System.Collections.Generic;
using System.IO;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a guild.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class GuildEditModel : BaseEditModel
{
    /// <summary>
    /// The new guild name.
    /// </summary>
    public Optional<string> Name { get; set; }

    /// <summary>
    /// The new guild voice region.
    /// </summary>
    public Optional<DiscordVoiceRegion> Region { get; set; }

    /// <summary>
    /// The new guild icon.
    /// </summary>
    public Optional<Stream> Icon { get; set; }

    /// <summary>
    /// The new guild verification level.
    /// </summary>
    public Optional<DiscordVerificationLevel> VerificationLevel { get; set; }

    /// <summary>
    /// The new guild default message notification level.
    /// </summary>
    public Optional<DiscordDefaultMessageNotifications> DefaultMessageNotifications { get; set; }

    /// <summary>
    /// The new guild MFA level.
    /// </summary>
    public Optional<DiscordMfaLevel> MfaLevel { get; set; }

    /// <summary>
    /// The new guild explicit content filter level.
    /// </summary>
    public Optional<DiscordExplicitContentFilter> ExplicitContentFilter { get; set; }

    /// <summary>
    /// The new AFK voice channel.
    /// </summary>
    public Optional<DiscordChannel> AfkChannel { get; set; }

    /// <summary>
    /// The new AFK timeout time in seconds.
    /// </summary>
    public Optional<int> AfkTimeout { get; set; }

    /// <summary>
    /// The new guild owner.
    /// </summary>
    public Optional<DiscordMember> Owner { get; set; }

    /// <summary>
    /// The new guild splash.
    /// </summary>
    public Optional<Stream> Splash { get; set; }

    /// <summary>
    /// The new guild system channel.
    /// </summary>
    public Optional<DiscordChannel> SystemChannel { get; set; }

    /// <summary>
    /// The new guild rules channel.
    /// </summary>
    public Optional<DiscordChannel> RulesChannel { get; set; }

    /// <summary>
    /// The new guild public updates channel.
    /// </summary>
    public Optional<DiscordChannel> PublicUpdatesChannel { get; set; }

    /// <summary>
    /// The new guild preferred locale.
    /// </summary>
    public Optional<string> PreferredLocale { get; set; }

    /// <summary>
    /// The new description of the guild
    /// </summary>
    public Optional<string> Description { get; set; }

    /// <summary>
    /// The new discovery splash image of the guild
    /// </summary>
    public Optional<string> DiscoverySplash { get; set; }

    /// <summary>
    /// A list of <see href="https://discord.com/developers/docs/resources/guild#guild-object-guild-features">guild features</see>
    /// </summary>
    public Optional<List<string>> Features { get; set; }

    /// <summary>
    /// The new banner of the guild
    /// </summary>
    public Optional<Stream> Banner { get; set; }

    /// <summary>
    /// The new system channel flags for the guild
    /// </summary>
    public Optional<DiscordSystemChannelFlags> SystemChannelFlags { get; set; }

    internal GuildEditModel() { }
}
