using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using DSharpPlus.Core.Entities;

namespace DSharpPlus.Core.JsonConverters
{
    using ShouldSerializeDel = Func<object, object?, bool>;

    public static class DiscordJsonTypeInfoResolver
    {
        public static IJsonTypeInfoResolver Default { get; } = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                Modifier
            }
        };

        public static void Modifier(JsonTypeInfo typeInfo)
        {
            foreach (JsonPropertyInfo prop in typeInfo.Properties)
            {
                if (prop.PropertyType.IsConstructedGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
                {
                    prop.ShouldSerialize = (ShouldSerializeDel)typeof(IgnoreCondition<>)
                        .MakeGenericType(prop.PropertyType.GetGenericArguments()[0])
                        .GetField(nameof(IgnoreCondition<int>.Delegate))!
                        .GetValue(null)!;
                }
            }
        }

        private static class IgnoreCondition<T>
        {
            public static readonly ShouldSerializeDel Delegate = ShouldIgnore;
            private static bool ShouldIgnore(object _, object? value) => Unsafe.Unbox<Optional<T>>(value!).HasValue;
        }
    }
}
