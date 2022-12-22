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
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an emoji to add to a component.
/// </summary>
public sealed class DiscordComponentEmoji
{
    /// <summary>
    /// The Id of the emoji to use.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong Id { get; set; }

    /// <summary>
    /// The name of the emoji to use. Ignored if <see cref="Id"/> is set.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    /// <summary>
    /// Constructs a new component emoji to add to a <see cref="DiscordComponent"/>.
    /// </summary>
    public DiscordComponentEmoji() { }

    /// <summary>
    /// Constructs a new component emoji from an emoji Id.
    /// </summary>
    /// <param name="id">The Id of the emoji to use. Any valid emoji Id can be passed.</param>
    public DiscordComponentEmoji(ulong id) => Id = id;

    /// <summary>
    /// Constructs a new component emoji from unicode.
    /// </summary>
    /// <param name="name">The unicode emoji to set.</param>
    public DiscordComponentEmoji(string name)
    {
        if (!DiscordEmoji.IsValidUnicode(name))
        {
            throw new ArgumentException("Only unicode emojis can be passed.");
        }

        Name = name;
    }

    /// <summary>
    /// Constructs a new component emoji from an existing <see cref="DiscordEmoji"/>.
    /// </summary>
    /// <param name="emoji">The emoji to use.</param>
    public DiscordComponentEmoji(DiscordEmoji emoji)
    {
        Id = emoji.Id;
        Name = emoji.Name; // Name is ignored if the Id is present. //
    }
}
