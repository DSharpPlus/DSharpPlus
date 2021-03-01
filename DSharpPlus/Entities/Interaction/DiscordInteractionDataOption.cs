using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents parameters for interaction commands. 
    /// </summary>
    public sealed class DiscordInteractionDataOption
    {
        /// <summary>
        /// Gets the name of this interaction parameter.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the value of this interaction parameter. 
        /// <para>This can be cast to an <see langword="int"></see>, <see langword="bool"></see>, <see langword="string"></see> or <see langword="ulong"/> depending on the <see cref="DiscordInteractionDataOption.Name"/></para>
        /// </summary>
        [JsonProperty("value")]
        [JsonConverter(typeof(DiscordInteractionOptionTypeConverter))]
        public object Value { get; internal set; }

        /// <summary>
        /// Gets the additional parameters if this parameter is a subcommand.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }
    }

    internal sealed class DiscordInteractionOptionTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(string) || objectType == typeof(int) || objectType == typeof(bool);

        public override object ReadJson(JsonReader reader, Type objectType, object value, JsonSerializer serializer)
        {
            if (reader.Value is string str)
            {
                if (ulong.TryParse(str, out var ul))
                    return ul;
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value);
    }
}
