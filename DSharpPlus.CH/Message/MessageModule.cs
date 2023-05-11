using System.Runtime.InteropServices.JavaScript;
using DSharpPlus.CH.Message.Internals;
using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message;

public abstract class MessageModule
{
    internal MessageHandler _handler = null!; // Will be set by the factory.

    public DiscordMessage Message { get; internal set; } = null!;
    public DiscordMessage? NewestMessage { get; internal set; } = null;
    public DiscordClient Client { get; internal set; } = null!;

    protected Task PostAsync(IMessageResult result) => _handler.TurnResultIntoActionAsync(result);

    protected IMessageResult Reply(MessageResult result, bool mention = false)
    {
        result.Type = mention ? MessageResultType.NoMentionReply : MessageResultType.Reply;
        return result;
    }

    protected IMessageResult FollowUp(MessageResult result)
    {
        result.Type = MessageResultType.FollowUp;
        return result;
    }

    protected IMessageResult Edit(MessageResult result)
    {
        result.Type = MessageResultType.Edit;
        return result;
    }

    protected IMessageResult Empty() =>
        new MessageResult { Type = MessageResultType.Empty };

    protected IMessageResult Send(MessageResult result)
    {
        result.Type = MessageResultType.Send;
        return result;
    }

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
