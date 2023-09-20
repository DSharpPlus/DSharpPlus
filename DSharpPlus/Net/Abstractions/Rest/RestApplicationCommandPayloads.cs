using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal class RestApplicationCommandCreatePayload
    {
        [JsonProperty("type")]
        public ApplicationCommandType Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordApplicationCommandOption> Options { get; set; }

        [JsonProperty("default_permission", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DefaultPermission { get; set; }

        [JsonProperty("name_localizations")]
        public IReadOnlyDictionary<string, string> NameLocalizations { get; set; }

        [JsonProperty("description_localizations")]
        public IReadOnlyDictionary<string, string> DescriptionLocalizations{ get; set; }

        [JsonProperty("dm_permission", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AllowDMUsage { get; set; }

        [JsonProperty("default_member_permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions? DefaultMemberPermissions { get; set; }

        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NSFW { get; set; }
    }

    internal class RestApplicationCommandEditPayload
    {
        [JsonProperty("name")]
        public Optional<string> Name { get; set; }

        [JsonProperty("description")]
        public Optional<string> Description { get; set; }

        [JsonProperty("options")]
        public Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> Options { get; set; }

        [JsonProperty("default_permission", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool?> DefaultPermission { get; set; }

        [JsonProperty("name_localizations")]
        public IReadOnlyDictionary<string, string> NameLocalizations { get; set; }

        [JsonProperty("description_localizations")]
        public IReadOnlyDictionary<string, string> DescriptionLocalizations{ get; set; }

        [JsonProperty("dm_permission", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> AllowDMUsage { get; set; }

        [JsonProperty("default_member_permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<Permissions?> DefaultMemberPermissions { get; set; }

        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool?> NSFW { get; set; }
    }

    internal class RestInteractionResponsePayload
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public InteractionResponseType Type { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInteractionApplicationCommandCallbackData Data { get; set; }
    }

    internal class RestFollowupMessageCreatePayload
    {
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; set; }

        [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordEmbed> Embeds { get; set; }

        [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMentions Mentions { get; set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public int? Flags { get; set; }

        [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordActionRowComponent> Components { get; set; }
    }

    internal class RestEditApplicationCommandPermissionsPayload
    {
        [JsonProperty("permissions")]
        public IEnumerable<DiscordApplicationCommandPermission> Permissions { get; set; }
    }
}
