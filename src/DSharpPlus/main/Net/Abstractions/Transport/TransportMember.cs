// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

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
