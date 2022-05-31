using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DSharpPlus.Core.RestEntities;

namespace DSharpPlus.Core.JsonConverters
{
    /// <summary>
    /// <see cref="JsonConverterFactory"/> for <see cref="OptionalJsonConverter{T}"/>.
    /// </summary>
    public sealed class OptionalJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsConstructedGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

        /// <inheritdoc/>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter)typeof(OptionalJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()).GetConstructor(Type.EmptyTypes)!.Invoke(null)!;
    }
}
