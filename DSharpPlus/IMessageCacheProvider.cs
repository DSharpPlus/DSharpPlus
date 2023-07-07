// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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

namespace DSharpPlus;
public interface IMessageCacheProvider
{
    /// <summary>
    /// Add a <see cref="DiscordMessage"/> object to the cache.
    /// </summary>
    /// <param name="message">The <see cref="DiscordMessage"/> object to add to the cache.</param>
    void Add(DiscordMessage message);

    /// <summary>
    /// Remove the <see cref="DiscordMessage"/> object associated with the message ID from the cache. 
    /// </summary>
    /// <param name="messageId">The ID of the message to remove from the cache.</param>
    void Remove(ulong messageId);

    /// <summary>
    /// Try to get a <see cref="DiscordMessage"/> object associated with the message ID from the cache.
    /// </summary>
    /// <param name="messageId">The ID of the message to retrieve from the cache.</param>
    /// <param name="message">The <see cref="DiscordMessage"/> object retrieved from the cache, if it exists; null otherwise.</param>
    /// <returns><see langword="true"/> if the message can be retrieved from the cache, <see langword="false"/> otherwise.</returns>
    bool TryGet(ulong messageId, out DiscordMessage? message);
}
