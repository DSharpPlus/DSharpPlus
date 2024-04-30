namespace DSharpPlus.EventArgs;

using System;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.TypingStarted"/> event.
/// </summary>
public class TypingStartEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the channel in which the indicator was triggered.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the user that started typing.
    /// <para>This can be cast to a <see cref="DiscordMember"/> if the typing occurred in a guild.</para>
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the guild in which the indicator was triggered.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the date and time at which the user started typing.
    /// </summary>
    public DateTimeOffset StartedAt { get; internal set; }

    internal TypingStartEventArgs() : base() { }
}
