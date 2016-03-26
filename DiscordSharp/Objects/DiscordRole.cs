using Newtonsoft.Json;

namespace DiscordSharp.Objects
{
    public class DiscordRole
    {
        [JsonProperty("color")]
        public DiscordSharp.Color Color { get; set; }

        /// <summary>
        /// Whether or not to display all members seperate of others.
        /// </summary>
        [JsonProperty("hoist")]
        public bool Hoist { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("permission")]
        public DiscordPermission Permissions { get; set; }

        [JsonProperty("managed")]
        public bool Managed { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

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
                ID = this.ID
            };
        }

        internal DiscordRole() { }
    }
}
