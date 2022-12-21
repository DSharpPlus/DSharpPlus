using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalInteractionResolvedData
{
    /// <summary>
    /// The ids and User objects.
    /// </summary>
    [JsonPropertyName("users")]
    public Optional<IReadOnlyDictionary<Snowflake, InternalUser>> Users { get; init; }

    /// <summary>
    /// The ids and partial Member objects.
    /// </summary>
    [JsonPropertyName("members")]
    public Optional<IReadOnlyDictionary<Snowflake, InternalGuildMember>> Members { get; init; }

    /// <summary>
    /// The ids and Role objects.
    /// </summary>
    [JsonPropertyName("roles")]
    public Optional<IReadOnlyDictionary<Snowflake, InternalRole>> Roles { get; init; }

    /// <summary>
    /// The ids and partial Channel objects.
    /// </summary>
    [JsonPropertyName("channels")]
    public Optional<IReadOnlyDictionary<Snowflake, InternalChannel>> Channels { get; init; }

    /// <summary>
    /// The ids and partial Message objects.
    /// </summary>
    [JsonPropertyName("messages")]
    public Optional<IReadOnlyDictionary<Snowflake, InternalMessage>> Messages { get; init; }

    /// <summary>
    /// The ids and attachment objects.
    /// </summary>
    [JsonPropertyName("attachments")]
    public Optional<IReadOnlyDictionary<Snowflake, InternalAttachment>> Attachments { get; init; }
}
