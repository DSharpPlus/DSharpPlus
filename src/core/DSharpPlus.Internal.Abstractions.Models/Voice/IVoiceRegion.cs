// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents metadata about a voice region.
/// </summary>
public interface IVoiceRegion
{
    /// <summary>
    /// The unique identifier of this region.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The name of this region.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Indicates whether this is the closest server to the current user's client.
    /// </summary>
    public bool Optimal { get; }

    /// <summary>
    /// Indicates whether this is a deprecated voice region.
    /// </summary>
    public bool Deprecated { get; }

    /// <summary>
    /// Indicates whether this is a custom voice region, used for official events etc.
    /// </summary>
    public bool Custom { get; }
}
