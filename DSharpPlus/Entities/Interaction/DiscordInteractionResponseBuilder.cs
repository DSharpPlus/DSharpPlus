using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.Entities;

/// <summary>
/// Constructs an interaction response.
/// </summary>
public sealed class DiscordInteractionResponseBuilder : BaseDiscordMessageBuilder<DiscordInteractionResponseBuilder>
{
    /// <summary>
    /// Whether this interaction response should be ephemeral.
    /// </summary>
    public bool IsEphemeral
    {
        get => (this.Flags & DiscordMessageFlags.Ephemeral) == DiscordMessageFlags.Ephemeral;
        set => _ = value ? this.Flags |= DiscordMessageFlags.Ephemeral : this.Flags &= ~DiscordMessageFlags.Ephemeral;
    }
    
    /// <summary>
    /// The choices to send on this interaction response. Mutually exclusive with content, embed, and components.
    /// </summary>
    public IReadOnlyList<DiscordAutoCompleteChoice> Choices => this.choices;
    private readonly List<DiscordAutoCompleteChoice> choices = [];

    /// <summary>
    /// Constructs a new empty interaction response builder.
    /// </summary>
    public DiscordInteractionResponseBuilder() { }

    /// <summary>
    /// Copies the common properties from the passed builder.
    /// </summary>
    /// <param name="builder">The builder to copy.</param>
    public DiscordInteractionResponseBuilder(IDiscordMessageBuilder builder) : base(builder) { }

    /// <summary>
    /// Constructs a new interaction response builder based on the passed builder.
    /// </summary>
    /// <param name="builder">The builder to copy.</param>
    public DiscordInteractionResponseBuilder(DiscordInteractionResponseBuilder builder) : base(builder)
    {
        this.IsEphemeral = builder.IsEphemeral;
        this.choices.AddRange(builder.choices);
    }

    /// <summary>
    /// Adds a single auto-complete choice to the builder.
    /// </summary>
    /// <param name="choice">The choice to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public DiscordInteractionResponseBuilder AddAutoCompleteChoice(DiscordAutoCompleteChoice choice)
    {
        if (this.choices.Count >= 25)
        {
            throw new ArgumentException("Maximum of 25 choices per response.");
        }

        this.choices.Add(choice);
        return this;
    }

    /// <summary>
    /// Adds auto-complete choices to the builder.
    /// </summary>
    /// <param name="choices">The choices to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public DiscordInteractionResponseBuilder AddAutoCompleteChoices(IEnumerable<DiscordAutoCompleteChoice> choices)
    {
        if (this.choices.Count >= 25 || this.choices.Count + choices.Count() > 25)
        {
            throw new ArgumentException("Maximum of 25 choices per response.");
        }

        this.choices.AddRange(choices);
        return this;
    }

    /// <summary>
    /// Adds auto-complete choices to the builder.
    /// </summary>
    /// <param name="choices">The choices to add.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public DiscordInteractionResponseBuilder AddAutoCompleteChoices(params DiscordAutoCompleteChoice[] choices)
        => AddAutoCompleteChoices((IEnumerable<DiscordAutoCompleteChoice>)choices);

    /// <summary>
    /// Sets the interaction response to be ephemeral.
    /// </summary>
    /// <param name="ephemeral">Ephemeral.</param>
    public DiscordInteractionResponseBuilder AsEphemeral(bool ephemeral = true)
    {
        this.IsEphemeral = ephemeral;
        return this;
    }

    /// <summary>
    /// Allows for clearing the Interaction Response Builder so that it can be used again to send a new response.
    /// </summary>
    public override void Clear()
    {
        this.IsEphemeral = false;
        this.choices.Clear();

        base.Clear();
    }
}
