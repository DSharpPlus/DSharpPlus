// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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

namespace DSharpPlus.Entities
{
    /// <summary>
    /// A component to attatch to a message.
    /// </summary>
    [JsonConverter(typeof(DiscordComponentConverter))]
    public class DiscordComponent
    {
        /// <summary>
        /// The type of component this represents.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public ComponentType Type { get; internal set; } = ComponentType.ActionRow;

        /// <summary>
        /// The Id of this compopnent, if applicable. Not applicable on ActionRow(s) and Link buttons.
        /// </summary>
        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomId { get; internal set; }

        internal DiscordComponent() { }

    }
    internal sealed class DiscordComponentConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var job = JObject.Load(reader);
            var type = job["type"]?.ToObject<ComponentType>();

            if (type == null)
                throw new ArgumentException($"Value {reader} does not have a component type specifier");

            var cmp = type switch
            {
                ComponentType.ActionRow => new DiscordActionRowComponent(),
                ComponentType.Button => new DiscordButtonComponent(),
                ComponentType.Select => new DiscordSelectComponent(),
                _ => new DiscordComponent() { Type = type.Value }
            };

            // Populate the existing component with the values in the JObject. This avoids a recursive JsonConverter loop
            using var jreader = job.CreateReader();
            serializer.Populate(jreader, cmp);

            return cmp;
        }

        public override bool CanConvert(Type objectType) => typeof(DiscordComponent).IsAssignableFrom(objectType);
    }
}
