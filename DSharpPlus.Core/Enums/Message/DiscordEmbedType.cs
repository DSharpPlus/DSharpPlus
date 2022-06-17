using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Enums
{
    /// <summary>
    /// Embed types are "loosely defined" and, for the most part, are not used by our clients for rendering. Embed attributes power what is rendered.
    /// </summary>
    /// <remarks>
    /// Embed types should be considered deprecated and might be removed in a future API version.
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter)), Obsolete("Embed types are \"loosely defined\" and, for the most part, are not used by our clients for rendering. Embed attributes power what is rendered. Embed types should be considered deprecated and might be removed in a future API version.", false)]
    public enum DiscordEmbedType
    {
        Rich,
        Image,
        Video,
        Gifv,
        Article,
        Link
    }
}
