using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a guild's welcome screen.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class WelcomeScreenEditModel
{
    /// <summary>
    /// Sets whether the welcome screen should be enabled.
    /// </summary>
    public Optional<bool> Enabled { get; set; }

    /// <summary>
    /// Sets the welcome channels.
    /// </summary>
    public Optional<List<DiscordGuildWelcomeScreenChannel>> WelcomeChannels { get; set; }

    /// <summary>
    /// Sets the serer description shown.
    /// </summary>
    public Optional<string> Description { get; set; }
}
