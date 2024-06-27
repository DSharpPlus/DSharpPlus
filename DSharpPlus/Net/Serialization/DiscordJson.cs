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
            Converters = new JsonConverter[] { new ISO8601DateTimeOffsetJsonConverter(), new DiscordPermissionsJsonConverter() }
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
