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
    public class DiscordForumChannelJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new ();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var job = JObject.Load(reader);

            DiscordChannel channel;
            var channelType = job["type"].ToObject<ChannelType>();

            if (channelType is ChannelType.GuildForum)
            {
                // Type erasure is almost unheard of in C#, but you never know...
                var chn = new DiscordForumChannel();
                serializer.Populate(job.CreateReader(), chn);

                channel = chn;
            }
            // May or not be necessary. Better safe than sorry.
            else if (channelType is ChannelType.NewsThread or ChannelType.PrivateThread or ChannelType.PublicThread)
            {
                var chn = new DiscordThreadChannel();
                serializer.Populate(job.CreateReader(), chn);

                channel = chn;
            }
            else
            {
                channel = new DiscordChannel();
                serializer.Populate(job.CreateReader(), channel);
            }

            return channel;
        }

        public override bool CanConvert(Type objectType) => objectType.IsAssignableFrom(typeof(DiscordChannel));
    }
}
