using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for EntitlementDeleted event.
/// </summary>
public class EntitlementDeletedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Entitlement which was deleted
    /// </summary>
    public DiscordEntitlement Entitlement { get; internal set; }
}
