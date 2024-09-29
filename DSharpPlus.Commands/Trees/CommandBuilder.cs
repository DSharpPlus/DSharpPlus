using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DSharpPlus.Commands.Trees;

[DebuggerDisplay("{ToString()}")]
public class CommandBuilder
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public MethodInfo? Method { get; set; }
    public object? Target { get; set; }
    public CommandBuilder? Parent { get; set; }
    public List<CommandBuilder> Subcommands { get; set; } = [];
    public List<CommandParameterBuilder> Parameters { get; set; } = [];
    public List<Attribute> Attributes { get; set; } = [];
    public List<ulong> GuildIds { get; set; } = [];
    public string? FullName =>
        this.Parent is not null ? $"{this.Parent.FullName}.{this.Name}" : this.Name;

    public CommandBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(
                nameof(name),
                "The name of the command cannot be null or whitespace."
            );
        }

        this.Name = name;
        return this;
    }

    public CommandBuilder WithDescription(string? description)
    {
        this.Description = description;
        return this;
    }

    public CommandBuilder WithDelegate(Delegate? method) =>
        WithDelegate(method?.Method, method?.Target);

    public CommandBuilder WithDelegate(MethodInfo? method, object? target = null)
    {
        if (method is not null)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (
                parameters.Length == 0
                || !parameters[0].ParameterType.IsAssignableTo(typeof(CommandContext))
            )
            {
                throw new ArgumentException(
                    $"The command method \"{(method.DeclaringType is not null ? $"{method.DeclaringType.FullName}.{method.Name}" : method.Name)}\" must have it's first parameter be a CommandContext.",
                    nameof(method)
                );
            }
        }

        this.Method = method;
        this.Target = target;
        return this;
    }

    public CommandBuilder WithParent(CommandBuilder? parent)
    {
        this.Parent = parent;
        return this;
    }

    public CommandBuilder WithSubcommands(IEnumerable<CommandBuilder> subcommands)
    {
        this.Subcommands = new(subcommands);
        return this;
    }

    public CommandBuilder WithParameters(IEnumerable<CommandParameterBuilder> parameters)
    {
        this.Parameters = new(parameters);
        foreach (CommandParameterBuilder parameter in parameters)
        {
            parameter.Parent ??= this;
        }

        return this;
    }

    public CommandBuilder WithAttributes(IEnumerable<Attribute> attributes)
    {
        this.Attributes = new(attributes);
        foreach (Attribute attribute in attributes)
        {
            if (attribute is CommandAttribute commandAttribute)
            {
                WithName(commandAttribute.Name);
            }
            else if (attribute is DescriptionAttribute descriptionAttribute)
            {
                WithDescription(descriptionAttribute.Description);
            }
            else if (attribute is RegisterToGuildsAttribute registerToGuildsAttribute)
            {
                WithGuildIds(registerToGuildsAttribute.GuildIds);
            }
        }

        return this;
    }

    public CommandBuilder WithGuildIds(IEnumerable<ulong> guildIds)
    {
        this.GuildIds = new(guildIds);
        return this;
    }

    [MemberNotNull(nameof(Name), nameof(Subcommands), nameof(Parameters), nameof(Attributes))]
    public Command Build(Command? parent = null)
    {
        ArgumentNullException.ThrowIfNull(this.Name, nameof(this.Name));
        ArgumentNullException.ThrowIfNull(this.Subcommands, nameof(this.Subcommands));
        ArgumentNullException.ThrowIfNull(this.Parameters, nameof(this.Parameters));
        ArgumentNullException.ThrowIfNull(this.Attributes, nameof(this.Attributes));

        // Push it through the With* methods again, which contain validation.
        WithName(this.Name);
        WithDescription(this.Description);
        WithDelegate(this.Method, this.Target);
        WithSubcommands(this.Subcommands);
        WithParameters(this.Parameters);
        WithAttributes(this.Attributes);
        WithGuildIds(this.GuildIds);

        return new(this.Subcommands, this.Parameters)
        {
            Name = this.Name,
            Description = this.Description,
            Method = this.Method,
            Id = Ulid.NewUlid(),
            Target = this.Target,
            Parent = parent,
            Attributes = this.Attributes,
            GuildIds = this.GuildIds,
        };
    }

    /// <summary>
    /// Traverses this command tree, returning this command builder and all subcommands recursively.
    /// </summary>
    /// <returns>A list of all command builders in this tree.</returns>
    public IReadOnlyList<CommandBuilder> Flatten()
    {
        List<CommandBuilder> commands = [this];
        foreach (CommandBuilder subcommand in this.Subcommands)
        {
            commands.AddRange(subcommand.Flatten());
        }

        return commands;
    }

    /// <inheritdoc cref="From(Type, ulong[])"/>
    public static CommandBuilder From<T>() => From(typeof(T), []);

    /// <inheritdoc cref="From(Type, ulong[])"/>
    /// <typeparam name="T">The type that'll be searched for subcommands.</typeparam>
    public static CommandBuilder From<T>(params ulong[] guildIds) => From(typeof(T), guildIds);

    /// <inheritdoc cref="From(Type, ulong[])"/>
    public static CommandBuilder From(Type type) => From(type, []);

    /// <summary>
    /// Creates a new group <see cref="CommandBuilder"/> from the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type that'll be searched for subcommands.</param>
    /// <param name="guildIds">The guild IDs that this command will be registered in.</param>
    /// <returns>A new <see cref="CommandBuilder"/> which does it's best to build a pre-filled <see cref="CommandBuilder"/> from the specified <paramref name="type"/>.</returns>
    public static CommandBuilder From(Type type, params ulong[] guildIds)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        CommandBuilder commandBuilder = new();
        commandBuilder.WithAttributes(type.GetCustomAttributes());
        commandBuilder.GuildIds.AddRange(guildIds);

        // Add subcommands
        List<CommandBuilder> subCommandBuilders = [];
        foreach (
            Type subCommand in type.GetNestedTypes(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
            )
        )
        {
            if (subCommand.GetCustomAttribute<CommandAttribute>() is null)
            {
                continue;
            }

            subCommandBuilders.Add(
                From(subCommand, [.. commandBuilder.GuildIds]).WithParent(commandBuilder)
            );
        }

        // Add methods
        foreach (
            MethodInfo method in type.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
            )
        )
        {
            if (method.GetCustomAttribute<CommandAttribute>() is null)
            {
                continue;
            }

            subCommandBuilders.Add(
                From(method, guildIds: [.. commandBuilder.GuildIds]).WithParent(commandBuilder)
            );
        }

        if (
            type.GetCustomAttribute<CommandAttribute>() is not null
            && subCommandBuilders.Count == 0
        )
        {
            throw new ArgumentException(
                $"The type \"{type.FullName ?? type.Name}\" does not have any subcommands or methods with a CommandAttribute.",
                nameof(type)
            );
        }

        commandBuilder.WithSubcommands(subCommandBuilders);

        // Might be set through the `DescriptionAttribute`
        if (string.IsNullOrEmpty(commandBuilder.Description))
        {
            commandBuilder.WithDescription("No description provided.");
        }

        return commandBuilder;
    }

    /// <inheritdoc cref="From(MethodInfo, object?, ulong[])"/>
    public static CommandBuilder From(Delegate method) => From(method.Method, method.Target, []);

    /// <inheritdoc cref="From(MethodInfo, object?, ulong[])"/>
    public static CommandBuilder From(Delegate method, params ulong[] guildIds) =>
        From(method.Method, method.Target, guildIds);

    /// <inheritdoc cref="From(MethodInfo, object?, ulong[])"/>
    public static CommandBuilder From(MethodInfo method, object? target = null) =>
        From(method, target, []);

    /// <summary>
    /// Creates a new <see cref="CommandBuilder"/> from the specified <paramref name="method"/>.
    /// </summary>
    /// <param name="method">The method that'll be invoked when the command is executed.</param>
    /// <param name="target">The object/class instance of which <paramref name="method"/> will create a delegate with.</param>
    /// <param name="guildIds">The guild IDs that this command will be registered in.</param>
    /// <returns>A new <see cref="CommandBuilder"/> which does it's best to build a pre-filled <see cref="CommandBuilder"/> from the specified <paramref name="method"/>.</returns>
    public static CommandBuilder From(
        MethodInfo method,
        object? target = null,
        params ulong[] guildIds
    )
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        if (method.GetCustomAttribute<CommandAttribute>() is null)
        {
            throw new ArgumentException(
                $"The method \"{(method.DeclaringType is not null ? $"{method.DeclaringType.FullName}.{method.Name}" : method.Name)}\" does not have a CommandAttribute.",
                nameof(method)
            );
        }

        ParameterInfo[] parameters = method.GetParameters();
        if (
            parameters.Length == 0
            || !parameters[0].ParameterType.IsAssignableTo(typeof(CommandContext))
        )
        {
            throw new ArgumentException(
                $"The command method \"{(method.DeclaringType is not null ? $"{method.DeclaringType.FullName}.{method.Name}" : method.Name)}\" must have a parameter and it must be a type of {nameof(CommandContext)}.",
                nameof(method)
            );
        }

        CommandBuilder commandBuilder = new();
        commandBuilder.WithAttributes(method.GetCustomAttributes());
        commandBuilder.WithDelegate(method, target);
        commandBuilder.WithParameters(
            parameters[1..]
                .Select(parameterInfo =>
                    CommandParameterBuilder.From(parameterInfo).WithParent(commandBuilder)
                )
        );
        commandBuilder.GuildIds.AddRange(guildIds);
        return commandBuilder;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        if (this.Parent is not null)
        {
            stringBuilder.Append(this.Parent.Name);
            stringBuilder.Append('.');
        }

        stringBuilder.Append(this.Name ?? "Unnamed Command");
        return stringBuilder.ToString();
    }
}
