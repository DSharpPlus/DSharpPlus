using System.Collections.Generic;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalApplicationInstallParameters
{
    /// <summary>
    /// The <see href="https://discord.com/developers/docs/topics/oauth2#shared-resources-oauth2-scopes">scopes</see> to add the application to the server with.
    /// </summary>
    public required IReadOnlyList<string> Scopes { get; init; }

    /// <summary>
    /// The <see cref="DiscordPermissions"/> to request for the bot role.
    /// </summary>
    public DiscordPermissions Permissions { get; init; }
}
