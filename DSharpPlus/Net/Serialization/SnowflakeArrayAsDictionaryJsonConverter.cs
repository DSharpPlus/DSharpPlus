using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Serialization
{
    /// <summary>
    /// Used for a <see cref="Dictionary{TKey,TValue}"/> or <see cref="ConcurrentDictionary{TKey,TValue}"/> mapping
    /// <see cref="ulong"/> to any class extending <see cref="SnowflakeObject"/>. When serializing, discards the ulong
    /// keys and writes only the values. When deserializing, pulls the keys from <see cref="SnowflakeObject.Id"/>.
    /// </summary>
    internal class SnowflakeArrayAsDictionaryJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var type = writer.GetType().GetTypeInfo();
                JToken.FromObject(type.GetDeclaredProperty("Values").GetValue(value)).WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var constructor = objectType.GetTypeInfo().DeclaredConstructors
                .FirstOrDefault(e => e.GetParameters().Length == 0);

            var dict = constructor.Invoke(new object[] {});

            // the default name of an indexer is "Item"
            var properties = objectType.GetTypeInfo().GetDeclaredProperty("Item");

            var entries = (IEnumerable) serializer.Deserialize(reader, objectType.GenericTypeArguments[1].MakeArrayType());
            foreach (var entry in entries)
            {
                properties.SetValue(dict, entry, new object[] { (entry as SnowflakeObject).Id });
            }
            
            return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            var genericTypedef = objectType.GetGenericTypeDefinition();
            return (genericTypedef == typeof(Dictionary<,>) || genericTypedef == typeof(ConcurrentDictionary<,>))
                && objectType.GenericTypeArguments[0] == typeof(ulong) 
                && typeof(SnowflakeObject).GetTypeInfo().IsAssignableFrom(objectType.GenericTypeArguments[1].GetTypeInfo());
        }
    }
}