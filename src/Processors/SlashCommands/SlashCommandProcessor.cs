using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.ContextChecks;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.Exceptions;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public sealed class SlashCommandProcessor : BaseCommandProcessor<InteractionCreateEventArgs, ISlashArgumentConverter, SlashConverterContext, SlashCommandContext>
    {
        public IReadOnlyDictionary<Type, ApplicationCommandOptionType> TypeMappings { get; private set; } = new Dictionary<Type, ApplicationCommandOptionType>();
        public IReadOnlyDictionary<ulong, Command> Commands { get; private set; } = new Dictionary<ulong, Command>();

        private readonly List<DiscordApplicationCommand> _applicationCommands = [];
        private bool _configured;

        public override async Task ConfigureAsync(CommandAllExtension extension)
        {
            await base.ConfigureAsync(extension);

            Dictionary<Type, ApplicationCommandOptionType> typeMappings = [];
            foreach (LazyConverter lazyConverter in _lazyConverters.Values)
            {
                ISlashArgumentConverter converter = lazyConverter.GetConverter(_extension.ServiceProvider);
                typeMappings.Add(lazyConverter.ArgumentType, converter.ArgumentType);
            }

            TypeMappings = typeMappings.ToFrozenDictionary();
            if (!_configured)
            {
                _configured = true;
                extension.Client.GuildDownloadCompleted += async (client, eventArgs) => await RegisterSlashCommandsAsync(extension);
                extension.Client.InteractionCreated += ExecuteInteractionAsync;
            }
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
                    Context = new SlashCommandContext()
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
                AutoCompleteContext? autoCompleteContext = await ParseAutoCompleteArgumentsAsync(converterContext, eventArgs);
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

        public void AddApplicationCommands(params DiscordApplicationCommand[] applicationCommands) => _applicationCommands.AddRange(applicationCommands);
        public void AddApplicationCommands(IEnumerable<DiscordApplicationCommand> applicationCommands) => _applicationCommands.AddRange(applicationCommands);

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

        [SuppressMessage("Roslyn", "IDE0045", Justification = "Ternary rabbit hole.")]
        public async Task<DiscordApplicationCommandOption> ToApplicationArgumentAsync(Command command, CommandArgument argument)
        {
            if (_extension is null)
            {
                throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
            }

            if (!TypeMappings.TryGetValue(GetConverterFriendlyBaseType(argument.Type), out ApplicationCommandOptionType type))
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

        private async Task<AutoCompleteContext?> ParseAutoCompleteArgumentsAsync(SlashConverterContext converterContext, InteractionCreateEventArgs eventArgs)
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
                    IOptional optional = await ConverterDelegates[GetConverterFriendlyBaseType(converterContext.Argument.Type)](converterContext, eventArgs);
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
                    Context = new SlashCommandContext()
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

        public override SlashCommandContext CreateCommandContext(SlashConverterContext converterContext, InteractionCreateEventArgs eventArgs, Dictionary<CommandArgument, object?> parsedArguments) => new()
        {
            Arguments = parsedArguments,
            Channel = eventArgs.Interaction.Channel,
            Command = converterContext.Command,
            Extension = _extension ?? throw new InvalidOperationException("SlashCommandProcessor has not been configured."),
            Interaction = eventArgs.Interaction,
            Options = converterContext.Options,
            ServiceScope = converterContext.ServiceScope,
            User = eventArgs.Interaction.User
        };
    }
}
