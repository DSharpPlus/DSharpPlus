using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalGuildMember
{
    /// <summary>
    /// The user this guild member represents.
    /// </summary>
    /// <remarks>
    /// The <c>user</c> object won't be included in the member object attached to <c>MESSAGE_CREATE</c> and <c>MESSAGE_UPDATE</c> gateway events.
    /// </remarks>
    [JsonPropertyName("user")]
    public Optional<InternalUser> User { get; init; }

    /// <summary>
    /// This user's guild nickname.
    /// </summary>
    [JsonPropertyName("nick")]
    public Optional<string?> Nick { get; init; }

    /// <summary>
    /// The member's guild avatar hash.
    /// </summary>
    [JsonPropertyName("avatar")]
    public Optional<string?> Avatar { get; init; }

    /// <summary>
    /// Array of <see cref="InternalRole"/> object ids.
    /// </summary>
    [JsonPropertyName("roles")]
    public required IReadOnlyList<Snowflake> Roles { get; init; }

    /// <summary>
    /// When the user joined the guild.
    /// </summary>
    /// <remarks>
    /// Resets when the member leaves and rejoins the guild.
    /// </remarks>
    [JsonPropertyName("joined_at")]
    public required DateTimeOffset JoinedAt { get; init; }

    /// <summary>
    /// When the user started boosting the guild.
    /// </summary>
    /// <remarks>
    /// Can also be seen as "Nitro boosting since".
    /// </remarks>
    [JsonPropertyName("premium_since")]
    public Optional<DateTimeOffset?> PremiumSince { get; init; }

    /// <summary>
    /// Whether the user is deafened in voice channels.
    /// </summary>
    /// <remarks>
    /// This could be a self or server deafen.
    /// </remarks>
    [JsonPropertyName("deaf")]
    public required bool Deaf { get; init; }

    /// <summary>
    /// Whether the user is muted in voice channels.
    /// </summary>
    /// <remarks>
    /// This could be a self or server mute.
    /// </remarks>
    [JsonPropertyName("mute")]
    public required bool Mute { get; init; }

    /// <summary>
    /// Whether the user has not yet passed the guild's membership screening requirements.
    /// </summary>
    [JsonPropertyName("pending")]
    public Optional<bool> Pending { get; init; }

    /// <summary>
    /// Total permissions of the member in the channel, including overwrites, returned when in the interaction object.
    /// </summary>
    /// <remarks>
    /// This is only available on an interaction, such as a Slash Command.
    /// </remarks>
    [JsonPropertyName("permissions")]
    public Optional<DiscordPermissions> Permissions { get; init; }

    /// <summary>
    /// When the user's timeout will expire and the user will be able to communicate in the guild again, 
    /// null or a time in the past if the user is not timed out.
    /// </summary>
    /// <remarks>
    /// Could also be seen as "muted until".
    /// </remarks>
    [JsonPropertyName("communication_disabled_until")]
    public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; init; }

    /// <summary>
    /// The id of the guild.
    /// </summary>
    /// <remarks>
    /// Only sent in the GUILD_MEMBER_ADD and GUILD_MEMBER_UPDATE payloads.
    /// </remarks>
    public Optional<Snowflake> GuildId { get; init; }
}
