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

using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageCreated"/> event.
/// </summary>
public class MessageCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the message that was created.
    /// </summary>
    public DiscordMessage Message { get; internal set; }

    /// <summary>
    /// Gets the channel this message belongs to.
    /// </summary>
    public DiscordChannel Channel
        => this.Message.Channel;

    /// <summary>
    /// Gets the guild this message belongs to.
    /// </summary>
    public DiscordGuild Guild
        => this.Channel.Guild;

    /// <summary>
    /// Gets the author of the message.
    /// </summary>
    public DiscordUser Author
        => this.Message.Author;

    /// <summary>
    /// Gets the collection of mentioned users.
    /// </summary>
    public IReadOnlyList<DiscordUser> MentionedUsers { get; internal set; }

    /// <summary>
    /// Gets the collection of mentioned roles.
    /// </summary>
    public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }

    /// <summary>
    /// Gets the collection of mentioned channels.
    /// </summary>
    public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }

    internal MessageCreateEventArgs() : base() { }
}
