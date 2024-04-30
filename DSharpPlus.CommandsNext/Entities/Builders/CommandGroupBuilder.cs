namespace DSharpPlus.CommandsNext.Builders;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.CommandsNext.Entities;

/// <summary>
/// Represents an interface to build a command group.
/// </summary>
public sealed class CommandGroupBuilder : CommandBuilder
{
    /// <summary>
    /// Gets the list of child commands registered for this group.
    /// </summary>
    public IReadOnlyList<CommandBuilder> Children { get; }
    private List<CommandBuilder> _childrenList { get; }

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
        _childrenList = new List<CommandBuilder>();
        Children = new ReadOnlyCollection<CommandBuilder>(_childrenList);
    }

    /// <summary>
    /// Adds a command to the collection of child commands for this group.
    /// </summary>
    /// <param name="child">Command to add to the collection of child commands for this group.</param>
    /// <returns>This builder.</returns>
    public CommandGroupBuilder WithChild(CommandBuilder child)
    {
        _childrenList.Add(child);
        return this;
    }

    internal override Command Build(CommandGroup? parent)
    {
        CommandGroup cmd = new CommandGroup
        {
            Name = Name,
            Description = Description,
            Aliases = Aliases,
            ExecutionChecks = ExecutionChecks,
            IsHidden = IsHidden,
            Parent = parent,
            Overloads = new ReadOnlyCollection<CommandOverload>(Overloads.Select(xo => xo.Build()).ToList()),
            Module = Module,
            CustomAttributes = CustomAttributes,
            Category = Category
        };

        List<Command> cs = new List<Command>();
        foreach (CommandBuilder xc in Children)
        {
            cs.Add(xc.Build(cmd));
        }

        cmd.Children = new ReadOnlyCollection<Command>(cs);
        return cmd;
    }
}
