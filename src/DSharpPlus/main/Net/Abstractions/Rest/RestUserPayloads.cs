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
        => AvatarSet;
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
