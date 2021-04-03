using Newtonsoft.Json;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Abstractions
{
    internal class RestApplicationCommandCreatePayload
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("options")]
        public IEnumerable<DiscordApplicationCommandOption> Options { get; set; }
    }

    internal class RestApplicationCommandEditPayload
    {
        [JsonProperty("name")]
        public Optional<string> Name { get; set; }

        [JsonProperty("description")]
        public Optional<string> Description { get; set; }

        [JsonProperty("options")]
        public Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> Options { get; set; }
    }

    internal class RestInteractionResponsePayload
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public InteractionResponseType Type { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInteractionApplicationCommandCallbackData Data { get; set; }
    }
}
