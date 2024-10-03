// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Serialization;

/// <summary>
/// Represents a serialization backend managed by <see cref="SerializationService{T}"/>.
/// </summary>
public interface ISerializationBackend
{
    /// <summary>
    /// A unique ID for this backend, used to distinguish if multiple different backends are in use at once.
    /// </summary>
    public static abstract string Id { get; }

    /// <summary>
    /// Serializes a given serialization model to the given writer.
    /// </summary>
    /// <remarks>
    /// This method serializes the library data models specifically, and may not exhibit correct behaviour
    /// for other types.
    /// </remarks>
    public void SerializeModel<TModel>(TModel model, ArrayPoolBufferWriter<byte> target)
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
