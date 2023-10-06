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
        public CommandArgument? Argument { get; private set; }

        public DiscordClient Client => Extension.Client;

        public ConverterContext(CommandAllExtension extension, AsyncEventArgs eventArgs, Command command)
        {
            Extension = extension ?? throw new ArgumentNullException(nameof(extension));
            EventArgs = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Argument = null; // Start it out null until NextArgument is called. This'll make it easier for while(context.NextArgument()) loops
        }

        public bool NextArgument()
        {
            int nextArgumentIndex = Command.Arguments.IndexOf(Argument) + 1;
            if (nextArgumentIndex >= Command.Arguments.Count)
            {
                return false;
            }

            Argument = Command.Arguments[nextArgumentIndex];
            return true;
        }
    }
}
