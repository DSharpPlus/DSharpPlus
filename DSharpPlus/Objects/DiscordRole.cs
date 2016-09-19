using Newtonsoft.Json;

namespace DSharpPlus.Objects
{
    public class DiscordRole
    {
        /// <summary>
        /// Role's color.
        /// </summary>
        [JsonProperty("color")]
        public DSharpPlus.Color Color { get; internal set; }

        /// <summary>
        /// Whether or not to display all members seperate of others.
        /// </summary>
        [JsonProperty("hoist")]
        public bool Hoist { get; internal set; }

        /// <summary>
        /// Role's name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Role's permissions.
        /// </summary>
        [JsonProperty("permission")]
        public DiscordPermission Permissions { get; internal set; }

        /// <summary>
        /// Wether this role is managed by an integration
        /// </summary>
        [JsonProperty("managed")]
        public bool Managed { get; internal set; }

        /// <summary>
        /// Role's position.
        /// </summary>
        [JsonProperty("position")]
        public int Position { get; internal set; }

        /// <summary>
        /// Role's ID.
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; internal set; }

        /// <summary>
        /// Wether this role can be mentioned.
        /// </summary>
        [JsonProperty("mentionable")]
        public bool CanBeMentioned { get; internal set; }
        /// <summary>
        /// Copies a role
        /// </summary>
        /// <returns>A new role with the same values as the one it copied</returns>
        public DiscordRole Copy()
        {
            return new DiscordRole
            {
                Color = this.Color,
                Hoist = this.Hoist,
                Name = this.Name,
                Permissions = this.Permissions,
                Managed = this.Managed,
                Position = this.Position,
                ID = this.ID,
                CanBeMentioned = this.CanBeMentioned
            };
        }

        internal DiscordRole() { }
    }
}
