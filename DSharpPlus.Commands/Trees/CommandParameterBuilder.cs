using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees;

public partial class CommandParameterBuilder
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Type? Type { get; set; }
    public List<Attribute> Attributes { get; set; } = [];
    public Optional<object?> DefaultValue { get; set; } = Optional.FromNoValue<object?>();

    public CommandParameterBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "The name of the command cannot be null or whitespace.");
        }

        if (!this.Attributes.Any(x => x is SnakeCasedNameAttribute))
        {
            this.Attributes.Add(new SnakeCasedNameAttribute(SlashCommandProcessor.ToSnakeCase(name)));
        }

        this.Name = name;
        return this;
    }

    public CommandParameterBuilder WithDescription(string? description)
    {
        this.Description = description;
        return this;
    }

    public CommandParameterBuilder WithType(Type type)
    {
        this.Type = type;

        if (type.IsEnum || (type is { Namespace: "System", Name: "Nullable`1" } && type.GetGenericArguments()[0].IsEnum))
        {
            if (this.Attributes.All(attribute => attribute is not SlashChoiceProviderAttribute and not SlashAutoCompleteProviderAttribute))
            {
                this.Attributes.Add(new SlashChoiceProviderAttribute<EnumOptionProvider>());
            }
        }

        return this;
    }

    public CommandParameterBuilder WithAttributes(IEnumerable<Attribute> attributes)
    {
        List<Attribute> listedAttributes = [];
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

            listedAttributes.Add(attribute);
        }

        this.Attributes = listedAttributes;
        return this;
    }

    public CommandParameterBuilder WithDefaultValue(Optional<object?> defaultValue)
    {
        this.DefaultValue = defaultValue;
        return this;
    }

    [MemberNotNull(nameof(Name), nameof(Description), nameof(Type), nameof(Attributes))]
    public CommandParameter Build()
    {
        ArgumentNullException.ThrowIfNull(this.Name, nameof(this.Name));
        ArgumentNullException.ThrowIfNull(this.Description, nameof(this.Description));
        ArgumentNullException.ThrowIfNull(this.Type, nameof(this.Type));
        ArgumentNullException.ThrowIfNull(this.Attributes, nameof(this.Attributes));
        ArgumentNullException.ThrowIfNull(this.DefaultValue, nameof(this.DefaultValue));

        // Push it through the With* methods again, which contain validation.
        WithName(this.Name);
        WithDescription(this.Description);
        WithAttributes(this.Attributes);
        WithType(this.Type);
        WithDefaultValue(this.DefaultValue);

        return new CommandParameter()
        {
            Name = this.Name,
            Description = this.Description,
            Type = this.Type,
            Attributes = this.Attributes,
            DefaultValue = this.DefaultValue
        };
    }

    public static CommandParameterBuilder From(ParameterInfo parameterInfo)
    {
        ArgumentNullException.ThrowIfNull(parameterInfo, nameof(parameterInfo));
        if (parameterInfo.ParameterType.IsAssignableTo(typeof(CommandContext)))
        {
            throw new ArgumentException("The parameter cannot be a CommandContext.", nameof(parameterInfo));
        }

        CommandParameterBuilder commandParameterBuilder = new();
        commandParameterBuilder.WithAttributes(parameterInfo.GetCustomAttributes());
        commandParameterBuilder.WithType(parameterInfo.ParameterType);
        if (parameterInfo.HasDefaultValue)
        {
            commandParameterBuilder.WithDefaultValue(parameterInfo.DefaultValue);
        }

        if (parameterInfo.GetCustomAttribute<ParameterAttribute>() is ParameterAttribute attribute)
        {
            commandParameterBuilder.WithName(attribute.Name);
        }
        else if (!string.IsNullOrWhiteSpace(parameterInfo.Name))
        {
            commandParameterBuilder.WithName(parameterInfo.Name);
        }

        // Might be set by the `DescriptionAttribute`
        if (string.IsNullOrWhiteSpace(commandParameterBuilder.Description))
        {
            commandParameterBuilder.WithDescription("No description provided.");
        }

        return commandParameterBuilder;
    }
}
