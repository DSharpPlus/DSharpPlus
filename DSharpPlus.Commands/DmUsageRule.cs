using System;

namespace DSharpPlus.Commands;

/// <summary>
/// Specifies whether a command may be executed in DMs, and if so, where.
/// </summary>
[Flags]
public enum DmUsageRule
{
    /// <summary>
    /// Stops this command from being executed in DMs.
    /// </summary>
    DenyDms = 0,

    /// <summary>
    /// If this flag is set, this command is allowed to be executed in DMs with the bot.
    /// </summary>
    AllowBotDms = 1 << 0,

    /// <summary>
    /// If this flag is set, this command is allowed to be executed in DMs between two users. This is only applicable to user apps.
    /// </summary>
    AllowUserDms = 1 << 1
}
