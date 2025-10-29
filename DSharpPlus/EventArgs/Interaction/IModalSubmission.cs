using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents the values submitted to a component from a modal.
/// </summary>
public interface IModalSubmission
{
    /// <summary>
    /// The type of component this submission represents.
    /// </summary>
    public DiscordComponentType ComponentType { get; }

    /// <summary>
    /// The custom ID of this component.
    /// </summary>
    public string CustomId { get; }
}
