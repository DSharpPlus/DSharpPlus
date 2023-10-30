using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using DSharpPlus.CommandAll.Commands.Attributes;

namespace DSharpPlus.CommandAll.Commands
{
    public record CommandBuilder
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public MethodInfo? Method { get; set; }
        public object? Target { get; set; }
        public Command? Parent { get; set; }
        public List<CommandBuilder> Subcommands { get; set; } = [];
        public List<CommandArgumentBuilder> Arguments { get; set; } = [];
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
                if (parameters.Length == 0 || !parameters[0].ParameterType.IsAssignableFrom(typeof(CommandContext)))
                {
                    throw new ArgumentException("The method must have it's first parameter be a CommandContext.", nameof(method));
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

        public CommandBuilder WithArguments(IEnumerable<CommandArgumentBuilder> arguments)
        {
            Arguments = new(arguments);
            return this;
        }

        public CommandBuilder WithAttributes(IEnumerable<Attribute> attributes)
        {
            Attributes = new(attributes);
            return this;
        }

        [MemberNotNull(nameof(Name), nameof(Description), nameof(Subcommands), nameof(Arguments), nameof(Attributes))]
        public Command Build()
        {
            ArgumentNullException.ThrowIfNull(Name, nameof(Name));
            ArgumentNullException.ThrowIfNull(Description, nameof(Description));
            ArgumentNullException.ThrowIfNull(Subcommands, nameof(Subcommands));
            ArgumentNullException.ThrowIfNull(Arguments, nameof(Arguments));
            ArgumentNullException.ThrowIfNull(Attributes, nameof(Attributes));

            // Push it through the With* methods again, which contain validation.
            WithName(Name);
            WithDescription(Description);
            WithDelegate(Method);
            WithParent(Parent);
            WithSubcommands(Subcommands);
            WithArguments(Arguments);
            WithAttributes(Attributes);

            return new()
            {
                Name = Name,
                Description = Description,
                Method = Method,
                Target = Target,
                Parent = Parent,
                Subcommands = Subcommands.Select(x => x.Build()).ToArray(),
                Arguments = Arguments.Select(x => x.Build()).ToArray(),
                Attributes = Attributes
            };
        }

        public static CommandBuilder From<T>() => From(typeof(T));
        public static CommandBuilder From(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            CommandBuilder commandBuilder = new();
            commandBuilder.WithAttributes(type.GetCustomAttributes().ToList());

            foreach (Attribute attribute in commandBuilder.Attributes)
            {
                switch (attribute)
                {
                    case CommandAttribute commandAttribute:
                        commandBuilder.WithName(commandAttribute.Name);
                        break;
                    case DescriptionAttribute descriptionAttribute:
                        commandBuilder.WithDescription(descriptionAttribute.Description);
                        break;
                    default:
                        break;
                }
            }

            foreach (MethodInfo methodInfo in type.GetMethods())
            {
                if (methodInfo.GetCustomAttribute<CommandAttribute>() is not null)
                {
                    commandBuilder.WithDelegate(methodInfo);
                    commandBuilder.WithArguments(methodInfo.GetParameters()[1..].Select(CommandArgumentBuilder.From));
                }
            }

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

            CommandBuilder builder = new();
            builder.WithDelegate(method, target);
            builder.WithArguments(method.GetParameters()[1..].Select(CommandArgumentBuilder.From));
            builder.WithAttributes(method.GetCustomAttributes().ToList());
            foreach (Attribute attribute in builder.Attributes)
            {
                if (attribute is CommandAttribute commandAttribute)
                {
                    builder.WithName(commandAttribute.Name);
                }
                else if (attribute is DescriptionAttribute descriptionAttribute)
                {
                    builder.WithDescription(descriptionAttribute.Description);
                }
            }

            // If no CommandAttribute is present, try to use the method name instead.
            if (string.IsNullOrWhiteSpace(builder.Name))
            {
                builder.WithName(method.Name.Replace("Async", string.Empty));
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
