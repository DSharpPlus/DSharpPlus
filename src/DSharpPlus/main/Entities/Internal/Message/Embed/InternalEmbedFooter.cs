using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalEmbedFooter
{
    /// <summary>
    /// The footer text.
    /// </summary>
    /// <remarks>
    /// Max 2048 characters.
    /// </remarks>
    [JsonPropertyName("text")]
    public string Text { get; init; } = null!;

    /// <summary>
    /// The url of footer icon (only supports http(s) and attachments).
    /// </summary>
    [JsonPropertyName("icon_url")]
    public Optional<string> IconUrl { get; init; }

    /// <summary>
    /// A proxied url of footer icon.
    /// </summary>
    [JsonPropertyName("proxy_icon_url")]
    public Optional<string> ProxyIconUrl { get; init; }
}
