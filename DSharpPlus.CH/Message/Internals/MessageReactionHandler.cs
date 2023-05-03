using System.Runtime.InteropServices;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.CH.Message.Internals;

internal static class MessageReactionHandler
{
    private static Dictionary<ulong, TaskCompletionSource<MessageReactionAddEventArgs>> _tasks = new();
    private static Mutex _mutex = new();

    internal static void AddTask(ulong message, TaskCompletionSource<MessageReactionAddEventArgs> task)
    {
        lock (_mutex)
        {
            _tasks.Add(message, task);
        }
    }

    internal static Task MessageReactionEventAsync(DiscordClient client, MessageReactionAddEventArgs reactionEvent)
    {
        lock (_mutex)
        {
            if (_tasks.TryGetValue(reactionEvent.Message.Id, out TaskCompletionSource<MessageReactionAddEventArgs>? reaction))
            {
                reaction.TrySetResult(reactionEvent);
            }
            else
            {
                client.Logger.LogInformation("Didn't find anything");
            }
        }

        return Task.CompletedTask;
    }
}
