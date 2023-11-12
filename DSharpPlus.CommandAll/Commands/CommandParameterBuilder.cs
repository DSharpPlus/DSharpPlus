using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Commands;

public record CommandParameterBuilder
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
        else if (name.Length is < 1 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "The name of the command must be between 1 and 32 characters.");
        }

        Name = name;
        return this;
    }

    public CommandParameterBuilder WithDescription(string description)
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

    public CommandParameterBuilder WithType(Type type)
    {
        Type = type;
        if (type.IsEnum && !Attributes.Any(attribute => attribute is SlashChoiceProviderAttribute))
        {
            Attributes.Add(new SlashChoiceProviderAttribute<EnumOptionProvider>());
        }

        return this;
    }

    public CommandParameterBuilder WithAttributes(IEnumerable<Attribute> attributes)
    {
        Attributes = new List<Attribute>(attributes);
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
        if (parameterInfo.HasDefaultValue)
        {
            commandParameterBuilder.WithDefaultValue(parameterInfo.DefaultValue);
        }

        if (string.IsNullOrWhiteSpace(commandParameterBuilder.Name) && !string.IsNullOrWhiteSpace(parameterInfo.Name))
        {
            commandParameterBuilder.WithName(ToSnakeCase(parameterInfo.Name));
        }

        if (commandParameterBuilder.Type is null)
        {
            commandParameterBuilder.WithType(parameterInfo.ParameterType);
        }

        return commandParameterBuilder;
    }

    private static string ToSnakeCase(string str)
    {
        StringBuilder stringBuilder = new();
        foreach (char character in str)
        {
            // kebab-cased, somehow.
            if (character == '-')
            {
                stringBuilder.Append('_');
                continue;
            }

            // camelCase, PascalCase
            if (char.IsUpper(character))
            {
                stringBuilder.Append('_');
            }

            stringBuilder.Append(char.ToLowerInvariant(character));
        }

        return stringBuilder.ToString();
    }

    public class EnumOptionProvider : IChoiceProvider
    {
        public Task<Dictionary<string, object>> ProvideAsync(CommandParameter parameter)
        {
            string[] enumNames = Enum.GetNames(parameter.Type);
            Array enumValues = Enum.GetValuesAsUnderlyingType(parameter.Type);

            Dictionary<string, object> choices = [];
            for (int i = 0; i < enumNames.Length; i++)
            {
                choices.Add(enumNames[i], Convert.ToDouble(enumValues.GetValue(i), CultureInfo.InvariantCulture));
            }

            return Task.FromResult(choices);
        }
    }
}
