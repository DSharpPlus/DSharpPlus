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
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Serialization
{
    internal sealed class DiscordComponentJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var job = JObject.Load(reader);
            var type = job["type"]?.ToDiscordObject<ComponentType>();

            if (type == null)
                throw new ArgumentException($"Value {reader} does not have a component type specifier");

            var cmp = type switch
            {
                ComponentType.ActionRow => new DiscordActionRowComponent(),
                ComponentType.Button when (int)job["style"] is 5 => new DiscordLinkButtonComponent(),
                ComponentType.Button => new DiscordButtonComponent(),
                ComponentType.StringSelect => new DiscordSelectComponent(),
                ComponentType.FormInput => new TextInputComponent(),
                ComponentType.UserSelect => new DiscordUserSelectComponent(),
                ComponentType.RoleSelect => new DiscordRoleSelectComponent(),
                ComponentType.MentionableSelect => new DiscordMentionableSelectComponent(),
                ComponentType.ChannelSelect => new DiscordChannelSelectComponent(),
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
