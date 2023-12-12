namespace DSharpPlus.Entities;

/// <summary>
/// Indicates in what event context a rule should be checked.
/// </summary>
public enum DiscordRuleEventType
{
    /// <summary>
    /// The rule will trigger when a member send or modify a message.
    /// </summary>
    MessageSend = 1
}
