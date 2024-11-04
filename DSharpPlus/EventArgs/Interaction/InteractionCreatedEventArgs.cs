using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for InteractionCreated
/// </summary>
public class InteractionCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the interaction data that was invoked.
    /// </summary>
    public DiscordInteraction Interaction { get; internal set; }
}
