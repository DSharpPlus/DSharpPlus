using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DSharpPlus.Net.Serialization
{
    public static class DiscordJson
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            ContractResolver = new OptionalJsonContractResolver()
        });

        /// <summary>Serializes the specified object to a JSON string.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value)
        {
            return SerializeObjectInternal(value, null, Serializer);
        }

        /// <summary>Populates an object with the values from a JSON node.</summary>
        /// <param name="value">The token to populate the object with.</param>
        /// <param name="target">The object to populate.</param>
        public static void PopulateObject(JToken value, object target)
        {
            using (var reader = value.CreateReader())
            {
                Serializer.Populate(reader, target);
            }
        }
        
        /// <summary>
        /// Converts this token into an object, passing any properties through extra <see cref="JsonConverter"/>s if
        /// needed.
        /// </summary>
        /// <param name="token">The token to convert</param>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <returns>The converted token</returns>
        public static T ToDiscordObject<T>(this JToken token)
        {
            return token.ToObject<T>(Serializer);
        }
        
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