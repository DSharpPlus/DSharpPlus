using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
            Attributes = new(attributes);
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

            CommandArgumentBuilder builder = new();
            builder.WithType(parameterInfo.ParameterType);
            builder.WithAttributes(parameterInfo.GetCustomAttributes());
            if (parameterInfo.HasDefaultValue)
            {
                builder.WithDefaultValue(parameterInfo.RawDefaultValue);
            }

            foreach (Attribute attribute in builder.Attributes)
            {
                if (attribute is ArgumentAttribute argumentAttribute)
                {
                    builder.WithName(argumentAttribute.Name);
                }
                else if (attribute is DescriptionAttribute descriptionAttribute)
                {
                    builder.WithDescription(descriptionAttribute.Description);
                }
            }

            // If no CommandArgumentAttribute is present, try to use the parameter name instead.
            if (string.IsNullOrWhiteSpace(builder.Name))
            {
                if (string.IsNullOrWhiteSpace(parameterInfo.Name))
                {
                    throw new InvalidOperationException($"The parameter is lacking a name from {nameof(ParameterInfo.Name)} and {nameof(ArgumentAttribute)} is not present.");
                }

                builder.WithName(parameterInfo.Name);
            }

            // If no DescriptionAttribute is present, use a default description.
            if (string.IsNullOrWhiteSpace(builder.Description))
            {
                builder.WithDescription("No description provided.");
            }

            return builder;
        }
    }
}
