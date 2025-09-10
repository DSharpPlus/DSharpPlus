using System;
using System.Collections.Generic;
using DSharpPlus.Entities.Interaction.Components;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a builder class to construct modals with.
/// </summary>
public class DiscordModalBuilder
{
    private List<DiscordComponent> _components = [];
    
    /// <summary>
    /// Gets the components to be displayed in this modal.
    /// </summary>
    /// <remarks>
    /// Generally, this will be either a <see cref="DiscordLabelComponent"/> or
    /// <see cref="DiscordTextDisplayComponent"/>, but <see cref="DiscordActionRowComponent"/> is also valid. This is subject to change.
    /// </remarks>
    public IReadOnlyList<DiscordComponent> Components => _components;
    
    /// <summary>
    /// Gets or sets the title of the modal.
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Gets the Custom ID of this modal.
    /// </summary>
    public string CustomId { get; set; }

    public DiscordModalBuilder WithTitle
    (
        string title
    )
    {
        Title = title;
        return this;
    }

    public DiscordModalBuilder WithCustomID
    (
        string customID
    )
    {
        this.CustomId = customID;
        return this;
    }

    public DiscordModalBuilder AddText(DiscordTextDisplayComponent text)
    {
        if (this.Components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }
        
        this._components.Add(text);
        return this;
    }

    public DiscordModalBuilder AddInput
    (
        DiscordLabelComponent component
    )
    {
        if (this._components.Count >= 5)
        {
            throw new InvalidOperationException("Modals can only have 5 components at this time.");
        }
        
        this._components.Add(component);
        
        return this;
    }

    public void Clear()
    {
        this._components.Clear();
        this.Title = string.Empty;
        this.CustomId = string.Empty;
    }

}
