using System.Collections.Generic;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.EventArgs
{
    public sealed class ConfigureCommandsEventArgs : AsyncEventArgs
    {
        public required CommandAllExtension Extension { get; init; }
        public required List<CommandBuilder> CommandBuilders { get; init; }
    }
}
