﻿using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class RestWebhookPayload
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
        public string AvatarBase64 { get; set; }

        [JsonProperty]
        public bool AvatarSet { get; set; }

        public bool ShouldSerializeAvatarBase64()
            => this.AvatarSet;
    }

    internal sealed class RestWebhookExecutePayload
    {
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
        public string AvatarUrl { get; set; }

        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; set; }

        [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordEmbed> Embeds { get; set; }

        [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMentions Mentions { get; set; }
    }
}
