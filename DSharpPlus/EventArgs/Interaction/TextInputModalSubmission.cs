using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about the data submitted through a text input component in a modal.
/// </summary>
public sealed class TextInputModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.TextInput;

    /// <inheritdoc/>
    public string CustomId { get; internal set; }

    /// <inheritdoc/>
    public string Value { get; internal set; }

    internal TextInputModalSubmission(string customId, string value)
    {
        this.CustomId = customId;
        this.Value = value;
    }
}
