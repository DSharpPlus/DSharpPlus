using System;
using System.Collections.Generic;
namespace DSharpPlus.Entities;

/// <summary>
/// Represents a builder class to construct modals with.
/// </summary>
public class DiscordModalBuilder
{
    private readonly List<DiscordComponent> components = [];
    
    /// <summary>
    /// Gets the components to be displayed in this modal.
    /// </summary>
    /// <remarks>
    /// Generally, this will be either a <see cref="DiscordLabelComponent"/> or
    /// <see cref="DiscordTextDisplayComponent"/>. This restriction is subject to change as Discord continues to release new Modal APIs.
    /// </remarks>
    public IReadOnlyList<DiscordComponent> Components => this.components;
    
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

        this.Title = title;
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
    /// <param name="input">The text input to add to this modal</param>
    /// <param name="label">A label text shown above the text input.</param>
    /// <param name="description">An optional description for the text input.</param>
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

        DiscordLabelComponent component = new(input, label, description);
        
        this.components.Add(component);
        
        return this;
    }

    /// <summary>
    /// Adds a select input to this modal.
    /// </summary>
    /// <param name="select">The select menu to add to this modal</param>
    /// <param name="label">A label text shown above the select menu.</param>
    /// <param name="description">An optional description for the menu.</param>
    /// <returns>The builder instance for chaining.</returns>
    public DiscordModalBuilder AddSelectMenu
    (
        BaseDiscordSelectComponent select,
        string label,
        string? description = null
    )
    {
        if (this.components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }

        DiscordLabelComponent component = new(select, label, description);

        this.components.Add(component);

        return this;
    }

    /// <summary>
    /// Adds a file upload field to this modal.
    /// </summary>
    /// <param name="upload">The upload field to add to this modal</param>
    /// <param name="label">A label text shown above the upload field.</param>
    /// <param name="description">An optional description for the upload field.</param>
    /// <returns>The builder instance for chaining.</returns>
    public DiscordModalBuilder AddFileUpload
    (
        DiscordFileUploadComponent upload,
        string label,
        string? description = null
    )
    {
        if (this.components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }

        DiscordLabelComponent component = new(upload, label, description);

        this.components.Add(component);

        return this;
    }

    /// <summary>
    /// Adds a single-binary-choice checkbox to this modal.
    /// </summary>
    /// <param name="checkbox">The checkbox to add to this modal.</param>
    /// <param name="label">The user-facing label for this checkbox.</param>
    /// <param name="description">An optional description for the checkbox.</param>
    /// <returns>The builder instance for chaining.</returns>
    public DiscordModalBuilder AddCheckbox
    (
        DiscordCheckboxComponent checkbox,
        string label,
        string? description = null
    )
    {
        if (this.components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }

        DiscordLabelComponent component = new(checkbox, label, description);

        this.components.Add(component);

        return this;
    }

    /// <summary>
    /// Adds a group of checkboxes to this modal.
    /// </summary>
    /// <param name="checkboxGroup">The group of checkboxes to add to this modal.</param>
    /// <param name="label">A label text shown above the checkbox group. Each individual checkbox has its own label.</param>
    /// <param name="description">An optional description for the checkbox group. Each individual checkbox may have its own description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public DiscordModalBuilder AddCheckboxGroup
    (
        DiscordCheckboxGroupComponent checkboxGroup,
        string label,
        string? description = null
    )
    {
        if (this.components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }

        DiscordLabelComponent component = new(checkboxGroup, label, description);

        this.components.Add(component);

        return this;
    }

    /// <summary>
    /// Adds a radio group to this modal.
    /// </summary>
    /// <param name="radioGroup">The radio group to add to this modal.</param>
    /// <param name="label">A label text shown above the radio group. Each individual option has its own label.</param>
    /// <param name="description">An optional description for the radio group. Each individual option may have its own description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public DiscordModalBuilder AddRadioGroup
    (
        DiscordRadioGroupComponent radioGroup,
        string label,
        string? description = null
    )
    {
        if (this.components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }

        DiscordLabelComponent component = new(radioGroup, label, description);

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
