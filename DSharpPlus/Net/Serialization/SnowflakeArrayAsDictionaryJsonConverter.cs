// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
    /// <see cref="ulong"/> to any class extending <see cref="SnowflakeObject"/> (or, as a special case,
    /// <see cref="DiscordVoiceState"/>). When serializing, discards the ulong
    /// keys and writes only the values. When deserializing, pulls the keys from <see cref="SnowflakeObject.Id"/> (or,
    /// in the case of <see cref="DiscordVoiceState"/>, <see cref="DiscordVoiceState.UserId"/>.
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
                var type = value.GetType().GetTypeInfo();
                JToken.FromObject(type.GetDeclaredProperty("Values").GetValue(value)).WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var constructor = objectType.GetTypeInfo().DeclaredConstructors
                .FirstOrDefault(e => !e.IsStatic && e.GetParameters().Length == 0);

            var dict = constructor.Invoke(new object[] {});

            // the default name of an indexer is "Item"
            var properties = objectType.GetTypeInfo().GetDeclaredProperty("Item");

            var entries = (IEnumerable) serializer.Deserialize(reader, objectType.GenericTypeArguments[1].MakeArrayType());
            foreach (var entry in entries)
            {
                properties.SetValue(dict, entry, new object[]
                {
                    (entry as SnowflakeObject)?.Id
                    ?? (entry as DiscordVoiceState)?.UserId
                    ?? throw new InvalidOperationException($"Type {entry?.GetType()} is not deserializable")
                });
            }

            return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            var genericTypedef = objectType.GetGenericTypeDefinition();
            if (genericTypedef != typeof(Dictionary<,>) && genericTypedef != typeof(ConcurrentDictionary<,>)) return false;
            if (objectType.GenericTypeArguments[0] != typeof(ulong)) return false;

            var valueParam = objectType.GenericTypeArguments[1];
            return typeof(SnowflakeObject).GetTypeInfo().IsAssignableFrom(valueParam.GetTypeInfo()) ||
                   valueParam == typeof(DiscordVoiceState);
        }
    }
}
