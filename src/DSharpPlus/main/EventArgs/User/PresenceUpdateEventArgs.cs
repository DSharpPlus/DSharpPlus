// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.PresenceUpdated"/> event.
/// </summary>
public class PresenceUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the user whose presence was updated.
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the user's new game.
    /// </summary>
    public DiscordActivity Activity { get; internal set; }

    /// <summary>
    /// Gets the user's status.
    /// </summary>
    public UserStatus Status { get; internal set; }

    /// <summary>
    /// Gets the user's old presence.
    /// </summary>
    public DiscordPresence PresenceBefore { get; internal set; }

    /// <summary>
    /// Gets the user's new presence.
    /// </summary>
    public DiscordPresence PresenceAfter { get; internal set; }

    /// <summary>
    /// Gets the user prior to presence update.
    /// </summary>
    public DiscordUser UserBefore { get; internal set; }

    /// <summary>
    /// Gets the user after the presence update.
    /// </summary>
    public DiscordUser UserAfter { get; internal set; }

    internal PresenceUpdateEventArgs() : base() { }
}
