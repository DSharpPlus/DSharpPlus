using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a channel.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class ChannelEditModel : BaseEditModel
{
    /// <summary>
    /// Sets the channel's new name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Sets the channel's new position.
    /// </summary>
    public int? Position { get; set; }

    /// <summary>
    /// Sets the channel's new topic.
    /// </summary>
    public Optional<string> Topic { get; set; }

    /// <summary>
    /// Sets whether the channel is to be marked as NSFW.
    /// </summary>
    public bool? Nsfw { get; set; }

    /// <summary>
    /// Sets the parent of this channel.
    /// This should be channel with <see cref="DiscordChannel.Type"/> set to <see cref="DiscordChannelType.Category"/>.
    /// </summary>
    public Optional<DiscordChannel> Parent { get; set; }

    /// <summary>
    /// Sets the voice channel's new bitrate.
    /// </summary>
    public int? Bitrate { get; set; }

    /// <summary>
    /// Sets the voice channel's new user limit.
    /// Setting this to 0 will disable the user limit.
    /// </summary>
    public int? Userlimit { get; set; }

    /// <summary>
    /// Sets the channel's new slow mode timeout.
    /// Setting this to null or 0 will disable slow mode.
    /// </summary>
    public Optional<int?> PerUserRateLimit { get; set; }

    /// <summary>
    /// Sets the voice channel's region override.
    /// Setting this to null will set it to automatic.
    /// </summary>
    public Optional<DiscordVoiceRegion> RtcRegion { get; set; }

    /// <summary>
    /// Sets the voice channel's video quality.
    /// </summary>
    public DiscordVideoQualityMode? QualityMode { get; set; }

    /// <summary>
    /// Sets the channel's type.
    /// This can only be used to convert between text and news channels.
    /// </summary>
    public Optional<DiscordChannelType> Type { get; set; }

    /// <summary>
    /// Sets the channel's permission overwrites.
    /// </summary>
    public List<DiscordOverwriteBuilder> PermissionOverwrites { get; set; }

    /// <summary>
    /// Sets the channel's auto-archive duration.
    /// </summary>
    public Optional<DiscordAutoArchiveDuration?> DefaultAutoArchiveDuration { get; set; }

    /// <summary>
    /// Sets the channel's flags (forum channels and posts only).
    /// </summary>
    public Optional<DiscordChannelFlags> Flags { get; set; }

    /// <summary>
    /// Sets the channel's available tags.
    /// </summary>
    public List<DiscordForumTagBuilder> AvailableTags { get; set; }

    /// <summary>
    /// Sets the channel's default reaction, if any.
    /// </summary>
    public Optional<DefaultReaction?> DefaultReaction { get; set; }

    /// <summary>
    /// Sets the default slowmode of newly created threads, but does not retroactively update.
    /// </summary>
    /// <remarks>https://discord.com/developers/docs/resources/channel#modify-channel-json-params-guild-channel</remarks>
    public Optional<int> DefaultThreadRateLimit { get; set; }

    /// <summary>
    /// Sets the default sort order of posts in this channel.
    /// </summary>
    public Optional<DiscordDefaultSortOrder?> DefaultSortOrder { get; set; }

    /// <summary>
    /// Sets the default layout of posts in this channel.
    /// </summary>
    public Optional<DiscordDefaultForumLayout> DefaultForumLayout { get; set; }

    internal ChannelEditModel() { }
}
