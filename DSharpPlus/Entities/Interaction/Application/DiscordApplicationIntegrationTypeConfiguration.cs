namespace DSharpPlus.Entities;
using Newtonsoft.Json;

/// <summary>
/// Represents the configuration for an integration type.
/// </summary>
public sealed class DiscordApplicationIntegrationTypeConfiguration
{
    /// <summary>
    /// The install parameters for the integration.
    /// </summary>
    [JsonProperty("oauth2_install_params")]
    public DiscordApplicationOAuth2InstallParams OAuth2InstallParams { get; internal set; } = default!;

    public DiscordApplicationIntegrationTypeConfiguration() { }
}
