﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal class TransportMember
    {
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public TransportUser User { get; internal set; }

        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname { get; internal set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<ulong> Roles { get; internal set; }

        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime JoinedAt { get; internal set; }

        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDeafened { get; internal set; }

        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsMuted { get; internal set; }

        [JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PremiumSince { get; internal set; }
    }
}
