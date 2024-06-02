using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.StageInstanceCreated"/>.
/// </summary>
public class StageInstanceCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the stage instance that was created.
    /// </summary>
    public DiscordStageInstance StageInstance { get; internal set; }

    /// <summary>
    /// Gets the guild the stage instance was created in.
    /// </summary>
    public DiscordGuild Guild
        => this.StageInstance.Guild;

    /// <summary>
    /// Gets the channel the stage instance was created in.
    /// </summary>
    public DiscordChannel Channel
        => this.StageInstance.Channel;
}
