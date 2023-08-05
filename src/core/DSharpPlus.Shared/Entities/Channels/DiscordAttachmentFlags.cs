// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents flags for an attachment.
/// </summary>
[Flags]
public enum DiscordAttachmentFlags
{
    /// <summary>
    /// Indicates that this attachment has been edited using the remix feature on mobile.
    /// </summary>
    IsRemix = 1 << 2
}
