using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about a checkbox group submitted through a modal.
/// </summary>
public class CheckboxGroupModalSubmission : IModalSubmission
{
    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.CheckboxGroup;

    /// <inheritdoc/>
    public string CustomId { get; }

    /// <summary>
    /// The developer-defined values of the checkboxes that were checked within this group.
    /// </summary>
    public IReadOnlyList<string> Values { get; }

    internal CheckboxGroupModalSubmission(string customId, IReadOnlyList<string> values)
    {
        this.CustomId = customId;
        this.Values = values;
    }
}
