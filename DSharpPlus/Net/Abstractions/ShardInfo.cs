// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Abstractions
{
    /// <summary>
    /// Represents data for identify payload's shard info.
    /// </summary>
    [JsonConverter(typeof(ShardInfoConverter))]
    internal sealed class ShardInfo
    {
        /// <summary>
        /// Gets or sets this client's shard id.
        /// </summary>
        public int ShardId { get; set; }

        /// <summary>
        /// Gets or sets the total shard count for this token.
        /// </summary>
        public int ShardCount { get; set; }
    }

    internal sealed class ShardInfoConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sinfo = value as ShardInfo;
            var obj = new object[] { sinfo.ShardId, sinfo.ShardCount };
            serializer.Serialize(writer, obj);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var arr = this.ReadArrayObject(reader, serializer);
            return new ShardInfo
            {
                ShardId = (int)arr[0],
                ShardCount = (int)arr[1],
            };
        }

        private JArray ReadArrayObject(JsonReader reader, JsonSerializer serializer)
        {
            return serializer.Deserialize<JToken>(reader) is not JArray arr || arr.Count != 2
                ? throw new JsonSerializationException("Expected array of length 2")
                : arr;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(ShardInfo);
    }
}
