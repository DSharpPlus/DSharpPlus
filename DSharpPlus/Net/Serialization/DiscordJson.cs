using System;
using System.Globalization;
using System.IO;
using System.Text;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Serialization
{
    public static class DiscordJson
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            ContractResolver = new OptionalJsonContractResolver(),
            DateParseHandling = DateParseHandling.None,
            Converters = new[] { new ISO8601DateTimeOffsetJsonConverter() }
        });

        /// <summary>Serializes the specified object to a JSON string.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value) => SerializeObjectInternal(value, null, _serializer);

        /// <summary>Populates an object with the values from a JSON node.</summary>
        /// <param name="value">The token to populate the object with.</param>
        /// <param name="target">The object to populate.</param>
        public static void PopulateObject(JToken value, object target)
        {
            using var reader = value.CreateReader();
            _serializer.Populate(reader, target);
        }

        /// <summary>
        /// Converts this token into an object, passing any properties through extra <see cref="JsonConverter"/>s if
        /// needed.
        /// </summary>
        /// <param name="token">The token to convert</param>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <returns>The converted token</returns>
        public static T ToDiscordObject<T>(this JToken token) => token.ToObject<T>(_serializer);

        private static string SerializeObjectInternal(object value, Type type, JsonSerializer jsonSerializer)
        {
            var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonTextWriter.Formatting = jsonSerializer.Formatting;
                jsonSerializer.Serialize(jsonTextWriter, value, type);
            }
            return stringWriter.ToString();
        }
    }
}
