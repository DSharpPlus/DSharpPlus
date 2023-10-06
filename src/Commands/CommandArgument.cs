using System;
using System.Collections.Generic;

namespace DSharpPlus.CommandAll.Commands
{
    public record CommandArgument
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required Type Type { get; init; }
        public required IReadOnlyList<Attribute> Attributes { get; init; }
    }
}
