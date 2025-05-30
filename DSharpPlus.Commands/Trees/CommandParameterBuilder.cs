#pragma warning disable CA2264

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees;

[DebuggerDisplay("{ToString()}")]
public partial class CommandParameterBuilder
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Type? Type { get; set; }
    public List<Attribute> Attributes { get; set; } = [];
    public Optional<object?> DefaultValue { get; set; } = Optional.FromNoValue<object?>();
    public CommandBuilder? Parent { get; set; }

    public CommandParameterBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "The name of the command cannot be null or whitespace.");
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
            else if (attribute is ParamArrayAttribute && !this.Attributes.Any(attribute => attribute is VariadicArgumentAttribute))
            {
                // Transform the params into a VariadicArgumentAttribute
                listedAttributes.Add(new VariadicArgumentAttribute(int.MaxValue));
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

    public CommandParameterBuilder WithParent(CommandBuilder parent)
    {
        this.Parent = parent;
        return this;
    }

    [MemberNotNull(nameof(Name), nameof(Description), nameof(Type), nameof(Attributes))]
    public CommandParameter Build(Command command)
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
            DefaultValue = this.DefaultValue,
            Parent = command,
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

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        if (this.Parent is not null)
        {
            stringBuilder.Append(this.Parent.FullName);
            stringBuilder.Append('.');
        }

        stringBuilder.Append(this.Name ?? "Unnamed Parameter");
        return stringBuilder.ToString();
    }
}
