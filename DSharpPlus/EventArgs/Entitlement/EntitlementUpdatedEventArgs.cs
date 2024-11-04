using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for EntitlementUpdated event.
/// </summary>
public class EntitlementUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Entitlement which was updated
    /// </summary>
    public DiscordEntitlement Entitlement { get; internal set; }
}
