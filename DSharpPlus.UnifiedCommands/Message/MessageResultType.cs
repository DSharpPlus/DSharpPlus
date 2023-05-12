namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// All type of actions that can be executed by the handler.
/// </summary>
public enum MessageResultType
{
    Empty,
    Reply,
    NoMentionReply,
    Send,
    FollowUp,
    Edit
}
