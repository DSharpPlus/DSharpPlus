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
    private List<string> aliasList { get; }

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
    private List<CheckBaseAttribute> executionCheckList { get; }

    /// <summary>
    /// Gets the collection of this command's overloads.
    /// </summary>
    public IReadOnlyList<CommandOverloadBuilder> Overloads { get; }
    private List<CommandOverloadBuilder> overloadList { get; }
    private HashSet<string> overloadArgumentSets { get; }

    /// <summary>
    /// Gets the module on which this command is to be defined.
    /// </summary>
    public ICommandModule? Module { get; }

    /// <summary>
    /// Gets custom attributes defined on this command.
    /// </summary>
    public IReadOnlyList<Attribute> CustomAttributes { get; }
    private List<Attribute> customAttributeList { get; }

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
        this.aliasList = [];
        this.Aliases = new ReadOnlyCollection<string>(this.aliasList);

        this.executionCheckList = [];
        this.ExecutionChecks = new ReadOnlyCollection<CheckBaseAttribute>(this.executionCheckList);

        this.overloadArgumentSets = [];
        this.overloadList = [];
        this.Overloads = new ReadOnlyCollection<CommandOverloadBuilder>(this.overloadList);

        this.Module = module;

        this.customAttributeList = [];
        this.CustomAttributes = new ReadOnlyCollection<Attribute>(this.customAttributeList);
    }

    /// <summary>
    /// Sets the name for this command.
    /// </summary>
    /// <param name="name">Name for this command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithName(string name)
    {
        if (name == null || name.ToCharArray().Any(xc => char.IsWhiteSpace(xc)))
        {
            throw new ArgumentException("Command name cannot be null or contain any whitespace characters.", nameof(name));
        }
        else if (this.Name != null)
        {
            throw new InvalidOperationException("This command already has a name.");
        }
        else if (this.aliasList.Contains(name))
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
        if (aliases == null || aliases.Length == 0)
        {
            throw new ArgumentException("You need to pass at least one alias.", nameof(aliases));
        }

        foreach (string alias in aliases)
        {
            WithAlias(alias);
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
        if (alias.ToCharArray().Any(xc => char.IsWhiteSpace(xc)))
        {
            throw new ArgumentException("Aliases cannot contain whitespace characters or null strings.", nameof(alias));
        }

        if (this.Name == alias || this.aliasList.Contains(alias))
        {
            throw new ArgumentException("Aliases cannot contain the command name, and cannot be duplicate.", nameof(alias));
        }

        this.aliasList.Add(alias);
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
        this.executionCheckList.AddRange(checks.Except(this.executionCheckList));
        return this;
    }

    /// <summary>
    /// Adds a pre-execution check to this command.
    /// </summary>
    /// <param name="check">Pre-execution check to add to this command.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithExecutionCheck(CheckBaseAttribute check)
    {
        if (!this.executionCheckList.Contains(check))
        {
            this.executionCheckList.Add(check);
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
        foreach (CommandOverloadBuilder overload in overloads)
        {
            WithOverload(overload);
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
        if (this.overloadArgumentSets.Contains(overload.argumentSet))
        {
            throw new DuplicateOverloadException(this.Name, overload.Arguments.Select(x => x.Type).ToList(), overload.argumentSet);
        }

        this.overloadArgumentSets.Add(overload.argumentSet);
        this.overloadList.Add(overload);

        return this;
    }

    /// <summary>
    /// Adds a custom attribute to this command. This can be used to indicate various custom information about a command.
    /// </summary>
    /// <param name="attribute">Attribute to add.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithCustomAttribute(Attribute attribute)
    {
        this.customAttributeList.Add(attribute);
        return this;
    }

    /// <summary>
    /// Adds multiple custom attributes to this command. This can be used to indicate various custom information about a command.
    /// </summary>
    /// <param name="attributes">Attributes to add.</param>
    /// <returns>This builder.</returns>
    public CommandBuilder WithCustomAttributes(params Attribute[] attributes)
    {
        foreach (Attribute attr in attributes)
        {
            WithCustomAttribute(attr);
        }

        return this;
    }

    internal virtual Command Build(CommandGroup? parent)
    {
        Command cmd = new()
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
