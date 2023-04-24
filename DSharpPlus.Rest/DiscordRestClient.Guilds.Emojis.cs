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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Gets a guild's emojis.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    public Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guildId)
        => ApiClient.GetGuildEmojisAsync(guildId);

    /// <summary>
    /// Gets a guild emoji.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="emojiId">The ID of the emoji.</param>
    public Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guildId, ulong emojiId)
        => ApiClient.GetGuildEmojiAsync(guildId, emojiId);

    /// <summary>
    /// Creates an emoji in a guild.
    /// </summary>
    /// <param name="name">Name of the emoji.</param>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="image">Image to use as the emoji.</param>
    /// <param name="roles">Roles for which the emoji will be available.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public Task<DiscordGuildEmoji> CreateEmojiAsync(ulong guildId, string name, Stream image, IEnumerable<ulong>? roles = null, string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        name = name.Trim();
        if (name.Length < 2 || name.Length > 50)
        {
            throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.");
        }
        else if (image == null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        using ImageTool imgtool = new ImageTool(image);
        string image64 = imgtool.GetBase64();
        return ApiClient.CreateGuildEmojiAsync(guildId, name, image64, roles, reason);
    }

    /// <summary>
    /// Modifies a guild's emoji.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="emojiId">The ID of the emoji.</param>
    /// <param name="name">New name of the emoji.</param>
    /// <param name="roles">Roles for which the emoji will be available.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guildId, ulong emojiId, string name, IEnumerable<ulong>? roles = null, string? reason = null)
        => ApiClient.ModifyGuildEmojiAsync(guildId, emojiId, name, roles, reason);

    /// <summary>
    /// Deletes a guild's emoji.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="emojiId">The ID of the emoji.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public Task DeleteGuildEmojiAsync(ulong guildId, ulong emojiId, string? reason = null)
        => ApiClient.DeleteGuildEmojiAsync(guildId, emojiId, reason);
}
