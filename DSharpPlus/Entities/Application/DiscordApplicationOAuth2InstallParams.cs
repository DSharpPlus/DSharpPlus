using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the default installation configuration for an integration.
/// </summary>
/// <remarks>
/// <see cref="Permissions"/> is ignored in the case of <see cref="DiscordApplicationIntegrationType.UserInstall"/>.
/// </remarks>
public sealed class DiscordApplicationOAuth2InstallParams
{
    /// <summary>
    /// Represents permissions that the integration requires.
    /// </summary>
    [JsonProperty("permissions")]
    public DiscordPermissions Permissions { get; internal set; }

    /// <summary>
    /// Represents scopes granted to the integration.
    /// </summary>
    [JsonProperty("scopes")]
    public IReadOnlyList<string> Scopes { get; internal set; }

    public DiscordApplicationOAuth2InstallParams() { }
}
