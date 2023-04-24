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
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Gets guild integrations
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    public Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guildId)
        => ApiClient.GetGuildIntegrationsAsync(guildId);

    /// <summary>
    /// Creates guild integration
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="type">Integration type</param>
    /// <param name="id">Integration id</param>
    public Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guildId, string type, ulong id)
        => ApiClient.CreateGuildIntegrationAsync(guildId, type, id);

    /// <summary>
    /// Modifies a guild integration
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="integrationId">Integration ID</param>
    /// <param name="expireBehavior">Expiration behaviour</param>
    /// <param name="expireGracePeriod">Expiration grace period</param>
    /// <param name="enableEmoticons">Whether to enable emojis for this integration</param>
    public Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guildId, ulong integrationId, int expireBehavior, int expireGracePeriod, bool enableEmoticons)
        => ApiClient.ModifyGuildIntegrationAsync(guildId, integrationId, expireBehavior, expireGracePeriod, enableEmoticons);

    /// <summary>
    /// Removes a guild integration
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="integration">Integration to remove</param>
    /// <param name="reason">Reason why this integration was removed</param>
    public Task DeleteGuildIntegrationAsync(ulong guildId, DiscordIntegration integration, string? reason = null)
        => ApiClient.DeleteGuildIntegrationAsync(guildId, integration, reason);

    /// <summary>
    /// Syncs guild integration
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="integrationId">Integration ID</param>
    public Task SyncGuildIntegrationAsync(ulong guildId, ulong integrationId)
        => ApiClient.SyncGuildIntegrationAsync(guildId, integrationId);

    /// <summary>
    /// Gets assets from an application
    /// </summary>
    /// <param name="application">Application to get assets from</param>
    public Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
        => ApiClient.GetApplicationAssetsAsync(application);
}
