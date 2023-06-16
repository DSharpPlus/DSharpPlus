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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;

namespace DSharpPlus.CommandsNext.Builders;

/// <summary>
/// Represents an interface to build a command.
/// </summary>
public class CommandBuilder
{
    /// <summary>
    /// Gets the name set for this command.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the category set for this command.
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Gets the aliases set for this command.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; }
    private List<string> _aliasList { get; }

    /// <summary>
    /// Gets the description set for this command.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets whether this command will be hidden or not.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Gets the execution checks defined for this command.
    /// </summary>
    public IReadOnlyList<CheckBaseAttribute> ExecutionChecks { get; }
    private List<CheckBaseAttribute> _executionCheckList { get; }

    /// <summary>
    /// Gets the collection of this command's overloads.
    /// </summary>
    public IReadOnlyList<CommandOverloadBuilder> Overloads { get; }
    private List<CommandOverloadBuilder> _overloadList { get; }
    private HashSet<string> _overloadArgumentSets { get; }

    /// <summary>
    /// Gets the module on which this command is to be defined.
    /// </summary>
    public ICommandModule? Module { get; }

    /// <summary>
    /// Gets custom attributes defined on this command.
    /// </summary>
    public IReadOnlyList<Attribute> CustomAttributes { get; }
    private List<Attribute> _customAttributeList { get; }

    /// <summary>
    /// Creates a new module-less command builder.
    /// </summary>
    public CommandBuilder() : this(null) { }

    /// <summary>
    /// Creates a new command builder.
    /// </summary>
    /// <param name="module">Module on which this command is to be defined.</param>
    public CommandBuilder(ICommandModule? module)
    {
        this._aliasList = new List<string>();
        this.Aliases = new ReadOnlyCollection<string>(this._aliasList);

        this._executionCheckList = new List<CheckBaseAttribute>();
        this.ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(this._executionCheckList);

        this._overloadArgumentSets = new HashSet<string>();
        this._overloadList = new List<CommandOverloadBuilder>();
        this.Overloads = new ReadOnlyCollection<CommandOverloadBuilder>(this._overloadList);

        this.Module = module;

        this._customAttributeList = new List<Attribute>();
        this.CustomAttributes = new ReadOnlyCollection<Attribute>(this._customAttributeList);
    }

    /// <summary>
    /// Sets the name for this command.
    /// </summary>
    /// <param name="name">Name for this command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithName(string name)
    {
        if (name == null || name.ToCharArray().Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("Command name cannot be null or contain any whitespace characters.", nameof(name));
        }
        else if (this.Name != null)
        {
            throw new InvalidOperationException("This command already has a name.");
        }
        else if (this._aliasList.Contains(name))
        {
            throw new ArgumentException("Command name cannot be one of its aliases.", nameof(name));
        }

        this.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the category for this command.
    /// </summary>
    /// <param name="category">Category for this command. May be <see langword="null"/>.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithCategory(string? category)
    {
        this.Category = category;
        return this;
    }

    /// <summary>
    /// Adds aliases to this command.
    /// </summary>
    /// <param name="aliases">Aliases to add to the command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithAliases(params string[] aliases)
    {
        if (aliases == null || !aliases.Any())
        {
            throw new ArgumentException("You need to pass at least one alias.", nameof(aliases));
        }

        foreach (string? alias in aliases)
        {
            this.WithAlias(alias);
        }

        return this;
    }

    /// <summary>
    /// Adds an alias to this command.
    /// </summary>
    /// <param name="alias">Alias to add to the command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithAlias(string alias)
    {
        if (alias.ToCharArray().Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("Aliases cannot contain whitespace characters or null strings.", nameof(alias));
        }

        if (this.Name == alias || this._aliasList.Contains(alias))
        {
            throw new ArgumentException("Aliases cannot contain the command name, and cannot be duplicate.", nameof(alias));
        }

        this._aliasList.Add(alias);
        return this;
    }

    /// <summary>
    /// Sets the description for this command.
    /// </summary>
    /// <param name="description">Description to use for this command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithDescription(string description)
    {
        this.Description = description;
        return this;
    }

    /// <summary>
    /// Sets whether this command is to be hidden.
    /// </summary>
    /// <param name="hidden">Whether the command is to be hidden.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithHiddenStatus(bool hidden)
    {
        this.IsHidden = hidden;
        return this;
    }

    /// <summary>
    /// Adds pre-execution checks to this command.
    /// </summary>
    /// <param name="checks">Pre-execution checks to add to this command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithExecutionChecks(params CheckBaseAttribute[] checks)
    {
        this._executionCheckList.AddRange(checks.Except(this._executionCheckList));
        return this;
    }

    /// <summary>
    /// Adds a pre-execution check to this command.
    /// </summary>
    /// <param name="check">Pre-execution check to add to this command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithExecutionCheck(CheckBaseAttribute check)
    {
        if (!this._executionCheckList.Contains(check))
        {
            this._executionCheckList.Add(check);
        }
        return this;
    }

    /// <summary>
    /// Adds overloads to this command. An executable command needs to have at least one overload.
    /// </summary>
    /// <param name="overloads">Overloads to add to this command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithOverloads(params CommandOverloadBuilder[] overloads)
    {
        foreach (CommandOverloadBuilder? overload in overloads)
        {
            this.WithOverload(overload);
        }

        return this;
    }

    /// <summary>
    /// Adds an overload to this command. An executable command needs to have at least one overload.
    /// </summary>
    /// <param name="overload">Overload to add to this command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithOverload(CommandOverloadBuilder overload)
    {
        if (this._overloadArgumentSets.Contains(overload._argumentSet))
        {
            throw new DuplicateOverloadException(this.Name, overload.Arguments.Select(x => x.Type).ToList(), overload._argumentSet);
        }

        this._overloadArgumentSets.Add(overload._argumentSet);
        this._overloadList.Add(overload);

        return this;
    }

    /// <summary>
    /// Adds a custom attribute to this command. This can be used to indicate various custom information about a command.
    /// </summary>
    /// <param name="attribute">Attribute to add.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithCustomAttribute(Attribute attribute)
    {
        this._customAttributeList.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds multiple custom attributes to this command. This can be used to indicate various custom information about a command.
    /// </summary>
    /// <param name="attributes">Attributes to add.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithCustomAttributes(params Attribute[] attributes)
    {
        foreach (Attribute? attr in attributes)
        {
            this.WithCustomAttribute(attr);
        }

        return this;
    }

    internal virtual Command Build(CommandGroup? parent)
    {
        Command? cmd = new()
        {
            Name = string.IsNullOrWhiteSpace(this.Name)
                ? throw new InvalidOperationException($"Cannot build a command with an invalid name. Use the method {nameof(this.WithName)} to set a valid name.")
                : this.Name,

            Category = this.Category,
            Description = this.Description,
            Aliases = this.Aliases,
            ExecutionChecks = this.ExecutionChecks,
            IsHidden = this.IsHidden,
            Parent = parent,
            Overloads = new ReadOnlyCollection<CommandOverload>(this.Overloads.Select(xo => xo.Build()).ToList()),
            Module = this.Module,
            CustomAttributes = this.CustomAttributes
        };

        return cmd;
    }
}
