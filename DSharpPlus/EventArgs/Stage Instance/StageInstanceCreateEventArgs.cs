namespace DSharpPlus.EventArgs;

using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.StageInstanceCreated"/>.
/// </summary>
public class StageInstanceCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the stage instance that was created.
    /// </summary>
    public DiscordStageInstance StageInstance { get; internal set; }

    /// <summary>
    /// Gets the guild the stage instance was created in.
    /// </summary>
    public DiscordGuild Guild
        => StageInstance.Guild;

    /// <summary>
    /// Gets the channel the stage instance was created in.
    /// </summary>
    public DiscordChannel Channel
        => StageInstance.Channel;
}
