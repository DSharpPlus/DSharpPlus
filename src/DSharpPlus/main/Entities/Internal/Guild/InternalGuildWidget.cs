using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalGuildWidget
{
    /// <summary>
    /// The guild id.
    /// </summary>
    [JsonPropertyName("id")]
    public required Snowflake Id { get; init; } 

    /// <summary>
    /// The guild name (2-100 characters).
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; } 

    /// <summary>
    /// The instant invite for the guilds specified widget invite channel.
    /// </summary>
    [JsonPropertyName("instant_invite")]
    public string? InstantInvite { get; init; }

    /// <summary>
    /// The voice and stage channels which are accessible by @everyone
    /// </summary>
    [JsonPropertyName("channels")]
    public required IReadOnlyList<InternalChannel> Channels { get; init; } 

    /// <summary>
    /// A list of special widget user objects that includes users presence (Limit 100).
    /// </summary>
    /// <remarks>
    /// The fields <see cref="InternalUser.Id"/>, <see cref="InternalUser.Discriminator"/> and 
    /// <see cref="InternalUser.Avatar"/> are anonymized to prevent abuse.
    /// <b>This field is full of <see cref="InternalUser"/> objects, NOT <see cref="InternalGuildMember"/> objects.</b>
    /// </remarks>
    [JsonPropertyName("members")]
    public required IReadOnlyList<InternalUser> Members { get; init; } 

    /// <summary>
    /// The number of online members in this guild.
    /// </summary>
    [JsonPropertyName("presence_count")]
    public required int PresenceCount { get; init; }
}
