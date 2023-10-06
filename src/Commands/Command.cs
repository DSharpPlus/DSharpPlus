using System;
using System.Collections.Generic;

namespace DSharpPlus.CommandAll.Commands
{
    public record Command
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required Delegate Delegate { get; init; }
        public required Command Parent { get; init; }
        public required IReadOnlyList<Command> Subcommands { get; init; }
        public required IReadOnlyList<CommandArgument> Arguments { get; init; }
        public required IReadOnlyList<Attribute> Attributes { get; init; }
    }
}
