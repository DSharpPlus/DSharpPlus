using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a command parameter choice for a <see cref="DiscordApplicationCommandOption"/>.
    /// </summary>
    public sealed class DiscordApplicationCommandOptionChoice
    {
        /// <summary>
        /// Gets the name of this choice parameter. 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the value of this choice parameter. 
        /// </summary>
        [JsonProperty("value")]  //TODO: JsonConverter may be necessary for this depending on the string.
        public object Value { get; set; }
    }
}
