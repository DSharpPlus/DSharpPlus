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
    /// Gets current user object
    /// </summary>
    public Task<DiscordUser> GetCurrentUserAsync()
        => ApiClient.GetCurrentUserAsync();

    /// <summary>
    /// Gets user object
    /// </summary>
    /// <param name="user">User ID</param>
    public Task<DiscordUser> GetUserAsync(ulong user)
        => ApiClient.GetUserAsync(user);

    /// <summary>
    /// Modifies current user
    /// </summary>
    /// <param name="username">New username</param>
    /// <param name="avatarBase64">New avatar (base64)</param>
    public async Task<DiscordUser> ModifyCurrentUserAsync(string? username, string? avatarBase64)
        => new DiscordUser(await ApiClient.ModifyCurrentUserAsync(username, avatarBase64)) { Discord = this };

    /// <summary>
    /// Modifies current user
    /// </summary>
    /// <param name="username">username</param>
    /// <param name="avatar">avatar</param>
    public Task<DiscordUser> ModifyCurrentUserAsync(string? username = null, Stream? avatar = null)
    {
        string? avatarBase64 = null;
        if (avatar != null)
        {
            using ImageTool imgtool = new(avatar);
            avatarBase64 = imgtool.GetBase64();
        }

        return ModifyCurrentUserAsync(username, avatarBase64);
    }

    /// <summary>
    /// Gets current user's guilds
    /// </summary>
    /// <param name="limit">Limit of guilds to get</param>
    /// <param name="before">Gets guild before ID</param>
    /// <param name="after">Gets guilds after ID</param>
    public Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
        => ApiClient.GetCurrentUserGuildsAsync(limit, before, after);

    /// <summary>
    /// Gets current user's connections
    /// </summary>
    public Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync()
        => ApiClient.GetUsersConnectionsAsync();

    /// <summary>
    /// Updates the current user's suppress state in a stage channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="suppress">Toggles the suppress state.</param>
    /// <param name="requestToSpeakTimestamp">Sets the time the user requested to speak.</param>
    public Task UpdateCurrentUserVoiceStateAsync(ulong guildId, ulong channelId, bool? suppress, DateTimeOffset? requestToSpeakTimestamp = null)
        => ApiClient.UpdateCurrentUserVoiceStateAsync(guildId, channelId, suppress, requestToSpeakTimestamp);

    /// <summary>
    /// Updates a member's suppress state in a stage channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="userId">The ID of the member.</param>
    /// <param name="channelId">The ID of the stage channel.</param>
    /// <param name="suppress">Toggles the member's suppress state.</param>
    public Task UpdateUserVoiceStateAsync(ulong guildId, ulong userId, ulong channelId, bool? suppress)
        => ApiClient.UpdateUserVoiceStateAsync(guildId, userId, channelId, suppress);
}
