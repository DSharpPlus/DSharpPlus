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
using DSharpPlus.Net;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Gets the author of a discord embed.
/// </summary>
public sealed class DiscordEmbedAuthor
{
    /// <summary>
    /// Gets the name of the author.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    /// <summary>
    /// Gets the url of the author.
    /// </summary>
    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public Uri Url { get; set; }

    /// <summary>
    /// Gets the url of the author's icon.
    /// </summary>
    [JsonProperty("icon_url", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUri IconUrl { get; set; }

    /// <summary>
    /// Gets the proxied url of the author's icon.
    /// </summary>
    [JsonProperty("proxy_icon_url", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUri ProxyIconUrl { get; internal set; }

    internal DiscordEmbedAuthor() { }
}
