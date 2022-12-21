using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalGuildWidget
{
    /// <summary>
    /// The guild id.
    /// </summary>
    [JsonPropertyName("id")]
    public InternalSnowflake Id { get; init; } = null!;

    /// <summary>
    /// The guild name (2-100 characters).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The instant invite for the guilds specified widget invite channel.
    /// </summary>
    [JsonPropertyName("instant_invite")]
    public string? InstantInvite { get; init; }

    /// <summary>
    /// The voice and stage channels which are accessible by @everyone
    /// </summary>
    [JsonPropertyName("channels")]
    public IReadOnlyList<InternalChannel> Channels { get; init; } = Array.Empty<InternalChannel>();

    /// <summary>
    /// A list of special widget user objects that includes users presence (Limit 100).
    /// </summary>
    /// <remarks>
    /// The fields <see cref="InternalUser.Id"/>, <see cref="InternalUser.Discriminator"/> and <see cref="InternalUser.Avatar"/>, are anonymized to prevent abuse.
    /// <b>This field is full of <see cref="InternalUser"/> objects, NOT <see cref="InternalGuildMember"/> objects.</b>
    /// </remarks>
    [JsonPropertyName("members")]
    public IReadOnlyList<InternalUser> Members { get; init; } = Array.Empty<InternalUser>();

    /// <summary>
    /// The number of online members in this guild.
    /// </summary>
    [JsonPropertyName("presence_count")]
    public int PresenceCount { get; init; }
}
