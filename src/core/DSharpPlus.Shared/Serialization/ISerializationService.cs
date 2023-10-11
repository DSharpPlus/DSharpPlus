// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Serialization;

/// <summary>
/// Represents an abstraction over model serialization.
/// </summary>
/// <typeparam name="T">The library component a given instance is associated with.</typeparam>
public interface ISerializationService<out T>
{
    /// <summary>
    /// Serializes a given serialization model to the given writer.
    /// </summary>
    /// <remarks>
    /// This method serializes the library data models specifically, and may not exhibit correct behaviour
    /// for other types.
    /// </remarks>
    public void SerializeModel<TModel>
    (
        TModel model,
        ArrayPoolBufferWriter<byte> target
    )
        where TModel : notnull;

    /// <summary>
    /// Deserializes a serialization model from the provided data.
    /// </summary>
    /// <remarks>
    /// This method deserializes the library data models specifically, and may not exhibit correct behaviour
    /// for other types.
    /// </remarks>
    public TModel DeserializeModel<TModel>(ReadOnlySpan<byte> data)
        where TModel : notnull;
}
