using System;
using System.Linq;
using System.Reflection;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.JsonConverters
{
    public class OptionalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Optional<>).IsAssignableFrom(objectType);
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            Type? genericType = objectType.GenericTypeArguments[0];
            ConstructorInfo? constructor = objectType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(e => e.GetParameters()[0].ParameterType == genericType);
            return constructor!.Invoke(new[] { serializer.Deserialize(reader, genericType) });
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }

            Optional<object> optional = (Optional<object>)value!;
            if (!optional.HasValue)
            {
                writer.WriteNull();
            }
            else
            {
                serializer.Serialize(writer, optional.Value);
            }
        }
    }
}
