using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Message.Internals;

namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// The MessageModule. Abstract class used for building message commands.
/// </summary>
public abstract class MessageModule
{
    public static readonly IMessageResult EmptyResult = new MessageResult { Type = MessageResultType.Empty };
    
    internal MessageHandler _handler = null!; // Will be set by the factory.

    public DiscordMessage Message { get; internal set; } = null!;
    /// <summary>
    /// The latest message that have been received and processed by PostAsync.
    /// </summary>
    public DiscordMessage? NewestMessage { get; internal set; } = null;
    public DiscordClient Client { get; internal set; } = null!;

    /// <summary>
    /// Performs a action without needing to return.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    protected Task PostAsync(IMessageResult result) => _handler.TurnResultIntoActionAsync(result);

    /// <summary>
    /// Sets a result to reply on action.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="mention">If the result should mention the user.</param>
    /// <returns></returns>
    protected IMessageResult Reply(MessageResult result, bool mention = false)
    {
        result.Type = mention ? MessageResultType.NoMentionReply : MessageResultType.Reply;
        return result;
    }

    /// <summary>
    /// Does a follow up to property of <see cref="MessageModule.NewestMessage">NewestMessage</see>
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    protected IMessageResult FollowUp(MessageResult result)
    {
        result.Type = MessageResultType.FollowUp;
        return result;
    }

    /// <summary>
    /// Edits a message to set content provided by result.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    protected IMessageResult Edit(MessageResult result)
    {
        result.Type = MessageResultType.Edit;
        return result;
    }

    /// <summary>
    /// Creates a MessageResult with no particular task.
    /// </summary>
    /// <returns></returns>
    protected IMessageResult Empty() => EmptyResult;

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    protected IMessageResult Send(MessageResult result)
    {
        result.Type = MessageResultType.Send;
        return result;
    }

    /// <summary>
    /// Do not use this. I have been too lazy to remove it.
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="condition"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    [Obsolete("Use DSharpPlus.Interactivity")]
    protected async Task<EventArgs.MessageReactionAddEventArgs?> WaitForReactionAsync(TimeSpan delay,
        Func<EventArgs.MessageReactionAddEventArgs, bool>? condition = null,
        DiscordMessage? message = null)
    {
        CancellationTokenSource source = new();
        TaskCompletionSource<EventArgs.MessageReactionAddEventArgs> reaction = new();
        source.Token.Register(() => reaction.TrySetCanceled());

        MessageReactionHandler.AddTask(message is null ? NewestMessage!.Id : message.Id, (reaction, condition));

        source.CancelAfter(delay);
        try
        {
            return await reaction.Task;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }
}
