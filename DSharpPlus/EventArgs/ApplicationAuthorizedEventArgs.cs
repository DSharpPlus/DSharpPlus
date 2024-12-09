using DSharpPlus.Entities;
using System;
using System.Collections.Generic;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Invoked when the current application is added to a server or user account. This is not available via the
/// standard gateway, and requires webhook events to be enabled.
/// </summary>
public sealed class ApplicationAuthorizedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The context this authorization occurred in.
    /// </summary>
    public DiscordApplicationIntegrationType IntegrationType { get; internal set; }

    /// <summary>
    /// The user who authorized the application. This may be a member object if the application was authorized
    /// into a guild.
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// The scopes the app was authorized for.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; internal set; }

    /// <summary>
    /// The guild the application was authorized for. Only applicable if <see cref="IntegrationType"/> is
    /// <see cref="DiscordApplicationIntegrationType.GuildInstall"/>.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }

    /// <summary>
    /// The timestamp at which this event was fired.
    /// </summary>
    public DateTimeOffset Timestamp { get; internal set; }
}
