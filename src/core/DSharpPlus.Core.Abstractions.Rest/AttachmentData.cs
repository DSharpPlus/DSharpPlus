// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.IO.Pipelines;

namespace DSharpPlus.Core.Abstractions.Rest;

/// <summary>
/// Contains the information necessary for sending an attachment to Discord.
/// </summary>
public readonly record struct AttachmentData
{
    /// <summary>
    /// The filename as uploaded to Discord.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// An optional description/alt text for this file.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// A pipe with the contents of the file.
    /// </summary>
    public required PipeReader Content { get; init; }
}
