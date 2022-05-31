using System;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.JsonConverters
{
    public class DiscordSnowflakeConverter : JsonConverter<DiscordSnowflake>
    {
        public override DiscordSnowflake? ReadJson(JsonReader reader, Type objectType, DiscordSnowflake? existingValue, bool hasExistingValue, JsonSerializer serializer) => reader.TokenType == JsonToken.Null
            ? null
            : ulong.TryParse(reader.Value!.ToString(), out ulong snowflake) ? new DiscordSnowflake(snowflake) : null;

        public override void WriteJson(JsonWriter writer, DiscordSnowflake? value, JsonSerializer serializer) => writer.WriteValue(value?.ToString());
    }
}
