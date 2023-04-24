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
using DSharpPlus.Net.Models;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Gets a sticker from a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="stickerId">The ID of the sticker.</param>
    public Task<DiscordMessageSticker> GetGuildStickerAsync(ulong guildId, ulong stickerId)
        => ApiClient.GetGuildStickerAsync(guildId, stickerId);

    /// <summary>
    /// Gets a sticker by its ID.
    /// </summary>
    /// <param name="stickerId">The ID of the sticker.</param>
    public Task<DiscordMessageSticker> GetStickerAsync(ulong stickerId)
        => ApiClient.GetStickerAsync(stickerId);

    /// <summary>
    /// Gets a collection of sticker packs that may be used by nitro users.
    /// </summary>
    public Task<IReadOnlyList<DiscordMessageStickerPack>> GetStickerPacksAsync()
        => ApiClient.GetStickerPacksAsync();

    /// <summary>
    /// Gets a list of stickers from a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    public Task<IReadOnlyList<DiscordMessageSticker>> GetGuildStickersAsync(ulong guildId)
        => ApiClient.GetGuildStickersAsync(guildId);

    /// <summary>
    /// Creates a sticker in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="name">The name of the sticker.</param>
    /// <param name="description">The description of the sticker.</param>
    /// <param name="tags">The tags of the sticker.</param>
    /// <param name="imageContents">The image content of the sticker.</param>
    /// <param name="format">The image format of the sticker.</param>
    /// <param name="reason">The reason this sticker is being created.</param>
    public Task<DiscordMessageSticker> CreateGuildStickerAsync
    (
        ulong guildId,
        string name,
        string description,
        string tags,
        Stream imageContents,
        StickerFormat format,
        string? reason = null
    )
    {
        string contentType;
        string extension;

        if (format is StickerFormat.PNG or StickerFormat.APNG)
        {
            contentType = "image/png";
            extension = "png";
        }
        else
        {
            contentType = "application/json";
            extension = "json";
        }

        return ApiClient.CreateGuildStickerAsync(
            guildId,
            name,
            description ?? string.Empty,
            tags,
            new DiscordMessageFile(null, imageContents, null, extension, contentType),
            reason
        );
    }

    /// <summary>
    /// Modifies a sticker in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="stickerId">The ID of the sticker.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">Reason for audit log.</param>
    public Task<DiscordMessageSticker> ModifyGuildStickerAsync(ulong guildId, ulong stickerId, Action<StickerEditModel> action, string? reason = null)
    {
        StickerEditModel stickerEditModel = new();
        action(stickerEditModel);
        return ApiClient.ModifyStickerAsync(
            guildId,
            stickerId,
            stickerEditModel.Name,
            stickerEditModel.Description,
            stickerEditModel.Tags,
            reason
        );
    }

    /// <summary>
    /// Deletes a sticker in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="stickerId">The ID of the sticker.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns></returns>
    public Task DeleteGuildStickerAsync(ulong guildId, ulong stickerId, string? reason = null)
        => ApiClient.DeleteStickerAsync(guildId, stickerId, reason);
}
