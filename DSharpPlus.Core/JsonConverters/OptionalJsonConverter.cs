using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DSharpPlus.Core.Entities;

namespace DSharpPlus.Core.JsonConverters
{
    /// <summary>
    /// <see cref="JsonConverter{T}"/> for <see cref="Optional{T}"/>.
    /// Will throw if the value is <see cref="Optional{T}.Empty"/> when serializing.
    /// </summary>
    /// <typeparam name="T"> The type of the value held by the <see cref="Optional{T}"/>. </typeparam>
    public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
    {
        /// <inheritdoc/>
        public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(ref reader, options)!;

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                throw new InvalidOperationException("Serializing Optional<T>.Empty into JSON is not supported.");
            }

            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}
