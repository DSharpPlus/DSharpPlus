using DSharpPlus.Entities;
namespace DSharpPlus.EventArgs;

public sealed class ContextMenuInteractionCreateEventArgs : InteractionCreateEventArgs
{
    /// <summary>
    /// The type of context menu that was used. This is never <see cref="DiscordApplicationCommandType.SlashCommand"/>.
    /// </summary>
    public DiscordApplicationCommandType Type { get; internal set; } //TODO: Set this

    /// <summary>
    /// The user that invoked this interaction. Can be casted to a member if this was on a guild.
    /// </summary>
    public DiscordUser User => this.Interaction.User;

    /// <summary>
    /// The user this interaction targets, if applicable.
    /// </summary>
    public DiscordUser TargetUser { get; internal set; }

    /// <summary>
    /// The message this interaction targets, if applicable.
    /// </summary>
    public DiscordMessage TargetMessage { get; internal set; }
}
