using System;
using System.Collections.Generic;
using DSharpPlus.Entities.Interaction.Components;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a builder class to construct modals with.
/// </summary>
public class DiscordModalBuilder
{
    private List<DiscordComponent> components = [];
    
    /// <summary>
    /// Gets the components to be displayed in this modal.
    /// </summary>
    /// <remarks>
    /// Generally, this will be either a <see cref="DiscordLabelComponent"/> or
    /// <see cref="DiscordTextDisplayComponent"/>. This restriction is subject to change as Discord continues to release new Modal APIs.
    /// </remarks>
    public IReadOnlyList<DiscordComponent> Components => components;
    
    /// <summary>
    /// Gets or sets the title of the modal.
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Gets the Custom ID of this modal.
    /// </summary>
    public string CustomId { get; set; }

    /// <summary>
    /// Sets the title for the modal.
    /// </summary>
    /// <param name="title">The title of the modal. (Maximum of 256 characters.)</param>
    /// <returns>The updated builder to chain calls with.</returns>
    public DiscordModalBuilder WithTitle
    (
        string title
    )
    {
        if (string.IsNullOrEmpty(title) || title.Length > 256)
        {
            throw new ArgumentException("Title must be between 1 and 256 characters.");
        }

        Title = title;
        return this;
    }

    /// <summary>
    /// Sets the custom ID of the modal.
    /// </summary>
    /// <param name="customId">The custom ID.</param>
    /// <returns>The updated builder to chain calls with.</returns>
    public DiscordModalBuilder WithCustomId
    (
        string customId
    )
    {
        if (string.IsNullOrEmpty(customId) || customId.Length > 100)
        {
            throw new ArgumentException("Custom ID must be between 1 and 100 characters.");
        }
        
        this.CustomId = customId;
        return this;
    }

    /// <summary>
    /// Adds a block of text to the modal. All markdown supported.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <returns>The updated builder to chain calls with.</returns>
    public DiscordModalBuilder AddTextDisplay(string text)
    {
        if (this.Components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }
        
        this.components.Add(new DiscordTextDisplayComponent(text));
        return this;
    }

    /// <summary>
    /// Adds a new text input to the modal.
    /// </summary>
    /// <param name="input">The text input.</param>
    /// <param name="label">The label.</param>
    /// <param name="description">The optional description of the label.</param>
    /// <returns>The updated builder to chain calls with.</returns>
    public DiscordModalBuilder AddTextInput
    (
        DiscordTextInputComponent input,
        string label,
        string? description = null
    )
    {
        if (this.components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }
        
        var component = new DiscordLabelComponent(input, label, description);
        
        this.components.Add(component);
        
        return this;
    }

    public void Clear()
    {
        this.components.Clear();
        this.Title = string.Empty;
        this.CustomId = string.Empty;
    }

}
