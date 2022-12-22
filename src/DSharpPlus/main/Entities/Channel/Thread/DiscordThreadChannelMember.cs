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

public class DiscordThreadChannelMember
{
    /// <summary>
    /// Gets ID of the thread.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ThreadId { get; set; }

    /// <summary>
    /// Gets ID of the user.
    /// </summary>
    [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong Id { get; set; }

    /// <summary>
    /// Gets timestamp when the user joined the thread.
    /// </summary>
    [JsonProperty("join_timestamp", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? JoinTimeStamp { get; internal set; }

    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    internal int UserFlags { get; set; }

    /// <summary>
    /// Gets the DiscordMember that represents this ThreadMember. Can be a skeleton object.
    /// </summary>
    [JsonIgnore]
    public DiscordMember Member
        => Guild != null ? (Guild._members.TryGetValue(Id, out DiscordMember? member) ? member : new DiscordMember { Id = Id, _guild_id = _guild_id, Discord = Discord }) : null;

    /// <summary>
    /// Gets the category that contains this channel. For threads, gets the channel this thread was created in.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel Thread
        => Guild != null ? (Guild._threads.TryGetValue(ThreadId, out DiscordThreadChannel? thread) ? thread : null) : null;

    /// <summary>
    /// Gets the guild to which this channel belongs.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild
        => Discord.Guilds.TryGetValue(_guild_id, out DiscordGuild? guild) ? guild : null;

    [JsonIgnore]
    internal ulong _guild_id;

    /// <summary>
    /// Gets the client instance this object is tied to.
    /// </summary>
    [JsonIgnore]
    internal BaseDiscordClient Discord { get; set; }

    internal DiscordThreadChannelMember() { }

    /// <summary>
    /// Checks whether this <see cref="DiscordThreadChannelMember"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordThreadChannelMember"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordThreadChannelMember);

    /// <summary>
    /// Checks whether this <see cref="DiscordThreadChannelMember"/> is equal to another <see cref="DiscordThreadChannelMember"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordThreadChannelMember"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordThreadChannelMember"/> is equal to this <see cref="DiscordThreadChannelMember"/>.</returns>
    public bool Equals(DiscordThreadChannelMember e)
    {
        return e is null ? false : ReferenceEquals(this, e) ? true : Id == e.Id && ThreadId == e.ThreadId;
    }

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordThreadChannelMember"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordThreadChannelMember"/>.</returns>
    public override int GetHashCode()
    {
        int hash = 13;

        hash = (hash * 7) + Id.GetHashCode();
        hash = (hash * 7) + ThreadId.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordThreadChannelMember"/> objects are equal.
    /// </summary>
    /// <param name="e1">First message to compare.</param>
    /// <param name="e2">Second message to compare.</param>
    /// <returns>Whether the two messages are equal.</returns>
    public static bool operator ==(DiscordThreadChannelMember e1, DiscordThreadChannelMember e2)
    {
        object? o1 = e1 as object;
        object? o2 = e2 as object;

        return (o1 == null && o2 != null) || (o1 != null && o2 == null)
            ? false
            : o1 == null && o2 == null ? true : e1.Id == e2.Id && e1.ThreadId == e2.ThreadId;
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordThreadChannelMember"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First message to compare.</param>
    /// <param name="e2">Second message to compare.</param>
    /// <returns>Whether the two messages are not equal.</returns>
    public static bool operator !=(DiscordThreadChannelMember e1, DiscordThreadChannelMember e2)
        => !(e1 == e2);
}
