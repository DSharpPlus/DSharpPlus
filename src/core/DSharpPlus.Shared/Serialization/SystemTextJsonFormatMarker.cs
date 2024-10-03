// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Serialization;

/// <summary>
/// Provides a marker type to force <see cref="SerializationService{T}"/> to use System.Text.Json.
/// </summary>
public abstract class SystemTextJsonFormatMarker
{
    // can't inherit it
    internal SystemTextJsonFormatMarker() { }
}
