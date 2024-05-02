
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;
/// <summary>
/// Represents arguments for <see cref="DiscordClient.StageInstanceUpdated"/>.
/// </summary>
public class StageInstanceUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the stage instance before the update.
    /// </summary>
    public DiscordStageInstance StageInstanceBefore { get; internal set; }

    /// <summary>
    /// Gets the stage instance after the update.
    /// </summary>
    public DiscordStageInstance StageInstanceAfter { get; internal set; }

    /// <summary>
    /// Gets the guild the stage instance is in.
    /// </summary>
    public DiscordGuild Guild
        => StageInstanceAfter.Guild;

    /// <summary>
    /// Gets the channel the stage instance is in.
    /// </summary>
    public DiscordChannel Channel
        => StageInstanceAfter.Channel;
}
