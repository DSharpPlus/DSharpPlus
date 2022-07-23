
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;

namespace ToffyDiscord.Commands;

public static class CommandHandleModule
{
    public static Task Handle(DiscordClient client, MessageCreateEventArgs e)
    {
        var nextCommand = client.GetCommandsNext();
        var msg = e.Message;

        var cmdStart = msg.GetStringPrefixLength("!");
        if (cmdStart == -1) return Task.CompletedTask;

        var prefix = msg.Content[..cmdStart];
        var cmdString = msg.Content[cmdStart..];

        var command = nextCommand.FindCommand(cmdString, out var args);
        if (command == null)
            return Task.CompletedTask;

        var ctx = nextCommand.CreateContext(msg, prefix, command, args);
        Task.Run(async () => await nextCommand.ExecuteCommandAsync(ctx));

        return Task.CompletedTask;
    }
}
