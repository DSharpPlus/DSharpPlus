// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an attachment to a message.
/// </summary>
public interface IAttachment : IPartialAttachment
{
    /// <inheritdoc cref="IPartialAttachment.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialAttachment.Filename"/>
    public new string Filename { get; }

    /// <inheritdoc cref="IPartialAttachment.Size"/>
    public new int Size { get; }

    /// <inheritdoc cref="IPartialAttachment.Url"/>
    public new string Url { get; }

    /// <inheritdoc cref="IPartialAttachment.ProxyUrl"/>
    public new string ProxyUrl { get; }

    // partial access routing

    /// <inheritdoc/>
    Optional<Snowflake> IPartialAttachment.Id => this.Id;

    /// <inheritdoc/>
    Optional<string> IPartialAttachment.Filename => this.Filename;

    /// <inheritdoc/>
    Optional<int> IPartialAttachment.Size => this.Size;

    /// <inheritdoc/>
    Optional<string> IPartialAttachment.Url => this.Url;

    /// <inheritdoc/>
    Optional<string> IPartialAttachment.ProxyUrl => this.ProxyUrl;
}
