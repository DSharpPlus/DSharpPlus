// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Serialization;

public sealed class ImageDataConverter : JsonConverter<ImageData>
{
    /// <summary>
    /// Deserializing image data is unsupported.
    /// </summary>
    public override ImageData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

    public override void Write
    (
        Utf8JsonWriter writer,
        ImageData value,
        JsonSerializerOptions options
    )
    {
        using ArrayPoolBufferWriter<byte> bufferWriter = new(65536);

        value.WriteTo(bufferWriter);

        writer.WriteStringValue(bufferWriter.WrittenSpan);
    }
}
