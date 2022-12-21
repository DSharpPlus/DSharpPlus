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
/// Represents arguments for <see cref="DiscordClient.GuildMembersChunked"/> event.
/// </summary>
public class GuildMembersChunkEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that requested this chunk.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the collection of members returned from this chunk.
    /// </summary>
    public IReadOnlyCollection<DiscordMember> Members { get; internal set; }

    /// <summary>
    /// Gets the current chunk index from the response.
    /// </summary>
    public int ChunkIndex { get; internal set; }

    /// <summary>
    /// Gets the total amount of chunks for the request. 
    /// </summary>
    public int ChunkCount { get; internal set; }

    /// <summary>
    /// Gets the collection of presences returned from this chunk, if specified.
    /// </summary>
    public IReadOnlyCollection<DiscordPresence> Presences { get; internal set; }

    /// <summary>
    /// Gets the returned Ids that were not found in the chunk, if specified.
    /// </summary>
    public IReadOnlyCollection<ulong> NotFound { get; internal set; }

    /// <summary>
    /// Gets the unique string used to identify the request, if specified.
    /// </summary>
    public string Nonce { get; set; }

    internal GuildMembersChunkEventArgs() : base() { }
}
