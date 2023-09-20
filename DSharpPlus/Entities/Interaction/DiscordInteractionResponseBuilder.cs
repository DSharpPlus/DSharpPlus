using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.Entities
{
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
            get => (this.Flags & MessageFlags.Ephemeral) == MessageFlags.Ephemeral;
            set => _ = value ? this.Flags |= MessageFlags.Ephemeral : this.Flags &= ~MessageFlags.Ephemeral;
        }

        /// <summary>
        /// The custom id to send with this interaction response. Only applicable when creating a modal.
        /// </summary>
        public string CustomId { get; set; }

        /// <summary>
        /// The title to send with this interaction response. Only applicable when creating a modal.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The choices to send on this interaction response. Mutually exclusive with content, embed, and components.
        /// </summary>
        public IReadOnlyList<DiscordAutoCompleteChoice> Choices => this._choices;
        private readonly List<DiscordAutoCompleteChoice> _choices = new();

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
            this._choices.AddRange(builder._choices);
        }

        /// <summary>
        /// If responding with a modal, sets the title of the modal.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public DiscordInteractionResponseBuilder WithTitle(string title)
        {
            if (string.IsNullOrEmpty(title) || title.Length > 256)
                throw new ArgumentException("Title must be between 1 and 256 characters.");

            this.Title = title;
            return this;
        }

        /// <summary>
        /// If responding with a modal, sets the custom id for the modal.
        /// </summary>
        /// <param name="id">The custom id of the modal.</param>
        /// <returns></returns>
        public DiscordInteractionResponseBuilder WithCustomId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 100)
                throw new ArgumentException("Custom ID must be between 1 and 100 characters.");

            this.CustomId = id;
            return this;
        }

        /// <summary>
        /// Adds a single auto-complete choice to the builder.
        /// </summary>
        /// <param name="choice">The choice to add.</param>
        /// <returns>The current builder to chain calls with.</returns>
        public DiscordInteractionResponseBuilder AddAutoCompleteChoice(DiscordAutoCompleteChoice choice)
        {
            if (this._choices.Count >= 25)
                throw new ArgumentException("Maximum of 25 choices per response.");

            this._choices.Add(choice);
            return this;
        }

        /// <summary>
        /// Adds auto-complete choices to the builder.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current builder to chain calls with.</returns>
        public DiscordInteractionResponseBuilder AddAutoCompleteChoices(IEnumerable<DiscordAutoCompleteChoice> choices)
        {
            if (this._choices.Count >= 25 || this._choices.Count + choices.Count() > 25)
                throw new ArgumentException("Maximum of 25 choices per response.");

            this._choices.AddRange(choices);
            return this;
        }

        /// <summary>
        /// Adds auto-complete choices to the builder.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current builder to chain calls with.</returns>
        public DiscordInteractionResponseBuilder AddAutoCompleteChoices(params DiscordAutoCompleteChoice[] choices)
            => this.AddAutoCompleteChoices((IEnumerable<DiscordAutoCompleteChoice>)choices);

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
            this._choices.Clear();

            base.Clear();
        }
    }
}
