// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// A top-level component that allows displaying an uploaded file.
/// </summary>
public interface IFileComponent : IComponent
{
    /// <summary>
    /// The type of this component.
    /// </summary>
    public DiscordMessageComponentType Type { get; }

    /// <summary>
    /// An optional numeric identifier for this component.
    /// </summary>
    public Optional<int> Id { get; }

    /// <summary>
    /// The file to display. In this case, only uploads are supported via <c>attachment://</c> reference, not other arbitrary URLs.
    /// </summary>
    public IUnfurledMediaItem File { get; }

    /// <summary>
    /// Indicates whether the file should be spoilered. Defaults to false.
    /// </summary>
    public Optional<bool> Spoiler { get; }
}
