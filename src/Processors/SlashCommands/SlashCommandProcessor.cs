using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.Exceptions;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public sealed class SlashCommandProcessor : ICommandProcessor<InteractionCreateEventArgs>
    {
        public IReadOnlyDictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> Converters { get; private set; } = new Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>>();
        public IReadOnlyDictionary<Type, ApplicationCommandOptionType> TypeMappings { get; private set; } = new Dictionary<Type, ApplicationCommandOptionType>();
        public IReadOnlyDictionary<ulong, Command> Commands { get; private set; } = new Dictionary<ulong, Command>();

        private readonly Dictionary<Type, ISlashArgumentConverter> _converters = [];
        private CommandAllExtension? _extension;
        private ILogger<SlashCommandProcessor> _logger = NullLogger<SlashCommandProcessor>.Instance;
        private bool _eventsRegistered;

        public void AddConverter<T>(ISlashArgumentConverter<T> converter) => _converters.Add(typeof(T), converter);
        public void AddConverter(Type type, ISlashArgumentConverter converter)
        {
            if (!converter.GetType().IsAssignableTo(typeof(ISlashArgumentConverter<>).MakeGenericType(type)))
            {
                throw new ArgumentException($"Type '{converter.GetType().FullName}' must implement '{typeof(ISlashArgumentConverter<>).MakeGenericType(type).FullName}'", nameof(converter));
            }

            _converters.Add(type, converter);
        }
        public void AddConverters(Assembly assembly) => AddConverters(assembly.GetTypes());
        public void AddConverters(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                // Ignore types that don't have a concrete implementation (abstract classes or interfaces)
                // Additionally ignore types that have open generics (ISlashArgumentConverter<T>) instead of closed generics (ISlashArgumentConverter<string>)
                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                // Check if the type implements ISlashArgumentConverter<T>
                Type? genericArgumentConverter = type.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISlashArgumentConverter<>));
                if (genericArgumentConverter is null)
                {
                    continue;
                }

                try
                {
                    // Try to create an instance of the converter.
                    object? converter = Activator.CreateInstance(type);
                    if (converter is null)
                    {
                        // Check to see if we have a service provider available
                        if (_extension is null)
                        {
                            // If not, we can't create the converter. Just skip it.
                            continue;
                        }

                        // If we do, try to create the converter using the service provider.
                        converter = ActivatorUtilities.CreateInstance(_extension.ServiceProvider, type);
                    }

                    // GenericTypeArguments[0] here is the T in ISlashArgumentConverter<T>
                    AddConverter(genericArgumentConverter.GenericTypeArguments[0], (ISlashArgumentConverter)converter);
                }
                catch (Exception error)
                {
                    // Log the error if possible
                    SlashLogging.FailedConverterCreation(_logger, type.FullName ?? type.Name, error);
                }
            }
        }

        public Task ConfigureAsync(CommandAllExtension extension, ConfigureCommandsEventArgs eventArgs)
        {
            _extension = extension;
            _logger = extension.ServiceProvider.GetService<ILogger<SlashCommandProcessor>>() ?? NullLogger<SlashCommandProcessor>.Instance;
            AddConverters(typeof(SlashCommandProcessor).Assembly);

            Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> converters = [];
            Dictionary<Type, ApplicationCommandOptionType> typeMappings = [];
            foreach ((Type type, ISlashArgumentConverter converter) in _converters)
            {
                MethodInfo executeConvertAsyncMethod = typeof(SlashCommandProcessor)
                    .GetMethod(nameof(ExecuteConvertAsync), BindingFlags.NonPublic | BindingFlags.Static)?
                    .MakeGenericMethod(type) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");

                converters.Add(type, executeConvertAsyncMethod.CreateDelegate<ConverterDelegate<InteractionCreateEventArgs>>(converter));
                typeMappings.Add(type, converter.ArgumentType);
            }

            Converters = converters.ToFrozenDictionary();
            TypeMappings = typeMappings.ToFrozenDictionary();
            if (_eventsRegistered)
            {
                return RegisterSlashCommandsAsync(extension);
            }

            _eventsRegistered = true;
            extension.Client.GuildDownloadCompleted += async (client, eventArgs) => await RegisterSlashCommandsAsync(extension);
            extension.Client.InteractionCreated += ExecuteInteractionAsync;

            return Task.CompletedTask;
        }

        public async Task ExecuteInteractionAsync(DiscordClient client, InteractionCreateEventArgs eventArgs)
        {
            if (_extension is null)
            {
                throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
            }
            else if (eventArgs.Interaction.Type != InteractionType.ApplicationCommand)
            {
                SlashLogging.UnknownInteractionType(_logger, eventArgs.Interaction.Type, null);
                return;
            }

            if (!TryFindCommand(eventArgs.Interaction, out Command? command))
            {
                SlashLogging.UnknownCommandName(_logger, eventArgs.Interaction.Data.Name, null);
                return;
            }

            SlashConverterContext converterContext = new()
            {
                Interaction = eventArgs.Interaction,
                Extension = _extension,
                Command = command,
                Channel = eventArgs.Interaction.Channel,
                User = eventArgs.Interaction.User,
            };

            CommandContext? commandContext = await ParseArgumentsAsync(converterContext, eventArgs);
            if (commandContext is null)
            {
                return;
            }

            await _extension.CommandExecutor.ExecuteAsync(commandContext);
        }

        public bool TryFindCommand(DiscordInteraction interaction, [NotNullWhen(true)] out Command? command)
        {
            if (!Commands.TryGetValue(interaction.Data.Id, out command))
            {
                return false;
            }

            // Resolve subcommands, which do not have id's.
            IEnumerable<DiscordInteractionDataOption> options = interaction.Data.Options;
            while (options is not null && options.Any())
            {
                DiscordInteractionDataOption option = options.First();
                if (option.Type is not ApplicationCommandOptionType.SubCommandGroup and not ApplicationCommandOptionType.SubCommand)
                {
                    break;
                }

                command = command.Subcommands.First(x => x.Name == option.Name);
                options = option.Options;
            }

            return true;
        }

        public async Task RegisterSlashCommandsAsync(CommandAllExtension extension)
        {
            List<DiscordApplicationCommand> applicationCommands = [];
            foreach (Command command in extension.Commands.Values)
            {
                applicationCommands.Add(await ToApplicationCommandAsync(command));
            }

            IReadOnlyList<DiscordApplicationCommand> commands = extension.DebugGuildId is null
                ? await extension.Client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommands)
                : await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId.Value, applicationCommands);

            Dictionary<ulong, Command> commandsDictionary = [];
            foreach (DiscordApplicationCommand command in commands)
            {
                commandsDictionary.Add(command.Id, extension.Commands[command.Name]);
            }

            Commands = commandsDictionary.ToFrozenDictionary();
            SlashLogging.RegisteredCommands(_logger, Commands.Count, null);
        }

        public async Task<DiscordApplicationCommand> ToApplicationCommandAsync(Command command)
        {
            if (_extension is null)
            {
                throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
            }

            // Translate the command's name and description.
            IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
            IReadOnlyDictionary<string, string> descriptionLocalizations = new Dictionary<string, string>();
            if (command.Attributes.OfType<SlashLocalizerAttribute>().FirstOrDefault() is SlashLocalizerAttribute localizerAttribute)
            {
                nameLocalizations = await localizerAttribute.LocalizeAsync(_extension.ServiceProvider, $"{command.FullName}.name");
                descriptionLocalizations = await localizerAttribute.LocalizeAsync(_extension.ServiceProvider, $"{command.FullName}.description");
            }

            // Convert the subcommands or arguments into application options
            List<DiscordApplicationCommandOption> options = [];
            if (command.Subcommands.Any())
            {
                foreach (Command subCommand in command.Subcommands)
                {
                    options.Add(await ToApplicationArgumentAsync(subCommand));
                }
            }
            else
            {
                foreach (CommandArgument argument in command.Arguments)
                {
                    options.Add(await ToApplicationArgumentAsync(command, argument));
                }
            }

            // Create the top level application command.
            return new(
                name: command.Name,
                description: command.Description,
                options: options,
                type: ApplicationCommandType.SlashCommand,
                name_localizations: nameLocalizations,
                description_localizations: descriptionLocalizations,
                allowDMUsage: command.Attributes.Any(x => x is AllowDMUsageAttribute),
                defaultMemberPermissions: command.Attributes.OfType<PermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? Permissions.None,
                nsfw: command.Attributes.Any(x => x is NsfwAttribute)
            );
        }

        public async Task<DiscordApplicationCommandOption> ToApplicationArgumentAsync(Command command)
        {
            if (_extension is null)
            {
                throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
            }

            // Convert the subcommands or arguments into application options
            List<DiscordApplicationCommandOption> options = [];
            if (command.Subcommands.Any())
            {
                foreach (Command subCommand in command.Subcommands)
                {
                    options.Add(await ToApplicationArgumentAsync(subCommand));
                }
            }
            else
            {
                foreach (CommandArgument argument in command.Arguments)
                {
                    options.Add(await ToApplicationArgumentAsync(command, argument));
                }
            }

            // Translate the subcommand's name and description.
            IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
            IReadOnlyDictionary<string, string> descriptionLocalizations = new Dictionary<string, string>();
            if (command.Attributes.OfType<SlashLocalizerAttribute>().FirstOrDefault() is SlashLocalizerAttribute localizerAttribute)
            {
                nameLocalizations = await localizerAttribute.LocalizeAsync(_extension.ServiceProvider, $"{command.FullName}.name");
                descriptionLocalizations = await localizerAttribute.LocalizeAsync(_extension.ServiceProvider, $"{command.FullName}.description");
            }

            return new(
                name: command.Name,
                description: command.Description,
                name_localizations: nameLocalizations,
                description_localizations: descriptionLocalizations,
                type: command.Subcommands.Any() ? ApplicationCommandOptionType.SubCommandGroup : ApplicationCommandOptionType.SubCommand,
                options: options
            );
        }

        public async Task<DiscordApplicationCommandOption> ToApplicationArgumentAsync(Command command, CommandArgument argument)
        {
            if (_extension is null)
            {
                throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
            }

            if (!TypeMappings.TryGetValue(argument.Type, out ApplicationCommandOptionType type))
            {
                throw new InvalidOperationException($"No type mapping found for argument type '{argument.Type.Name}'");
            }

            SlashMinMaxValueAttribute? minMaxValue = argument.Attributes.OfType<SlashMinMaxValueAttribute>().FirstOrDefault();
            SlashMinMaxLengthAttribute? minMaxLength = argument.Attributes.OfType<SlashMinMaxLengthAttribute>().FirstOrDefault();

            // Translate the argument's name and description.
            IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
            IReadOnlyDictionary<string, string> descriptionLocalizations = new Dictionary<string, string>();
            if (argument.Attributes.OfType<SlashLocalizerAttribute>().FirstOrDefault() is SlashLocalizerAttribute localizerAttribute)
            {
                nameLocalizations = await localizerAttribute.LocalizeAsync(_extension.ServiceProvider, $"{command.FullName}.arguments.{argument.Name}.name");
                descriptionLocalizations = await localizerAttribute.LocalizeAsync(_extension.ServiceProvider, $"{command.FullName}.arguments.{argument.Name}.description");
            }

            IEnumerable<DiscordApplicationCommandOptionChoice> choices = [];
            if (argument.Attributes.OfType<SlashChoiceProviderAttribute>().FirstOrDefault() is SlashChoiceProviderAttribute choiceAttribute)
            {
                choices = await choiceAttribute.GrabChoicesAsync(_extension.ServiceProvider, argument);
            }

            return new(
                name: argument.Name,
                description: argument.Description,
                name_localizations: nameLocalizations,
                description_localizations: descriptionLocalizations,
                channelTypes: argument.Attributes.OfType<SlashChannelTypesAttribute>().FirstOrDefault()?.ChannelTypes ?? [],
                minLength: minMaxLength?.MinLength,
                maxLength: minMaxLength?.MaxLength,
                minValue: minMaxValue?.MinValue!, // Incorrect nullable annotations within the lib
                maxValue: minMaxValue?.MaxValue!, // Incorrect nullable annotations within the lib
                type: type,
                choices: choices,
                required: !argument.DefaultValue.HasValue
            );
        }

        private async Task<CommandContext?> ParseArgumentsAsync(SlashConverterContext converterContext, InteractionCreateEventArgs eventArgs)
        {
            List<object?> parameters = [];
            try
            {
                while (converterContext.NextArgument())
                {
                    IOptional optional = await Converters[converterContext.Argument.Type](converterContext, eventArgs);
                    if (!optional.HasValue)
                    {
                        break;
                    }

                    parameters.Add(optional.RawValue);
                }
            }
            catch (Exception error)
            {
                if (_extension is null)
                {
                    return null;
                }

                await _extension._commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                {
                    Context = new SlashContext()
                    {
                        User = eventArgs.Interaction.User,
                        Channel = eventArgs.Interaction.Channel,
                        Extension = converterContext.Extension,
                        Command = converterContext.Command,
                        Arguments = converterContext.Command.Arguments.ToDictionary(
                            argument => argument,
                            argument => parameters.Count > converterContext.Command.Arguments.IndexOf(argument) ? parameters[converterContext.Command.Arguments.IndexOf(argument)] : null),
                        Interaction = eventArgs.Interaction
                    },
                    Exception = new ParseArgumentException(converterContext.Argument, error)
                });
            }

            return new SlashContext()
            {
                User = eventArgs.Interaction.User,
                Channel = eventArgs.Interaction.Channel,
                Extension = converterContext.Extension,
                Command = converterContext.Command,
                Arguments = converterContext.Command.Arguments.ToDictionary(x => x, x => parameters[converterContext.Command.Arguments.IndexOf(x)]),
                Interaction = eventArgs.Interaction
            };
        }

        private static async Task<IOptional> ExecuteConvertAsync<T>(ISlashArgumentConverter<T> converter, ConverterContext context, InteractionCreateEventArgs eventArgs) => await converter.ConvertAsync(context, eventArgs);
    }
}
