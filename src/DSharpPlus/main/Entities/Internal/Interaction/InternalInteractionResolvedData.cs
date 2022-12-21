using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalInteractionResolvedData
{
    /// <summary>
    /// The ids and User objects.
    /// </summary>
    [JsonPropertyName("users")]
    public Optional<IReadOnlyDictionary<InternalSnowflake, InternalUser>> Users { get; init; }

    /// <summary>
    /// The ids and partial Member objects.
    /// </summary>
    [JsonPropertyName("members")]
    public Optional<IReadOnlyDictionary<InternalSnowflake, InternalGuildMember>> Members { get; init; }

    /// <summary>
    /// The ids and Role objects.
    /// </summary>
    [JsonPropertyName("roles")]
    public Optional<IReadOnlyDictionary<InternalSnowflake, InternalRole>> Roles { get; init; }

    /// <summary>
    /// The ids and partial Channel objects.
    /// </summary>
    [JsonPropertyName("channels")]
    public Optional<IReadOnlyDictionary<InternalSnowflake, InternalChannel>> Channels { get; init; }

    /// <summary>
    /// The ids and partial Message objects.
    /// </summary>
    [JsonPropertyName("messages")]
    public Optional<IReadOnlyDictionary<InternalSnowflake, InternalMessage>> Messages { get; init; }

    /// <summary>
    /// The ids and attachment objects.
    /// </summary>
    [JsonPropertyName("attachments")]
    public Optional<IReadOnlyDictionary<InternalSnowflake, InternalAttachment>> Attachments { get; init; }
}
