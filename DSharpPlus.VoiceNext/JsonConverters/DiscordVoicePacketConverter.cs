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
using DSharpPlus.VoiceNext.VoiceGatewayEntities;
using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.JsonConverters
{
    public class DiscordVoicePacketConverter : JsonConverter<DiscordVoicePacket>
    {
        public override DiscordVoicePacket? ReadJson(JsonReader reader, Type objectType, DiscordVoicePacket? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var completeVoicePacketSpan = (Span<byte>)reader.ReadAsBytes();
            return new DiscordVoicePacket()
            {
                VersionAndFlags = completeVoicePacketSpan[0],
                PayloadType = completeVoicePacketSpan[1],
                Sequence = BinaryPrimitives.ReadUInt16BigEndian(completeVoicePacketSpan.Slice(2, 2)),
                Timestamp = BinaryPrimitives.ReadUInt32BigEndian(completeVoicePacketSpan.Slice(4, 4)),
                SSRC = BinaryPrimitives.ReadUInt32BigEndian(completeVoicePacketSpan.Slice(8, 4)),
                EncryptedAudio = completeVoicePacketSpan.Slice(12).ToArray()
            };
        }

        public override void WriteJson(JsonWriter writer, DiscordVoicePacket? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            Span<byte> completeVoicePacketSpan = new(new byte[value.EncryptedAudio.Length + 12], 0, value.EncryptedAudio.Length + 12);
            completeVoicePacketSpan[0] = value.VersionAndFlags;
            completeVoicePacketSpan[1] = value.PayloadType;
            BinaryPrimitives.WriteUInt16BigEndian(completeVoicePacketSpan.Slice(2, 2), value.Sequence);
            BinaryPrimitives.WriteUInt32BigEndian(completeVoicePacketSpan.Slice(4, 4), value.Timestamp);
            BinaryPrimitives.WriteUInt32BigEndian(completeVoicePacketSpan.Slice(8, 4), value.SSRC);
            value.EncryptedAudio.CopyTo(completeVoicePacketSpan.Slice(12));

            writer.WriteValue(completeVoicePacketSpan.ToArray());
        }
    }
}
