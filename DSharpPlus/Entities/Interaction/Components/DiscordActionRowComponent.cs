using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a row of components. Action rows can have up to five components.
    /// </summary>
    public sealed class DiscordActionRowComponent : DiscordComponent
    {
        /// <summary>
        /// The components contained within the action row.
        /// </summary>
        public IReadOnlyCollection<DiscordComponent> Components
        {
            get => this._components ?? new List<DiscordComponent>();
            set => this._components = new List<DiscordComponent>(value);
        }

        [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
        private List<DiscordComponent> _components;

        public DiscordActionRowComponent(IEnumerable<DiscordComponent> components) : this()
        {
            this.Components = components.ToList().AsReadOnly();
        }
        internal DiscordActionRowComponent()
        {
            this.Type = ComponentType.ActionRow;
        } // For Json.NET
    }
}
