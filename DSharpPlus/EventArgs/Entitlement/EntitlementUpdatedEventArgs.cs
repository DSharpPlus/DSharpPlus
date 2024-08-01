using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for EntitlementUpdated event.
/// </summary>
public class EntitlementUpdatedEventArgs : AsyncEventArgs
{
    public DiscordEntitlement Entitlement { get; internal set; }
}
