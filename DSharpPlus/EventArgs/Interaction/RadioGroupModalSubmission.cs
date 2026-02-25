using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about a radio group submitted through a modal.
/// </summary>
public class RadioGroupModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.RadioGroup;

    /// <inheritdoc/>
    public string CustomId { get; }

    /// <summary>
    /// The developer-defined value of the option that was chosen from this group.
    /// </summary>
    public string Value { get; }

    internal RadioGroupModalSubmission(string customId, string value)
    {
        this.CustomId = customId;
        this.Value = value;
    }
}
