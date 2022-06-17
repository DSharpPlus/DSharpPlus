using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DSharpPlus.Core.RestEntities;

namespace DSharpPlus.Core.JsonConverters
{
    public class DiscordSnowflakeConverter : JsonConverter<DiscordSnowflake>
    {
        /// <inheritdoc/>
        public override DiscordSnowflake? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => ulong.TryParse(reader.GetString()!, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong snowflake) ? snowflake : null;

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, DiscordSnowflake value, JsonSerializerOptions options) => writer.WriteStringValue(value.Value.ToString(CultureInfo.InvariantCulture));
    }
}
