namespace DSharpPlus.Entities;


/// <summary>
/// Represents the type of context in which an interaction was created.
/// </summary>
public enum DiscordInteractionContextType
{
    /// <summary>
    /// The interaction is in a guild.
    /// </summary>
    Guild,

    /// <summary>
    /// The interaction is in a DM with the bot associated with the application. (This is to say, your bot.)
    /// </summary>
    BotDM,

    /// <summary>
    /// The interaction is in a [group] DM.
    /// </summary>
    PrivateChannel,
}
