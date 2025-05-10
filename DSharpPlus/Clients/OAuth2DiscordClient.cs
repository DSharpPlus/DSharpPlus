using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Clients;

/// <summary>
/// Represents a thin client to serve as the base for OAuth2 usage.
/// </summary>
public sealed class OAuth2DiscordClient : BaseDiscordClient
{
    private readonly Dictionary<ulong, DiscordGuild> guilds = [];

    /// <inheritdoc/>
    public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds => this.guilds;

    /// <inheritdoc/>
    public override void Dispose() => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        if (this.CurrentUser is null)
        {
            this.CurrentUser = await this.ApiClient.GetCurrentUserAsync();
            UpdateUserCache(this.CurrentUser);
        }
    }
}
