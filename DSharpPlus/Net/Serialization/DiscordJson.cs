using System;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Serialization
{
    public static class DiscordJson
    {
        /// <summary>Serializes the specified object to a JSON string.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, new OptionalJsonConverter());
        }
    }

    public class OptionalJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value.GetType();
#if NETSTANDARD1_1 || NETSTANDARD1_3
            var hasValue = (bool) type.GetTypeInfo().DeclaredProperties.FirstOrDefault(e => e.Name == "HasValue")
                .GetValue(value);
#else
            var hasValue = (bool)type.GetProperty("HasValue").GetValue(value);
#endif

            if (!hasValue) return;

#if NETSTANDARD1_1 || NETSTANDARD1_3
            var t = JToken.FromObject(type.GetTypeInfo().DeclaredProperties.FirstOrDefault(e => e.Name == "Value")
                .GetValue(value));
#else
            var t = JToken.FromObject(type.GetProperty("Value").GetValue(value));
#endif
            t.WriteTo(writer);
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
#if NETSTANDARD1_1 || NETSTANDARD1_3
            if (!objectType.GetTypeInfo().IsGenericType) return false;
#else
            if (!objectType.IsGenericType) return false;
#endif

            if (objectType.GetGenericTypeDefinition() != typeof(Optional<>)) return false;

#if NETSTANDARD1_1 || NETSTANDARD1_3
            var firstGeneric = objectType.GetTypeInfo().GenericTypeArguments[0];
#else
            var firstGeneric = objectType.GetGenericArguments()[0];
#endif

#if NETSTANDARD1_1 || NETSTANDARD1_3
            if (!firstGeneric.GetTypeInfo().IsGenericType) return false;
#else
            if (!firstGeneric.IsGenericType) return false;
#endif

            return firstGeneric.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

//        private static void DisplayTypeInfo(Type t)
//        {
//            Console.WriteLine($"\r\n{t}");
//            Console.WriteLine($"\tIs this a generic type definition? {t.IsGenericTypeDefinition}");
//            Console.WriteLine($"\tIs it a generic type? {t.IsGenericType}");
//            var typeArguments = t.GetGenericArguments();
//            Console.WriteLine($"\tList type arguments ({typeArguments.Length}):");
//            foreach (var tParam in typeArguments)
//            {
//                Console.WriteLine($"\t\t{tParam}");
//            }
//        }
    }
}