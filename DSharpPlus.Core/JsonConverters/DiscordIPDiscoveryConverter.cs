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
using DSharpPlus.Core.VoiceGatewayEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.JsonConverters
{
    public class DiscordIPDiscoveryConverter : JsonConverter<DiscordIPDiscovery>
    {
        public override DiscordIPDiscovery? ReadJson(JsonReader reader, Type objectType, DiscordIPDiscovery? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            byte[]? completeVoicePacket = reader.ReadAsBytes();
            return completeVoicePacket is null ? null : new DiscordIPDiscovery()
            {
                Type = BinaryPrimitives.ReadUInt16BigEndian(completeVoicePacket.AsSpan(0, 2)),
                Length = BinaryPrimitives.ReadUInt16BigEndian(completeVoicePacket.AsSpan(2, 2)),
                SSRC = BinaryPrimitives.ReadUInt32BigEndian(completeVoicePacket.AsSpan(4, 4)),
                Address = Encoding.UTF8.GetString(completeVoicePacket.AsSpan(8, 64)),
                Port = BinaryPrimitives.ReadUInt16BigEndian(completeVoicePacket.AsSpan(72, 2))
            };
        }

        public override void WriteJson(JsonWriter writer, DiscordIPDiscovery? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            Span<byte> completeVoicePacket = stackalloc byte[74];
            BinaryPrimitives.WriteUInt16BigEndian(completeVoicePacket[..2], value.Type);
            BinaryPrimitives.WriteUInt16BigEndian(completeVoicePacket.Slice(2, 2), value.Length);
            BinaryPrimitives.WriteUInt32BigEndian(completeVoicePacket.Slice(4, 4), value.SSRC);
            Encoding.UTF8.GetBytes(value.Address, completeVoicePacket.Slice(8, 64));
            BinaryPrimitives.WriteUInt16BigEndian(completeVoicePacket.Slice(72, 2), value.Port);

            writer.WriteValue(completeVoicePacket.ToArray());
        }
    }
}
