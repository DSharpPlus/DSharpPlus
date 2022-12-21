using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalGuildWelcomeScreen
{
    /// <summary>
    /// The server description shown in the welcome screen.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The channels shown in the welcome screen, up to 5.
    /// </summary>
    [JsonPropertyName("welcome_channels")]
    public IReadOnlyList<InternalGuildWelcomeScreenChannel> WelcomeChannels { get; init; } = Array.Empty<InternalGuildWelcomeScreenChannel>();
}
