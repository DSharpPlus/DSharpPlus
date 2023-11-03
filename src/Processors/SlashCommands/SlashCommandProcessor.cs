using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.ContextChecks;
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
        private readonly List<DiscordApplicationCommand> _applicationCommands = [];
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

            _converters.TryAdd(type, converter);
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
                    object converter;

                    // Check to see if we have a service provider available
                    if (_extension is not null)
                    {
                        // If we do, try to create the converter using the service provider.
                        converter = ActivatorUtilities.CreateInstance(_extension.ServiceProvider, type);
                    }
                    else
                    {
                        // If we don't, try using a parameterless constructor.
                        converter = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Failed to create instance of {type.FullName ?? type.Name}");
                    }

                    // GenericTypeArguments[0] here is the T in ITextArgumentConverter<T>
                    AddConverter(genericArgumentConverter.GenericTypeArguments[0], (ISlashArgumentConverter)converter);
                }
                catch (Exception error)
                {
                    // Log the error if possible
                    SlashLogging.FailedConverterCreation(_logger, type.FullName ?? type.Name, error);
                }
            }
        }

        public void AddApplicationCommands(params DiscordApplicationCommand[] applicationCommands) => _applicationCommands.AddRange(applicationCommands);
        public void AddApplicationCommands(IEnumerable<DiscordApplicationCommand> applicationCommands) => _applicationCommands.AddRange(applicationCommands);

        public Task ConfigureAsync(CommandAllExtension extension)
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
            if (!_eventsRegistered)
            {
                _eventsRegistered = true;
                extension.Client.GuildDownloadCompleted += async (client, eventArgs) => await RegisterSlashCommandsAsync(extension);
                extension.Client.InteractionCreated += ExecuteInteractionAsync;
            }

            return Task.CompletedTask;
        }

        public async Task ExecuteInteractionAsync(DiscordClient client, InteractionCreateEventArgs eventArgs)
        {
            if (_extension is null)
            {
                throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
            }
            else if (eventArgs.Interaction.Type is not InteractionType.ApplicationCommand and not InteractionType.AutoComplete || eventArgs.Interaction.Data.Type is not ApplicationCommandType.SlashCommand)
            {
                return;
            }

            AsyncServiceScope scope = _extension.ServiceProvider.CreateAsyncScope();
            if (!TryFindCommand(eventArgs.Interaction, out Command? command, out IEnumerable<DiscordInteractionDataOption>? options))
            {
                await _extension._commandErrored.InvokeAsync(_extension, new CommandErroredEventArgs()
                {
                    Context = new SlashContext()
                    {
                        Arguments = new Dictionary<CommandArgument, object?>(),
                        Channel = eventArgs.Interaction.Channel,
                        Command = null!,
                        Extension = _extension,
                        ServiceScope = scope,
                        User = eventArgs.Interaction.User,
                        Interaction = eventArgs.Interaction,
                        Options = eventArgs.Interaction.Data.Options ?? []
                    },
                    CommandObject = null,
                    Exception = new CommandNotFoundException(eventArgs.Interaction.Data.Name)
                });

                await scope.DisposeAsync();
                return;
            }

            SlashConverterContext converterContext = new()
            {
                Channel = eventArgs.Interaction.Channel,
                Command = command,
                Extension = _extension,
                Interaction = eventArgs.Interaction,
                Options = options,
                ServiceScope = scope,
                User = eventArgs.Interaction.User
            };

            if (eventArgs.Interaction.Type == InteractionType.AutoComplete)
            {
                AutoCompleteContext? autoCompleteContext = await ParseAutoCompletesAsync(converterContext, eventArgs);
                if (autoCompleteContext is not null)
                {
                    IEnumerable<DiscordAutoCompleteChoice> choices = await converterContext.Argument.Attributes.OfType<SlashAutoCompleteProviderAttribute>().First().AutoCompleteAsync(autoCompleteContext);
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
                }

                await converterContext.ServiceScope.DisposeAsync();
            }
            else
            {
                CommandContext? commandContext = await ParseArgumentsAsync(converterContext, eventArgs);
                if (commandContext is null)
                {
                    await converterContext.ServiceScope.DisposeAsync();
                    return;
                }

                await _extension.CommandExecutor.ExecuteAsync(commandContext);
            }
        }

        public bool TryFindCommand(DiscordInteraction interaction, [NotNullWhen(true)] out Command? command, [NotNullWhen(true)] out IEnumerable<DiscordInteractionDataOption>? options)
        {
            if (!Commands.TryGetValue(interaction.Data.Id, out command))
            {
                options = null;
                return false;
            }

            // Resolve subcommands, which do not have id's.
            options = interaction.Data.Options ?? [];
            while (options.Any())
            {
                DiscordInteractionDataOption option = options.First();
                if (option.Type is not ApplicationCommandOptionType.SubCommandGroup and not ApplicationCommandOptionType.SubCommand)
                {
                    break;
                }

                command = command.Subcommands.First(x => x.Name == option.Name);
                options = option.Options ?? [];
            }

            return true;
        }

        public async Task RegisterSlashCommandsAsync(CommandAllExtension extension)
        {
            List<DiscordApplicationCommand> applicationCommands = [];
            applicationCommands.AddRange(_applicationCommands);
            foreach (Command command in extension.Commands.Values)
            {
                // If there is a SlashCommandTypesAttribute, check if it contains SlashCommandTypes.ApplicationCommand
                // If there isn't, default to SlashCommands
                if (command.Attributes.OfType<SlashCommandTypesAttribute>().FirstOrDefault() is SlashCommandTypesAttribute slashCommandTypesAttribute && !slashCommandTypesAttribute.ApplicationCommandTypes.Contains(ApplicationCommandType.SlashCommand))
                {
                    continue;
                }

                applicationCommands.Add(await ToApplicationCommandAsync(command));
            }

            IReadOnlyList<DiscordApplicationCommand> commands = extension.DebugGuildId is null
                ? await extension.Client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommands)
                : await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId.Value, applicationCommands);

            Dictionary<ulong, Command> commandsDictionary = [];
            foreach (DiscordApplicationCommand command in commands)
            {
                if (!extension.Commands.TryGetValue(command.Name, out Command? caCommand))
                {
                    foreach (Command officialCommand in extension.Commands.Values)
                    {
                        if (officialCommand.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName == command.Name)
                        {
                            caCommand = officialCommand;
                            break;
                        }
                    }
                }

                if (caCommand is null)
                {
                    SlashLogging.UnknownCommandName(_logger, command.Name, null);
                    continue;
                }

                commandsDictionary.Add(command.Id, caCommand);
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
                AsyncServiceScope scope = _extension.ServiceProvider.CreateAsyncScope();
                nameLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.name");
                descriptionLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.description");
                await scope.DisposeAsync();
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
                defaultMemberPermissions: command.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? Permissions.None,
                nsfw: command.Attributes.Any(x => x is RequireNsfwAttribute)
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
                AsyncServiceScope scope = _extension.ServiceProvider.CreateAsyncScope();
                nameLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.name");
                descriptionLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.description");
                await scope.DisposeAsync();
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
            AsyncServiceScope scope = _extension.ServiceProvider.CreateAsyncScope();

            // Translate the argument's name and description.
            IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
            IReadOnlyDictionary<string, string> descriptionLocalizations = new Dictionary<string, string>();
            if (argument.Attributes.OfType<SlashLocalizerAttribute>().FirstOrDefault() is SlashLocalizerAttribute localizerAttribute)
            {
                nameLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.arguments.{argument.Name}.name");
                descriptionLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.arguments.{argument.Name}.description");
            }

            IEnumerable<DiscordApplicationCommandOptionChoice> choices = [];
            if (argument.Attributes.OfType<SlashChoiceProviderAttribute>().FirstOrDefault() is SlashChoiceProviderAttribute choiceAttribute)
            {
                choices = await choiceAttribute.GrabChoicesAsync(scope.ServiceProvider, argument);
            }

            await scope.DisposeAsync();
            return new(
                name: argument.Name,
                description: argument.Description,
                name_localizations: nameLocalizations,
                description_localizations: descriptionLocalizations,
                autocomplete: argument.Attributes.Any(x => x is SlashAutoCompleteProviderAttribute),
                channelTypes: argument.Attributes.OfType<SlashChannelTypesAttribute>().FirstOrDefault()?.ChannelTypes ?? [],
                choices: choices,
                maxLength: minMaxLength?.MaxLength,
                maxValue: minMaxValue?.MaxValue!, // Incorrect nullable annotations within the lib
                minLength: minMaxLength?.MinLength,
                minValue: minMaxValue?.MinValue!, // Incorrect nullable annotations within the lib
                required: !argument.DefaultValue.HasValue,
                type: type
            );
        }

        private async Task<CommandContext?> ParseArgumentsAsync(SlashConverterContext converterContext, InteractionCreateEventArgs eventArgs)
        {
            if (_extension is null)
            {
                return null;
            }

            Dictionary<CommandArgument, object?> parsedArguments = [];
            try
            {
                while (converterContext.NextArgument())
                {
                    IOptional optional = await Converters[converterContext.Argument.Type](converterContext, eventArgs);
                    if (!optional.HasValue)
                    {
                        break;
                    }

                    parsedArguments.Add(converterContext.Argument, optional.RawValue);
                }
            }
            catch (Exception error)
            {
                await _extension._commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                {
                    Context = new SlashContext()
                    {
                        Arguments = parsedArguments,
                        Channel = eventArgs.Interaction.Channel,
                        Command = converterContext.Command,
                        Extension = converterContext.Extension,
                        Interaction = eventArgs.Interaction,
                        Options = converterContext.Options,
                        ServiceScope = converterContext.ServiceScope,
                        User = eventArgs.Interaction.User
                    },
                    Exception = new ParseArgumentException(converterContext.Argument, error),
                    CommandObject = null
                });

                return null;
            }

            return new SlashContext()
            {
                Arguments = parsedArguments,
                Channel = eventArgs.Interaction.Channel,
                Command = converterContext.Command,
                Extension = converterContext.Extension,
                Interaction = eventArgs.Interaction,
                Options = converterContext.Options,
                ServiceScope = converterContext.ServiceScope,
                User = eventArgs.Interaction.User
            };
        }

        private async Task<AutoCompleteContext?> ParseAutoCompletesAsync(SlashConverterContext converterContext, InteractionCreateEventArgs eventArgs)
        {
            if (_extension is null)
            {
                return null;
            }

            Dictionary<CommandArgument, object?> parsedArguments = [];
            try
            {
                while (converterContext.NextArgument() && !converterContext.Options.ElementAt(converterContext.ArgumentIndex).Focused)
                {
                    IOptional optional = await Converters[converterContext.Argument.Type](converterContext, eventArgs);
                    if (!optional.HasValue)
                    {
                        break;
                    }

                    parsedArguments.Add(converterContext.Argument, optional.RawValue);
                }
            }
            catch (Exception error)
            {
                await _extension._commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                {
                    Context = new SlashContext()
                    {
                        Arguments = parsedArguments,
                        Channel = eventArgs.Interaction.Channel,
                        Command = converterContext.Command,
                        Extension = converterContext.Extension,
                        Interaction = eventArgs.Interaction,
                        Options = converterContext.Options,
                        ServiceScope = converterContext.ServiceScope,
                        User = eventArgs.Interaction.User
                    },
                    Exception = new ParseArgumentException(converterContext.Argument, error),
                    CommandObject = null
                });

                return null;
            }

            return new AutoCompleteContext()
            {
                Arguments = parsedArguments,
                AutoCompleteArgument = converterContext.Argument,
                Channel = eventArgs.Interaction.Channel,
                Command = converterContext.Command,
                Extension = converterContext.Extension,
                Interaction = eventArgs.Interaction,
                Options = converterContext.Options,
                ServiceScope = converterContext.ServiceScope,
                User = eventArgs.Interaction.User,
                UserInput = converterContext.Options.ElementAt(converterContext.ArgumentIndex).Value
            };
        }

        private static async Task<IOptional> ExecuteConvertAsync<T>(ISlashArgumentConverter<T> converter, ConverterContext context, InteractionCreateEventArgs eventArgs) => await converter.ConvertAsync(context, eventArgs);
    }
}
