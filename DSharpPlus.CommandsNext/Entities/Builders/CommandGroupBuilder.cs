using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext.Builders;

/// <summary>
/// Represents an interface to build a command group.
/// </summary>
public sealed class CommandGroupBuilder : CommandBuilder
{
    /// <summary>
    /// Gets the list of child commands registered for this group.
    /// </summary>
    public IReadOnlyList<CommandBuilder> Children { get; }
    private List<CommandBuilder> childrenList { get; }

    /// <summary>
    /// Creates a new module-less command group builder.
    /// </summary>
    public CommandGroupBuilder() : this(null) { }

    /// <summary>
    /// Creates a new command group builder.
    /// </summary>
    /// <param name="module">Module on which this group is to be defined.</param>
    public CommandGroupBuilder(ICommandModule? module) : base(module)
    {
        this.childrenList = [];
        this.Children = new ReadOnlyCollection<CommandBuilder>(this.childrenList);
    }

    /// <summary>
    /// Adds a command to the collection of child commands for this group.
    /// </summary>
    /// <param name="child">Command to add to the collection of child commands for this group.</param>
    /// <returns>This builder.</returns>
    public CommandGroupBuilder WithChild(CommandBuilder child)
    {
        this.childrenList.Add(child);
        return this;
    }

    internal override Command Build(CommandGroup? parent)
    {
        CommandGroup cmd = new()
        {
            Name = this.Name,
            Description = this.Description,
            Aliases = this.Aliases,
            ExecutionChecks = this.ExecutionChecks,
            IsHidden = this.IsHidden,
            Parent = parent,
            Overloads = new ReadOnlyCollection<CommandOverload>(this.Overloads.Select(xo => xo.Build()).ToList()),
            Module = this.Module,
            CustomAttributes = this.CustomAttributes,
            Category = this.Category
        };

        List<Command> cs = [];
        foreach (CommandBuilder xc in this.Children)
        {
            cs.Add(xc.Build(cmd));
        }

        cmd.Children = new ReadOnlyCollection<Command>(cs);
        return cmd;
    }
}
