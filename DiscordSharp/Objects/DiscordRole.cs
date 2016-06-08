using Newtonsoft.Json;

namespace DiscordSharp.Objects
{
    public class DiscordRole
    {
        [JsonProperty("color")]
        public DiscordSharp.Color Color { get; internal set; }

        /// <summary>
        /// Whether or not to display all members seperate of others.
        /// </summary>
        [JsonProperty("hoist")]
        public bool Hoist { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("permission")]
        public DiscordPermission Permissions { get; internal set; }

        [JsonProperty("managed")]
        public bool Managed { get; internal set; }

        [JsonProperty("position")]
        public int Position { get; internal set; }

        [JsonProperty("id")]
        public string ID { get; internal set; }

        [JsonProperty("mentionable")]
        public bool CanBeMentioned { get; internal set; }

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
