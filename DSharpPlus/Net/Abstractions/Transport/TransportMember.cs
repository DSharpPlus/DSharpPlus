namespace DSharpPlus.Net.Abstractions;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

internal class TransportMember
{
    [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
    public string AvatarHash { get; internal set; }

    [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
    public TransportUser User { get; internal set; }

    [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
    public string Nickname { get; internal set; }

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    public List<ulong> Roles { get; internal set; }

    [JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Include)]
    public DateTimeOffset? CommunicationDisabledUntil { get; internal set; }

    [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime JoinedAt { get; internal set; }

    [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsDeafened { get; internal set; }

    [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsMuted { get; internal set; }

    [JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? PremiumSince { get; internal set; }

    [JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsPending { get; internal set; }
}
