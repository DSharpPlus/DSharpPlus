namespace DSharpPlus.Entities
{
    public sealed class DiscordMentionableSelectComponent : BaseDiscordSelectComponent
    {
        /// <summary>
        /// Enables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordMentionableSelectComponent Enable()
        {
            this.Disabled = false;
            return this;
        }
        /// <summary>
        /// Disables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordMentionableSelectComponent Disable()
        {
            this.Disabled = true;
            return this;
        }

        internal DiscordMentionableSelectComponent()
        {
            this.Type = ComponentType.MentionableSelect;
        }

        /// <summary>
        /// Creates a new mentionable select component.
        /// </summary>
        /// <param name="customId">The ID of this component.</param>
        /// <param name="placeholder">Placeholder text that's shown when no options are selected.</param>
        /// <param name="disabled">Whether this component is disabled.</param>
        /// <param name="minOptions">The minimum amount of options to be selected.</param>
        /// <param name="maxOptions">The maximum amount of options to be selected, up to 25.</param>
        public DiscordMentionableSelectComponent(string customId, string placeholder, bool disabled = false, int minOptions = 1, int maxOptions = 1) : this()
        {
            this.CustomId = customId;
            this.Placeholder = placeholder;
            this.Disabled = disabled;
            this.MinimumSelectedValues = minOptions;
            this.MaximumSelectedValues = maxOptions;
        }
    }
}
