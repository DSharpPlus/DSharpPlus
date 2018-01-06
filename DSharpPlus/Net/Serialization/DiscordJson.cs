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
        private static readonly OptionalJsonConverter OptionalJsonConverter = new OptionalJsonConverter();

        /// <summary>Serializes the specified object to a JSON string.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, OptionalJsonConverter);
        }
    }

    public class OptionalJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var typeInfo = value.GetType().GetTypeInfo();
            var hasValue = (bool) typeInfo.GetDeclaredProperty("HasValue").GetValue(value);

            if (!hasValue) return;
            var t = JToken.FromObject(typeInfo.GetDeclaredProperty("Value").GetValue(value));
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
            if (!objectType.GetTypeInfo().IsGenericType) return false;

            if (objectType.GetGenericTypeDefinition() != typeof(Optional<>)) return false;

            var firstGeneric = objectType.GetTypeInfo().GenericTypeArguments[0];

            if (!firstGeneric.GetTypeInfo().IsGenericType) return false;

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