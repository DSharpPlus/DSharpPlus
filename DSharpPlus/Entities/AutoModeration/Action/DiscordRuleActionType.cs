namespace DSharpPlus.Entities;


/// <summary>
/// Contains all actions that can be taken after a rule activation.
/// </summary>
public enum DiscordRuleActionType
{
    /// <summary>
    /// Blocks a member's message and prevents it from being posted. 
    /// A custom message can be specified and shown to members whenever their message is blocked.
    /// </summary>
    BlockMessage = 1,

    /// <summary>
    /// Logs the user content to a specified channel.
    /// </summary>
    SendAlertMessage = 2,

    /// <summary>
    /// Timeout user for a specified duration.
    /// </summary>
    Timeout = 3
}
