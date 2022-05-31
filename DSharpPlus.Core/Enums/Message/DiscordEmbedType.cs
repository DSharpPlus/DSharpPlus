using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSharpPlus.Core.Enums
{
    /// <summary>
    /// Embed types are "loosely defined" and, for the most part, are not used by our clients for rendering. Embed attributes power what is rendered.
    /// </summary>
    /// <remarks>
    /// Embed types should be considered deprecated and might be removed in a future API version.
    /// </remarks>
    [JsonConverter(typeof(StringEnumConverter)), Obsolete("Embed types are \"loosely defined\" and, for the most part, are not used by our clients for rendering. Embed attributes power what is rendered. Embed types should be considered deprecated and might be removed in a future API version.")]
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
