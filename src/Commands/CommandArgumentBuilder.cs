using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Commands
{
    public record CommandArgumentBuilder
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Type? Type { get; set; }
        public List<Attribute> Attributes { get; set; } = [];
        public Optional<object?> DefaultValue { get; set; } = Optional.FromNoValue<object?>();

        public CommandArgumentBuilder WithName(string name)
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

        public CommandArgumentBuilder WithDescription(string description)
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

        public CommandArgumentBuilder WithType(Type type)
        {
            Type = type;
            return this;
        }

        public CommandArgumentBuilder WithAttributes(IEnumerable<Attribute> attributes)
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

        public CommandArgumentBuilder WithDefaultValue(Optional<object?> defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        [MemberNotNull(nameof(Name), nameof(Description), nameof(Type), nameof(Attributes))]
        public CommandArgument Build()
        {
            ArgumentNullException.ThrowIfNull(Name, nameof(Name));
            ArgumentNullException.ThrowIfNull(Description, nameof(Description));
            ArgumentNullException.ThrowIfNull(Type, nameof(Type));
            ArgumentNullException.ThrowIfNull(Attributes, nameof(Attributes));
            ArgumentNullException.ThrowIfNull(DefaultValue, nameof(DefaultValue));

            // Push it through the With* methods again, which contain validation.
            WithName(Name);
            WithDescription(Description);
            WithType(Type);
            WithAttributes(Attributes);
            WithDefaultValue(DefaultValue);

            return new CommandArgument()
            {
                Name = Name,
                Description = Description,
                Type = Type,
                Attributes = Attributes,
                DefaultValue = DefaultValue
            };
        }

        public static CommandArgumentBuilder From(ParameterInfo parameterInfo)
        {
            ArgumentNullException.ThrowIfNull(parameterInfo, nameof(parameterInfo));
            if (parameterInfo.ParameterType.IsAssignableTo(typeof(CommandContext)))
            {
                throw new ArgumentException("The parameter cannot be a CommandContext.", nameof(parameterInfo));
            }

            CommandArgumentBuilder commandArgumentBuilder = new();
            commandArgumentBuilder.WithAttributes(parameterInfo.GetCustomAttributes());
            if (parameterInfo.HasDefaultValue)
            {
                commandArgumentBuilder.WithDefaultValue(parameterInfo.DefaultValue);
            }

            if (string.IsNullOrWhiteSpace(commandArgumentBuilder.Name) && !string.IsNullOrWhiteSpace(parameterInfo.Name))
            {
                commandArgumentBuilder.WithName(ToSnakeCase(parameterInfo.Name));
            }

            if (commandArgumentBuilder.Type is null)
            {
                commandArgumentBuilder.WithType(parameterInfo.ParameterType);
            }

            return commandArgumentBuilder;
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
    }
}
