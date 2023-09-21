using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
namespace DSharpPlus.Entities
{
    public sealed class DiscordChannelSelectComponent : BaseDiscordSelectComponent
    {
        [JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<ChannelType> ChannelTypes { get; internal set; }

        /// <summary>
        /// Enables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordChannelSelectComponent Enable()
        {
            this.Disabled = false;
            return this;
        }

        /// <summary>
        /// Disables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordChannelSelectComponent Disable()
        {
            this.Disabled = true;
            return this;
        }

        internal DiscordChannelSelectComponent()
        {
            this.Type = ComponentType.ChannelSelect;
        }

        /// <summary>
        /// Creates a new channel select component.
        /// </summary>
        /// <param name="customId">The ID of this component.</param>
        /// <param name="placeholder">Placeholder text that's shown when no options are selected.</param>
        /// <param name="channelTypes">Optional channel types to filter by.</param>
        /// <param name="disabled">Whether this component is disabled.</param>
        /// <param name="minOptions">The minimum amount of options to be selected.</param>
        /// <param name="maxOptions">The maximum amount of options to be selected, up to 25.</param>
        public DiscordChannelSelectComponent(string customId, string placeholder, IEnumerable<ChannelType>? channelTypes = null, bool disabled = false, int minOptions = 1, int maxOptions = 1) : this()
        {
            this.CustomId = customId;
            this.Placeholder = placeholder;
            this.ChannelTypes = channelTypes?.ToList();
            this.Disabled = disabled;
            this.MinimumSelectedValues = minOptions;
            this.MaximumSelectedValues = maxOptions;
        }
    }
}
