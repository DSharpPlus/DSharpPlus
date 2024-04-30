namespace DSharpPlus.Net;
using System;
using Newtonsoft.Json;

/// <summary>
/// An URI in a Discord embed doesn't necessarily conform to the RFC 3986. If it uses the <c>attachment://</c>
/// protocol, it mustn't contain a trailing slash to be interpreted correctly as an embed attachment reference by
/// Discord.
/// </summary>
[JsonConverter(typeof(DiscordUriJsonConverter))]
public readonly record struct DiscordUri
{
    private readonly string uri;

    /// <summary>
    /// The type of this URI.
    /// </summary>
    public DiscordUriType Type { get; }

    internal DiscordUri(Uri value)
    {
        uri = value.AbsoluteUri;
        Type = DiscordUriType.Standard;
    }

    internal DiscordUri(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        uri = value;
        Type = IsStandard(uri) ? DiscordUriType.Standard : DiscordUriType.NonStandard;
    }

    private static bool IsStandard(string value) => !value.StartsWith("attachment://");

    /// <summary>
    /// Returns a string representation of this DiscordUri.
    /// </summary>
    /// <returns>This DiscordUri, as a string.</returns>
    public override string? ToString() => uri;

    /// <summary>
    /// Converts this DiscordUri into a canonical representation of a <see cref="Uri"/> if it can be represented as
    /// such, throwing an exception otherwise.
    /// </summary>
    /// <returns>A canonical representation of this DiscordUri.</returns>
    /// <exception cref="UriFormatException">If <see cref="Type"/> is not <see cref="DiscordUriType.Standard"/>, as
    /// that would mean creating an invalid Uri, which would result in loss of data.</exception>
    public Uri? ToUri()
        => Type == DiscordUriType.Standard
            ? new Uri(uri)
            : throw new UriFormatException(
                $@"DiscordUri ""{uri}"" would be invalid as a regular URI, please check the {nameof(Type)} property first.");

    internal sealed class DiscordUriJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            => writer.WriteValue((value as DiscordUri?)?.uri);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            => reader.Value is null ? null : new DiscordUri((string)reader.Value);

        public override bool CanConvert(Type objectType) => objectType == typeof(DiscordUri);
    }
}

public enum DiscordUriType : byte
{
    /// <summary>
    /// Represents a URI that conforms to RFC 3986.
    /// </summary>
    Standard,

    /// <summary>
    /// Represents a URI that does not conform to RFC 3986.
    /// </summary>
    NonStandard
}
