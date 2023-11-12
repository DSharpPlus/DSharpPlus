using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using DSharpPlus.CommandAll.Commands.Attributes;

namespace DSharpPlus.CommandAll.Commands;

public record CommandBuilder
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public MethodInfo? Method { get; set; }
    public object? Target { get; set; }
    public Command? Parent { get; set; }
    public List<CommandBuilder> Subcommands { get; set; } = [];
    public List<CommandParameterBuilder> Parameters { get; set; } = [];
    public List<Attribute> Attributes { get; set; } = [];

    public CommandBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "The name of the command cannot be null or whitespace.");
        }
        else if (name.Length is < 1 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "The name of the command must be between 1 and 32 characters.");
        }

        Name = name;
        return this;
    }

    public CommandBuilder WithDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentNullException(nameof(description), "The description of the command cannot be null or whitespace.");
        }
        else if (description.Length is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(description), "The description of the command must be between 1 and 100 characters.");
        }

        Description = description;
        return this;
    }

    public CommandBuilder WithDelegate(Delegate? method) => WithDelegate(method?.Method, method?.Target);
    public CommandBuilder WithDelegate(MethodInfo? method, object? target = null)
    {
        if (method is not null)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0 || !parameters[0].ParameterType.IsAssignableTo(typeof(CommandContext)))
            {
                throw new ArgumentException($"The command method \"{(method.DeclaringType is not null ? $"{method.DeclaringType.FullName}.{method.Name}" : method.Name)}\" must have it's first parameter be a CommandContext.", nameof(method));
            }
        }

        Method = method;
        Target = target;
        return this;
    }

    public CommandBuilder WithParent(Command? parent)
    {
        Parent = parent;
        return this;
    }

    public CommandBuilder WithSubcommands(IEnumerable<CommandBuilder> subcommands)
    {
        Subcommands = new(subcommands);
        return this;
    }

    public CommandBuilder WithParameters(IEnumerable<CommandParameterBuilder> parameters)
    {
        Parameters = new(parameters);
        return this;
    }

    public CommandBuilder WithAttributes(IEnumerable<Attribute> attributes)
    {
        Attributes = new(attributes);
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
        }

        if (string.IsNullOrEmpty(Description))
        {
            WithDescription("No description provided.");
        }

        return this;
    }

    [MemberNotNull(nameof(Name), nameof(Description), nameof(Subcommands), nameof(Parameters), nameof(Attributes))]
    public Command Build()
    {
        ArgumentNullException.ThrowIfNull(Name, nameof(Name));
        ArgumentNullException.ThrowIfNull(Description, nameof(Description));
        ArgumentNullException.ThrowIfNull(Subcommands, nameof(Subcommands));
        ArgumentNullException.ThrowIfNull(Parameters, nameof(Parameters));
        ArgumentNullException.ThrowIfNull(Attributes, nameof(Attributes));

        // Push it through the With* methods again, which contain validation.
        WithName(Name);
        WithDescription(Description);
        WithDelegate(Method);
        WithParent(Parent);
        WithSubcommands(Subcommands);
        WithParameters(Parameters);
        WithAttributes(Attributes);

        return new(Subcommands)
        {
            Name = Name,
            Description = Description,
            Method = Method,
            Target = Target,
            Parent = Parent,
            Parameters = Parameters.Select(x => x.Build()).ToArray(),
            Attributes = Attributes
        };
    }

    /// <inheritdoc cref="From(Type)"/>
    /// <typeparam name="T">The type that'll be searched for subcommands.</typeparam>
    public static CommandBuilder From<T>() => From(typeof(T));

    /// <summary>
    /// Creates a group command from the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type that'll be searched for subcommands.</param>
    /// <returns>A new <see cref="CommandBuilder"/> which does it's best to build a pre-filled <see cref="CommandBuilder"/> from the specified <paramref name="type"/>.</returns>
    public static CommandBuilder From(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        if (type.GetCustomAttribute<CommandAttribute>() is null)
        {
            throw new ArgumentException($"The type \"{type.FullName ?? type.Name}\" does not have a CommandAttribute.", nameof(type));
        }

        // Add subcommands
        List<CommandBuilder> subCommandBuilders = [];
        foreach (Type subCommand in type.GetNestedTypes(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (subCommand.GetCustomAttribute<CommandAttribute>() is null)
            {
                continue;
            }

            subCommandBuilders.Add(From(subCommand));
        }

        // Add methods
        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (method.GetCustomAttribute<CommandAttribute>() is null)
            {
                continue;
            }

            subCommandBuilders.Add(From(method));
        }

        if (subCommandBuilders.Count == 0)
        {
            throw new ArgumentException($"The type \"{type.FullName ?? type.Name}\" does not have any subcommands or methods with a CommandAttribute.", nameof(type));
        }

        CommandBuilder commandBuilder = new();
        commandBuilder.WithAttributes(type.GetCustomAttributes());
        commandBuilder.WithSubcommands(subCommandBuilders);
        return commandBuilder;
    }

    public static CommandBuilder From(Delegate method) => From(method.Method, method.Target);

    /// <summary>
    /// Creates a new <see cref="CommandBuilder"/> from the specified <paramref name="method"/>.
    /// </summary>
    /// <param name="method">The method that'll be invoked when the command is executed.</param>
    /// <param name="target">The object/class instance of which <paramref name="method"/> will create a delegate with.</param>
    /// <returns>A new <see cref="CommandBuilder"/> which does it's best to build a pre-filled <see cref="CommandBuilder"/> from the specified <paramref name="method"/>.</returns>
    public static CommandBuilder From(MethodInfo method, object? target = null)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        if (method.GetCustomAttribute<CommandAttribute>() is null)
        {
            throw new ArgumentException($"The method \"{(method.DeclaringType is not null ? $"{method.DeclaringType.FullName}.{method.Name}" : method.Name)}\" does not have a CommandAttribute.", nameof(method));
        }

        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length == 0 || !parameters[0].ParameterType.IsAssignableTo(typeof(CommandContext)))
        {
            throw new ArgumentException($"The command method \"{(method.DeclaringType is not null ? $"{method.DeclaringType.FullName}.{method.Name}" : method.Name)}\" must have a parameter and it must be a type of {nameof(CommandContext)}.", nameof(method));
        }

        CommandBuilder commandBuilder = new();
        commandBuilder.WithAttributes(method.GetCustomAttributes());
        commandBuilder.WithDelegate(method, target);
        commandBuilder.WithParameters(parameters[1..].Select(CommandParameterBuilder.From));
        return commandBuilder;
    }
}
