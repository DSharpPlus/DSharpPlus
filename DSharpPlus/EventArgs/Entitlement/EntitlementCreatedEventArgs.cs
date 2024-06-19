using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for EntitlementCreated event.
/// </summary>
public class EntitlementCreatedEventArgs : DiscordEventArgs
{
    public DiscordEntitlement Entitlement { get; internal set; }
}
