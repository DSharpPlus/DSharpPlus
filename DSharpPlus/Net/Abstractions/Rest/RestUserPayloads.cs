﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class RestUserDmCreatePayload
    {
        [JsonProperty("recipient_id")]
        public ulong Recipient { get; set; }
    }

    internal sealed class RestUserGroupDmCreatePayload
    {
        [JsonProperty("access_tokens")]
        public IEnumerable<string> AccessTokens { get; set; }

        [JsonProperty("nicks")]
        public IDictionary<ulong, string> Nicknames { get; set; }
    }

    internal sealed class RestUserUpdateCurrentPayload
    {
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
        public string AvatarBase64 { get; set; }

        [JsonIgnore]
        public bool AvatarSet { get; set; }

        public bool ShouldSerializeAvatarBase64()
            => this.AvatarSet;
    }

    internal sealed class RestUserGuild
    {
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public ulong Id { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string IconHash { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOwner { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Permissions { get; set; }
    }

    internal sealed class RestUserGuildListPayload
    {
        [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
        public int Limit { get; set; }

        [JsonProperty("before", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? Before { get; set; }

        [JsonProperty("after", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? After { get; set; }
    }
}
