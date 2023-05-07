using System.Runtime.InteropServices.JavaScript;
using DSharpPlus.CH.Message.Internals;
using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message;

public abstract class MessageCommandModule
{
    internal MessageCommandHandler _handler = null!; // Will be set by the factory.

    public DiscordMessage Message { get; internal set; } = null!;
    public DiscordMessage? NewestMessage { get; internal set; } = null;
    public DiscordClient Client { get; internal set; } = null!;

    protected Task PostAsync(IMessageCommandResult result) => _handler.TurnResultIntoActionAsync(result);

    protected IMessageCommandResult Reply(MessageCommandResult result, bool mention = false)
    {
        result.Type = mention ? MessageCommandResultType.NoMentionReply : MessageCommandResultType.Reply;
        return result;
    }

    protected IMessageCommandResult FollowUp(MessageCommandResult result)
    {
        result.Type = MessageCommandResultType.FollowUp;
        return result;
    }

    protected IMessageCommandResult Edit(MessageCommandResult result)
    {
        result.Type = MessageCommandResultType.Edit;
        return result;
    }

    protected IMessageCommandResult Empty() =>
        new MessageCommandResult { Type = MessageCommandResultType.Empty };

    protected IMessageCommandResult Send(MessageCommandResult result)
    {
        result.Type = MessageCommandResultType.Send;
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
