namespace DSharpPlus.Commands.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public record Command
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public MethodInfo? Method { get; init; }
    public object? Target { get; init; }
    public Command? Parent { get; init; }
    public IReadOnlyList<Command> Subcommands { get; init; }
    public required IReadOnlyList<CommandParameter> Parameters { get; init; }
    public required IReadOnlyList<Attribute> Attributes { get; init; }
    public string FullName => this.Parent is null ? this.Name : $"{this.Parent.FullName} {this.Name}";

    public Command(IEnumerable<CommandBuilder> subCommandBuilders) => this.Subcommands = subCommandBuilders.Select(x => x.WithParent(this).Build()).ToArray();
}
