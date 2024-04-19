// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a poll object.
/// </summary>
public interface IPoll
{
    /// <summary>
    /// The question this poll asks. The text is limited to 300 characters.
    /// </summary>
    public IPollMedia Question { get; }

    /// <summary>
    /// The answers available within this poll, between 1 and 10.
    /// </summary>
    public IReadOnlyList<IPollAnswer> Answers { get; }

    /// <summary>
    /// The timestamp at which this poll expires.
    /// </summary>
    public DateTimeOffset Expiry { get; }

    /// <summary>
    /// Specifies whether this poll allows selecting multiple answers.
    /// </summary>
    public bool AllowMultiselect { get; }

    /// <summary>
    /// The layout type of this poll. "Defaults to... DEFAULT!" - Discord.
    /// </summary>
    public Optional<DiscordPollLayoutType> LayoutType { get; }
}
