using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DSharpPlus.Commands.Trees;

[DebuggerDisplay("{ToString()}")]
public record Command
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required MethodInfo? Method { get; init; }
    public required Ulid Id { get; init; }
    public object? Target { get; init; }
    public Command? Parent { get; init; }
    public IReadOnlyList<Command> Subcommands { get; init; }
    public IReadOnlyList<CommandParameter> Parameters { get; init; }
    public required IReadOnlyList<Attribute> Attributes { get; init; }
    public IReadOnlyList<ulong> GuildIds { get; init; } = [];
    public string FullName => this.Parent is null ? this.Name : $"{this.Parent.FullName} {this.Name}";

    public Command(IEnumerable<CommandBuilder> subcommandBuilders, IEnumerable<CommandParameterBuilder> parameterBuilders)
    {
        this.Subcommands = subcommandBuilders.Select(x => x.Build(this)).ToArray();
        this.Parameters = parameterBuilders.Select(x => x.Build(this)).ToArray();
    }

    /// <summary>
    /// Traverses this command tree, returning this command and all subcommands recursively.
    /// </summary>
    /// <returns>A list of all commands in this tree.</returns>
    public IReadOnlyList<Command> Flatten()
    {
        List<Command> commands = [this];
        foreach (Command subcommand in this.Subcommands)
        {
            commands.AddRange(subcommand.Flatten());
        }

        return commands;
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append(this.FullName);
        if (this.Subcommands.Count == 0)
        {
            stringBuilder.Append('(');
            stringBuilder.AppendJoin(", ", this.Parameters.Select(x => $"{x.Type.Name} {x.Name}"));
            stringBuilder.Append(')');
        }

        return stringBuilder.ToString();
    }
}
