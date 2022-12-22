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

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents information about a Discord voice server region.
/// </summary>
public class DiscordVoiceRegion
{
    /// <summary>
    /// Gets the unique ID for the region.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; internal set; }

    /// <summary>
    /// Gets the name of the region.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets an example server hostname for this region.
    /// </summary>
    [JsonProperty("sample_hostname", NullValueHandling = NullValueHandling.Ignore)]
    public string SampleHostname { get; internal set; }

    /// <summary>
    /// Gets an example server port for this region.
    /// </summary>
    [JsonProperty("sample_port", NullValueHandling = NullValueHandling.Ignore)]
    public int SamplePort { get; internal set; }

    /// <summary>
    /// Gets whether this is a VIP-only region.
    /// </summary>
    [JsonProperty("vip", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsVIP { get; internal set; }

    /// <summary>
    /// Gets whether this region is the most optimal for the current user.
    /// </summary>
    [JsonProperty("optimal", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsOptimal { get; internal set; }

    /// <summary>
    /// Gets whether this voice region is deprecated.
    /// </summary>
    [JsonProperty("deprecated", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsDeprecated { get; internal set; }

    /// <summary>
    /// Gets whether this is a custom voice region.
    /// </summary>
    [JsonProperty("custom", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsCustom { get; internal set; }

    /// <summary>
    /// Gets whether two <see cref="DiscordVoiceRegion"/>s are equal.
    /// </summary>
    /// <param name="region">The region to compare with.</param>
    /// <returns></returns>
    public bool Equals(DiscordVoiceRegion region)
        => this == region;

    public override bool Equals(object obj) => Equals(obj as DiscordVoiceRegion);

    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordVoiceRegion"/> objects are equal.
    /// </summary>
    /// <param name="left">First voice region to compare.</param>
    /// <param name="right">Second voice region to compare.</param>
    /// <returns>Whether the two voice regions are equal.</returns>
    public static bool operator ==(DiscordVoiceRegion left, DiscordVoiceRegion right)
    {
        object? o1 = left as object;
        object? o2 = right as object;

        return (o1 == null && o2 != null) || (o1 != null && o2 == null) ? false : o1 == null && o2 == null ? true : left.Id == right.Id;
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordVoiceRegion"/> objects are not equal.
    /// </summary>
    /// <param name="left">First voice region to compare.</param>
    /// <param name="right">Second voice region to compare.</param>
    /// <returns>Whether the two voice regions are not equal.</returns>
    public static bool operator !=(DiscordVoiceRegion left, DiscordVoiceRegion right)
        => !(left == right);

    internal DiscordVoiceRegion() { }
}
