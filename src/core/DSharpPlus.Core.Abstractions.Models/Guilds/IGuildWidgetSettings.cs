// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Contains settings for this guild's widget.
/// </summary>
public interface IGuildWidgetSettings
{
    /// <summary>
    /// Indicates whether the widget is enabled.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// The snowflake identifier of the widget channel.
    /// </summary>
    public Snowflake? ChannelId { get; }
}
