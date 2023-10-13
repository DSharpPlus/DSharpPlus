using System;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Converters.Meta
{
    public record ConverterContext
    {
        public CommandAllExtension Extension { get; init; }
        public AsyncEventArgs EventArgs { get; init; }
        public Command Command { get; init; }
        public CommandArgument? Argument => (ArgumentIndex == -1 || ArgumentIndex >= Command.Arguments.Count) ? null : Command.Arguments[ArgumentIndex];
        public int ArgumentIndex { get; private set; } = -1;

        public DiscordClient Client => Extension.Client;

        public ConverterContext(CommandAllExtension extension, AsyncEventArgs eventArgs, Command command)
        {
            Extension = extension ?? throw new ArgumentNullException(nameof(extension));
            EventArgs = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public bool NextArgument()
        {
            if (ArgumentIndex + 1 >= Command.Arguments.Count)
            {
                return false;
            }

            ArgumentIndex++;
            return true;
        }
    }
}
