using System;

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for EntitlementCreated event.
/// </summary>
public class EntitlementCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Entitlement which was created
    /// </summary>
    public DiscordEntitlement Entitlement { get; internal set; }

    /// <summary>
    /// The timestamp at which this event was invoked. Unset for gateway events.
    /// </summary>
    public DateTimeOffset? Timestamp { get; internal set; }
}
