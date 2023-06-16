// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
    private List<CommandBuilder> ChildrenList { get; }

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
        this.ChildrenList = new List<CommandBuilder>();
        this.Children = new ReadOnlyCollection<CommandBuilder>(this.ChildrenList);
    }

    /// <summary>
    /// Adds a command to the collection of child commands for this group.
    /// </summary>
    /// <param name="child">Command to add to the collection of child commands for this group.</param>
    /// <returns>This builder.</returns>
    public CommandGroupBuilder WithChild(CommandBuilder child)
    {
        this.ChildrenList.Add(child);
        return this;
    }

    internal override Command Build(CommandGroup? parent)
    {
        CommandGroup? cmd = new()
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

        List<Command>? cs = new();
        foreach (CommandBuilder? xc in this.Children)
        {
            cs.Add(xc.Build(cmd));
        }

        cmd.Children = new ReadOnlyCollection<Command>(cs);
        return cmd;
    }
}
