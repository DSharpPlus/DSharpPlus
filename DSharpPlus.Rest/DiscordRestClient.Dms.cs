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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Joins a group DM
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="nickname">DM nickname</param>
    public Task JoinGroupDmAsync(ulong channelId, string nickname)
        => ApiClient.AddGroupDmRecipientAsync(channelId, CurrentUser.Id, Configuration.Token, nickname);

    /// <summary>
    /// Adds a member to a group DM
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="accessToken">User's access token</param>
    /// <param name="nickname">Nickname for user</param>
    public Task GroupDmAddRecipientAsync(ulong channelId, ulong userId, string accessToken, string nickname)
        => ApiClient.AddGroupDmRecipientAsync(channelId, userId, accessToken, nickname);

    /// <summary>
    /// Leaves a group DM
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    public Task LeaveGroupDmAsync(ulong channelId)
        => ApiClient.RemoveGroupDmRecipientAsync(channelId, CurrentUser.Id);

    /// <summary>
    /// Removes a member from a group DM
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="userId">User ID</param>
    public Task GroupDmRemoveRecipientAsync(ulong channelId, ulong userId)
        => ApiClient.RemoveGroupDmRecipientAsync(channelId, userId);

    /// <summary>
    /// Creates a group DM
    /// </summary>
    /// <param name="accessTokens">Access tokens</param>
    /// <param name="nicks">Nicknames per user</param>
    public Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> accessTokens, IDictionary<ulong, string> nicks)
        => ApiClient.CreateGroupDmAsync(accessTokens, nicks);

    /// <summary>
    /// Creates a group DM with current user
    /// </summary>
    /// <param name="accessTokens">Access tokens</param>
    /// <param name="nicks">Nicknames</param>
    public Task<DiscordDmChannel> CreateGroupDmWithCurrentUserAsync(IEnumerable<string> accessTokens, IDictionary<ulong, string> nicks)
    {
        List<string> accessTokensList = accessTokens.ToList();
        accessTokensList.Add(Configuration.Token);
        return ApiClient.CreateGroupDmAsync(accessTokensList, nicks);
    }

    /// <summary>
    /// Creates a DM
    /// </summary>
    /// <param name="recipientId">Recipient user ID</param>
    public Task<DiscordDmChannel> CreateDmAsync(ulong recipientId)
        => ApiClient.CreateDmAsync(recipientId);
}
