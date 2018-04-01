using System;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DSharpPlus.Net.Serialization
{
    public static class DiscordJson
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = {new OptionalJsonConverter()},
            ContractResolver = new OptionalJsonContractResolver()
        };

        /// <summary>Serializes the specified object to a JSON string.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, JsonSerializerSettings);
        }
    }

    public class OptionalJsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            var type = property.PropertyType;
            
            if (!type.GetTypeInfo().IsGenericType)
                return property;

            if (type.GetGenericTypeDefinition() != typeof(Optional<>))
                return property;
            
            // we cache the PropertyInfo object here (it's captured in closure). we don't have direct 
            // access to the property value so we have to reflect into it from the parent instance
            // we use UnderlyingName instead of PropertyName in case the C# name is different from the Json name.
            var optionalProp = property.DeclaringType.GetTypeInfo().GetDeclaredProperty(property.UnderlyingName);
            property.ShouldSerialize = instance => // instance here is the declaring (parent) type
            {
                // this is the Optional<T> object
                var optionalValue = optionalProp.GetValue(instance);
                // get the HasValue property of the Optional<T> object and cast it to a bool, and only serialize it if
                // it's present
                return (bool)optionalValue.GetType().GetTypeInfo().GetDeclaredProperty("HasValue").GetValue(optionalValue);
            };
    
            return property;
        }
    }

    public class OptionalJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var typeInfo = value.GetType().GetTypeInfo();
            // we don't check for HasValue here since it's checked above
            var val = typeInfo.GetDeclaredProperty("Value").GetValue(value);
            // JToken.FromObject will throw if `null` so we manually write a null value.
            if (val == null)
            {
                // you can read serializer.NullValueHandling here, but unfortunately you can **not** skip serialization
                // here, or else you will get a nasty JsonWriterException, so we just ignore its value and manually
                // write the null.
                writer.WriteToken(JsonToken.Null);
            }
            else
            {
                // convert the value to a JSON object and write it to the property value.
                var t = JToken.FromObject(val);
                t.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException(
                "Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            if (!objectType.GetTypeInfo().IsGenericType) return false;

            return objectType.GetGenericTypeDefinition() == typeof(Optional<>);
        }
    }
}