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
            Converters =
            {
                new OptionalJsonConverter(),
                new DiscordUri.DiscordUriJsonConverter(),
            },
            ContractResolver = new OptionalJsonContractResolver()
        });

        /// <summary>Serializes the specified object to a JSON string.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value)
        {
            return SerializeObjectInternal(value, null, Serializer);
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
            
            // DIRTY HACK to support both serializing both fields and properties, if you know a better way to do this
            // write me at cadrekucra@gmx.com!!!!
            var propPresent = optionalProp != null;
            var optionalField = !propPresent
                ? property.DeclaringType.GetTypeInfo().GetDeclaredField(property.UnderlyingName)
                : null;
            
            property.ShouldSerialize = instance => // instance here is the declaring (parent) type
            {
                // this is the Optional<T> object
                var optionalValue = propPresent ? optionalProp.GetValue(instance) : optionalField.GetValue(instance);
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
            var genericType = objectType.GenericTypeArguments[0];

            var constructor = objectType.GetTypeInfo().DeclaredConstructors
                .Single(e => e.GetParameters()[0].ParameterType == genericType);
            
            try
            {
                return constructor.Invoke(new[] { Convert.ChangeType(reader.Value, genericType)});
            }
            catch
            {
                return existingValue;
            }
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            if (!objectType.GetTypeInfo().IsGenericType) return false;

            return objectType.GetGenericTypeDefinition() == typeof(Optional<>);
        }
    }
}