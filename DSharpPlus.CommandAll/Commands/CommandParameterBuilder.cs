namespace DSharpPlus.CommandAll.Commands;

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

        this.Name = name;
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
        this.Attributes = new List<Attribute>(attributes);
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
        }

        if (string.IsNullOrEmpty(this.Description))
        {
            this.WithDescription("No description provided.");
        }

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
