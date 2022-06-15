using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordApplicationInstallParameters
    {
        /// <summary>
        /// The <see href="https://discord.com/developers/docs/topics/oauth2#shared-resources-oauth2-scopes">scopes</see> to add the application to the server with.
        /// </summary>
        public IReadOnlyList<string> Scopes { get; init; } = Array.Empty<string>();

        /// <summary>
        /// The <see cref="DiscordPermissions"/> to request for the bot role.
        /// </summary>
        public DiscordPermissions Permissions { get; init; }
    }
}
