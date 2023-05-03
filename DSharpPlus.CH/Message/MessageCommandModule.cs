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

    protected Task PostAsync(IMessageCommandModuleResult result) => _handler.TurnResultIntoActionAsync(result);

    protected IMessageCommandModuleResult Reply(MessageCommandModuleResult result, bool mention = false)
    {
        result.Type = mention ? MessageCommandModuleResultType.Reply : MessageCommandModuleResultType.NoMentionReply;
        return result;
    }

    protected IMessageCommandModuleResult FollowUp(MessageCommandModuleResult result)
    {
        result.Type = MessageCommandModuleResultType.FollowUp;
        return result;
    }

    protected IMessageCommandModuleResult Edit(MessageCommandModuleResult result)
    {
        result.Type = MessageCommandModuleResultType.Edit;
        return result;
    }

    protected IMessageCommandModuleResult Empty() =>
        new MessageCommandModuleResult { Type = MessageCommandModuleResultType.Empty };

    protected IMessageCommandModuleResult Send(MessageCommandModuleResult result)
    {
        result.Type = MessageCommandModuleResultType.Send;
        return result;
    }

    protected async Task<EventArgs.MessageReactionAddEventArgs?> WaitForReactionAsync(TimeSpan delay,
        DiscordMessage? message = null)
    {
        CancellationTokenSource source = new();
        TaskCompletionSource<EventArgs.MessageReactionAddEventArgs> reaction = new();
        source.Token.Register(() => reaction.TrySetCanceled());

        MessageReactionHandler.AddTask(message is null ? NewestMessage!.Id : message.Id, reaction);

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
