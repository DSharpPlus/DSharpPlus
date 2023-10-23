using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.StageInstanceDeleted"/>.
/// </summary>
public class StageInstanceDeleteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the stage instance that was deleted.
    /// </summary>
    public DiscordStageInstance StageInstance { get; internal set; }

    /// <summary>
    /// Gets the guild the stage instance was in.
    /// </summary>
    public DiscordGuild Guild
        => this.StageInstance.Guild;

    /// <summary>
    /// Gets the channel the stage instance was in.
    /// </summary>
    public DiscordChannel Channel
        => this.StageInstance.Channel;
}
