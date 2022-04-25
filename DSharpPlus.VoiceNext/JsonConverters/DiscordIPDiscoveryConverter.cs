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
using System.Buffers.Binary;
using System.Text;
using DSharpPlus.VoiceNext.VoiceGatewayEntities;
using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.JsonConverters
{
    public class DiscordIPDiscoveryConverter : JsonConverter<DiscordIPDiscovery>
    {
        public override DiscordIPDiscovery? ReadJson(JsonReader reader, Type objectType, DiscordIPDiscovery? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var ipDiscoverySpan = (Span<byte>)reader.ReadAsBytes();
            return new DiscordIPDiscovery()
            {
                Type = BinaryPrimitives.ReadUInt16BigEndian(ipDiscoverySpan.Slice(0, 2)),
                Length = BinaryPrimitives.ReadUInt16BigEndian(ipDiscoverySpan.Slice(2, 2)),
                SSRC = BinaryPrimitives.ReadUInt32BigEndian(ipDiscoverySpan.Slice(4, 4)),
                Address = Encoding.UTF8.GetString(ipDiscoverySpan.Slice(8, 64).ToArray()),
                Port = BinaryPrimitives.ReadUInt16BigEndian(ipDiscoverySpan.Slice(72, 2))
            };
        }

        public override void WriteJson(JsonWriter writer, DiscordIPDiscovery? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            Span<byte> ipDiscoverySpan = stackalloc byte[74];
            BinaryPrimitives.WriteUInt16BigEndian(ipDiscoverySpan.Slice(0, 2), value.Type);
            BinaryPrimitives.WriteUInt16BigEndian(ipDiscoverySpan.Slice(2, 2), value.Length);
            BinaryPrimitives.WriteUInt32BigEndian(ipDiscoverySpan.Slice(4, 4), value.SSRC);
            Encoding.UTF8.GetBytes(value.Address).CopyTo(ipDiscoverySpan.Slice(8, 64));
            BinaryPrimitives.WriteUInt16BigEndian(ipDiscoverySpan.Slice(72, 2), value.Port);

            writer.WriteValue(ipDiscoverySpan.ToArray());
        }
    }
}
