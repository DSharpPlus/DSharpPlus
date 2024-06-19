using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for EntitlementUpdated event.
/// </summary>
public class EntitlementUpdatedEventArgs : DiscordEventArgs
{
    public DiscordEntitlement Entitlement { get; internal set; }
}
