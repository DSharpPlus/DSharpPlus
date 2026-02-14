using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about a checkbox submitted through a modal.
/// </summary>
public class CheckboxModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.Checkbox;

    /// <inheritdoc/>
    public string CustomId { get; }

    /// <summary>
    /// Indicates whether the checkbox was checked by the user.
    /// </summary>
    public bool Value { get; }

    internal CheckboxModalSubmission(string customId, bool value)
    {
        this.CustomId = customId;
        this.Value = value;
    }
}
