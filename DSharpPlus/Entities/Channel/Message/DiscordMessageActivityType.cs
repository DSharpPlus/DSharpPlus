namespace DSharpPlus.Entities;


/// <summary>
/// Indicates the type of MessageActivity for the Rich Presence.
/// </summary>
public enum DiscordMessageActivityType
{
    /// <summary>
    /// Invites the user to join.
    /// </summary>
    Join = 1,

    /// <summary>
    /// Invites the user to spectate.
    /// </summary>
    Spectate = 2,

    /// <summary>
    /// Invites the user to listen.
    /// </summary>
    Listen = 3,

    /// <summary>
    /// Allows the user to request to join.
    /// </summary>
    JoinRequest = 5
}
