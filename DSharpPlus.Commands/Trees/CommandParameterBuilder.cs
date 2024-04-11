namespace DSharpPlus.Commands.Trees;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.Attributes;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;

public class CommandParameterBuilder
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
        if (type.IsEnum && !this.Attributes.Any(attribute => attribute is SlashChoiceProviderAttribute))
        {
            this.Attributes.Add(new SlashChoiceProviderAttribute<EnumOptionProvider>());
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
                this.WithName(commandAttribute.Name);
            }
            else if (attribute is DescriptionAttribute descriptionAttribute)
            {
                this.WithDescription(descriptionAttribute.Description);
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
        this.WithName(this.Name);
        this.WithDescription(this.Description);
        this.WithAttributes(this.Attributes);
        this.WithType(this.Type);
        this.WithDefaultValue(this.DefaultValue);

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

        // Might be set by the `ArgumentAttribute`
        if (string.IsNullOrWhiteSpace(commandParameterBuilder.Name) && !string.IsNullOrWhiteSpace(parameterInfo.Name))
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

    public class EnumOptionProvider : IChoiceProvider
    {
        public ValueTask<Dictionary<string, object>> ProvideAsync(CommandParameter parameter)
        {
            List<string> enumNames = [];
            foreach (FieldInfo fieldInfo in parameter.Type.GetFields())
            {
                if (fieldInfo.IsSpecialName || !fieldInfo.IsStatic)
                {
                    continue;
                }
                else if (fieldInfo.GetCustomAttribute<DisplayNameAttribute>() is DisplayNameAttribute displayNameAttribute)
                {
                    enumNames.Add(displayNameAttribute.DisplayName);
                }
                else
                {
                    enumNames.Add(fieldInfo.Name);
                }
            }

            Array enumValues = Enum.GetValuesAsUnderlyingType(parameter.Type);

            Dictionary<string, object> choices = [];
            for (int i = 0; i < enumNames.Count; i++)
            {
                choices.Add(enumNames[i], Convert.ToDouble(enumValues.GetValue(i), CultureInfo.InvariantCulture));
            }

            return ValueTask.FromResult(choices);
        }
    }
}
