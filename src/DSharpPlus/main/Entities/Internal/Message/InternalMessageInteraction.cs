using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// This is sent on the message object when the message is a response to an Interaction without an existing message.
/// </summary>
/// <remarks>
/// This means responses to Message Components do not include this property, instead including a message reference object as components always exist on preexisting messages.
/// </remarks>
public sealed record InternalMessageInteraction
{
    /// <summary>
    /// The id of the interaction.
    /// </summary>
    [JsonPropertyName("id")]
    public InternalSnowflake Id { get; init; } = null!;

    /// <summary>
    /// The type of interaction.
    /// </summary>
    [JsonPropertyName("type")]
    public DiscordInteractionType Type { get; init; }

    /// <summary>
    /// The name of the application command.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The user who invoked the interaction.
    /// </summary>
    [JsonPropertyName("user")]
    public InternalUser User { get; init; } = null!;

    /// <summary>
    /// The member who invoked the interaction in the guild.
    /// </summary>
    [JsonPropertyName("member")]
    public Optional<InternalGuildMember> Member { get; init; }
}
