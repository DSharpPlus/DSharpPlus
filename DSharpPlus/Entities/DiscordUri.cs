using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace DSharpPlus.Net;

/// <summary>
/// An URI in a Discord embed doesn't necessarily conform to the RFC 3986. If it uses the <c>attachment://</c>
/// protocol, it mustn't contain a trailing slash to be interpreted correctly as an embed attachment reference by
/// Discord.
/// </summary>
[JsonConverter(typeof(DiscordUriJsonConverter))]
public class DiscordUri
{
    private readonly object _value;

    /// <summary>
    /// The type of this URI.
    /// </summary>
    public DiscordUriType Type { get; }

    internal DiscordUri(Uri value)
    {
        this._value = value ?? throw new ArgumentNullException(nameof(value));
        this.Type = DiscordUriType.Standard;
    }

    internal DiscordUri(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (IsStandard(value))
        {
            this._value = new Uri(value);
            this.Type = DiscordUriType.Standard;
        }
        else
        {
            this._value = value;
            this.Type = DiscordUriType.NonStandard;
        }
    }

    // can be changed in future
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsStandard(string value) => !value.StartsWith("attachment://");

    /// <summary>
    /// Returns a string representation of this DiscordUri.
    /// </summary>
    /// <returns>This DiscordUri, as a string.</returns>
    public override string ToString() => this._value.ToString();

    /// <summary>
    /// Converts this DiscordUri into a canonical representation of a <see cref="Uri"/> if it can be represented as
    /// such, throwing an exception otherwise.
    /// </summary>
    /// <returns>A canonical representation of this DiscordUri.</returns>
    /// <exception cref="UriFormatException">If <see cref="Type"/> is not <see cref="DiscordUriType.Standard"/>, as
    /// that would mean creating an invalid Uri, which would result in loss of data.</exception>
    public Uri ToUri()
        => this.Type == DiscordUriType.Standard
            ? this._value as Uri
            : throw new UriFormatException(
                $@"DiscordUri ""{this._value}"" would be invalid as a regular URI, please the {nameof(this.Type)} property first.");

    internal sealed class DiscordUriJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue((value as DiscordUri)._value);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            object? val = reader.Value;
            return val == null
                ? null
                : (object)(val is not string s
                ? throw new JsonReaderException("DiscordUri value invalid format! This is a bug in DSharpPlus. " +
                                              $"Include the type in your bug report: [[{reader.TokenType}]]")
                : IsStandard(s)
                ? new DiscordUri(new Uri(s))
                : new DiscordUri(s));
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(DiscordUri);
    }
}

public enum DiscordUriType : byte
{
    /// <summary>
    /// Represents a URI that conforms to RFC 3986, meaning it's stored internally as a <see cref="Uri"/> and will
    /// contain a trailing slash after the domain name.
    /// </summary>
    Standard,

    /// <summary>
    /// Represents a URI that does not conform to RFC 3986, meaning it's stored internally as a plain string and
    /// should be treated as one.
    /// </summary>
    NonStandard
}
