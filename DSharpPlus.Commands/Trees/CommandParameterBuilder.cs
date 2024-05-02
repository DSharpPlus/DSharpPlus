
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees;
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

        Name = name;
        return this;
    }

    public CommandParameterBuilder WithDescription(string? description)
    {
        Description = description;
        return this;
    }

    public CommandParameterBuilder WithType(Type type)
    {
        Type = type;
        if (type.IsEnum && Attributes.All(attribute => attribute is not SlashChoiceProviderAttribute and not SlashAutoCompleteProviderAttribute))
        {
            Attributes.Add(new SlashChoiceProviderAttribute<EnumOptionProvider>());
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

        Attributes = listedAttributes;
        return this;
    }

    public CommandParameterBuilder WithDefaultValue(Optional<object?> defaultValue)
    {
        DefaultValue = defaultValue;
        return this;
    }

    [MemberNotNull(nameof(Name), nameof(Description), nameof(Type), nameof(Attributes))]
    public CommandParameter Build()
    {
        ArgumentNullException.ThrowIfNull(Name, nameof(Name));
        ArgumentNullException.ThrowIfNull(Description, nameof(Description));
        ArgumentNullException.ThrowIfNull(Type, nameof(Type));
        ArgumentNullException.ThrowIfNull(Attributes, nameof(Attributes));
        ArgumentNullException.ThrowIfNull(DefaultValue, nameof(DefaultValue));

        // Push it through the With* methods again, which contain validation.
        WithName(Name);
        WithDescription(Description);
        WithAttributes(Attributes);
        WithType(Type);
        WithDefaultValue(DefaultValue);

        return new CommandParameter()
        {
            Name = Name,
            Description = Description,
            Type = Type,
            Attributes = Attributes,
            DefaultValue = DefaultValue
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

    public class EnumOptionProvider : IChoiceProvider
    {
        public ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter)
        {
            List<string> enumNames = [];
            foreach (FieldInfo fieldInfo in parameter.Type.GetFields())
            {
                if (fieldInfo.IsSpecialName || !fieldInfo.IsStatic)
                {
                    continue;
                }
                else if (fieldInfo.GetCustomAttribute<ChoiceDisplayNameAttribute>() is ChoiceDisplayNameAttribute displayNameAttribute)
                {
                    enumNames.Add(displayNameAttribute.DisplayName);
                }
                else
                {
                    enumNames.Add(fieldInfo.Name);
                }
            }

            Dictionary<string, object> choices = [];
            Array enumValues = Enum.GetValuesAsUnderlyingType(parameter.Type);
            for (int i = 0; i < enumNames.Count; i++)
            {
                string? value = enumValues.GetValue(i)?.ToString() ?? throw new InvalidOperationException($"Failed to get the value of the enum {parameter.Type.Name} for element {enumNames[i]}");
                choices.Add(enumNames[i], value.ToString());
            }

            return ValueTask.FromResult<IReadOnlyDictionary<string, object>>(choices);
        }
    }
}
