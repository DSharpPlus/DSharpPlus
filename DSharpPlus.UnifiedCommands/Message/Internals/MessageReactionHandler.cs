using System.Collections.Concurrent;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal static class MessageReactionHandler
{
    private static readonly ConcurrentDictionary<ulong, (TaskCompletionSource<MessageReactionAddEventArgs>,
            Func<MessageReactionAddEventArgs, bool>?)> _tasks = new();

    private static readonly Mutex _mutex = new();

    internal static void AddTask(ulong message,
        (TaskCompletionSource<MessageReactionAddEventArgs>, Func<MessageReactionAddEventArgs, bool>?) task)
        => _tasks.TryAdd(message, task);


    internal static Task MessageReactionEventAsync(DiscordClient client, MessageReactionAddEventArgs reactionEvent)
    {
        if (_tasks.TryGetValue(reactionEvent.Message.Id,
                out (TaskCompletionSource<MessageReactionAddEventArgs>,
                System.Func<MessageReactionAddEventArgs, bool>?) tuple))
        {
            if (tuple.Item2 is not null && tuple.Item2(reactionEvent))
            {
                tuple.Item1.TrySetResult(reactionEvent);
                _tasks.TryRemove(KeyValuePair.Create(reactionEvent.Message.Id, tuple));
            }
            else
            {
                tuple.Item1.TrySetResult(reactionEvent);
                _tasks.TryRemove(KeyValuePair.Create(reactionEvent.Message.Id, tuple));
            }
        }
        else
        {
            client.Logger.LogInformation("Didn't find anything");
        }

        return Task.CompletedTask;
    }
}
