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
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a field in a guild's membership screening form
/// </summary>
public class DiscordGuildMembershipScreeningField
{
    /// <summary>
    /// Gets the type of the field.
    /// </summary>
    [JsonProperty("field_type", NullValueHandling = NullValueHandling.Ignore)]
    public MembershipScreeningFieldType Type { get; internal set; }

    /// <summary>
    /// Gets the title of the field.
    /// </summary>
    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    public string Label { get; internal set; }

    /// <summary>
    /// Gets the list of rules
    /// </summary>
    [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string> Values { get; internal set; }

    /// <summary>
    /// Gets whether the user has to fill out this field
    /// </summary>
    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsRequired { get; internal set; }

    public DiscordGuildMembershipScreeningField(MembershipScreeningFieldType type, string label, IEnumerable<string> values, bool required = true)
    {
        this.Type = type;
        this.Label = label;
        this.Values = values.ToList();
        this.IsRequired = required;
    }

    internal DiscordGuildMembershipScreeningField() { }
}
