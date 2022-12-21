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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal sealed class TransportTeam
{
    [JsonProperty("id")]
    public ulong Id { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
    public string Name { get; set; }

    [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
    public string IconHash { get; set; }

    [JsonProperty("owner_user_id")]
    public ulong OwnerId { get; set; }

    [JsonProperty("members", NullValueHandling = NullValueHandling.Include)]
    public IEnumerable<TransportTeamMember> Members { get; set; }

    internal TransportTeam() { }
}

internal sealed class TransportTeamMember
{
    [JsonProperty("membership_state")]
    public int MembershipState { get; set; }

    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Include)]
    public IEnumerable<string> Permissions { get; set; }

    [JsonProperty("team_id")]
    public ulong TeamId { get; set; }

    [JsonProperty("user", NullValueHandling = NullValueHandling.Include)]
    public TransportUser User { get; set; }

    internal TransportTeamMember() { }
}
