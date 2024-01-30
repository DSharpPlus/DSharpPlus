using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.ContextChecks;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.Exceptions;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.CommandAll.Processors.MessageCommands
{
    public sealed class MessageCommandProcessor : ICommandProcessor<InteractionCreateEventArgs>
    {
        public IReadOnlyDictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> Converters => _slashCommandProcessor?.ConverterDelegates ?? new Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>>();
        private CommandAllExtension? _extension;
        private SlashCommandProcessor? _slashCommandProcessor;

        public async Task ConfigureAsync(CommandAllExtension extension)
        {
            _extension = extension;
            _extension.Client.ContextMenuInteractionCreated += ExecuteInteractionAsync;
            _slashCommandProcessor = _extension.GetProcessor<SlashCommandProcessor>() ?? new SlashCommandProcessor();
            await _slashCommandProcessor.ConfigureAsync(_extension);

            ILogger<MessageCommandProcessor> logger = _extension.ServiceProvider.GetService<ILogger<MessageCommandProcessor>>() ?? NullLogger<MessageCommandProcessor>.Instance;
            List<DiscordApplicationCommand> applicationCommands = [];
            foreach (Command command in _extension.Commands.Values)
            {
                // Message commands must be explicitly defined as such, otherwise they are ignored.
                if (!command.Attributes.Any(x => x is SlashCommandTypesAttribute slashCommandTypesAttribute && slashCommandTypesAttribute.ApplicationCommandTypes.Contains(ApplicationCommandType.MessageContextMenu)))
                {
                    continue;
                }
                // Ensure there are no subcommands.
                else if (command.Subcommands.Count != 0)
                {
                    logger.LogError("Message command '{CommandName}' cannot have subcommands.", command.Name);
                    continue;
                }
                // Check to see if the method signature is valid.
                else if (command.Parameters.Count != 1 || command.Parameters[0].Type != typeof(DiscordMessage))
                {
                    logger.LogError("Message command '{CommandName}' must have a single parameter of type DiscordMessage.", command.Name);
                    continue;
                }

                applicationCommands.Add(await ToApplicationCommandAsync(_extension, command));
            }

            _slashCommandProcessor.AddApplicationCommands(applicationCommands);
        }

        public async Task ExecuteInteractionAsync(DiscordClient client, ContextMenuInteractionCreateEventArgs eventArgs)
        {
            if (_extension is null || _slashCommandProcessor is null)
            {
                throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
            }
            else if (eventArgs.Interaction.Type is not InteractionType.ApplicationCommand || eventArgs.Interaction.Data.Type is not ApplicationCommandType.MessageContextMenu)
            {
                return;
            }

            AsyncServiceScope scope = _extension.ServiceProvider.CreateAsyncScope();
            if (!_slashCommandProcessor.TryFindCommand(eventArgs.Interaction, out Command? command, out IEnumerable<DiscordInteractionDataOption>? options))
            {
                await _extension._commandErrored.InvokeAsync(_extension, new CommandErroredEventArgs()
                {
                    Context = new SlashCommandContext()
                    {
                        Arguments = new Dictionary<CommandParameter, object?>(),
                        Channel = eventArgs.Interaction.Channel,
                        Command = null!,
                        Extension = _extension,
                        Interaction = eventArgs.Interaction,
                        Options = eventArgs.Interaction.Data.Options ?? [],
                        ServiceScope = scope,
                        User = eventArgs.Interaction.User
                    },
                    CommandObject = null,
                    Exception = new CommandNotFoundException(eventArgs.Interaction.Data.Name),
                });

                await scope.DisposeAsync();
                return;
            }

            SlashCommandContext context = new()
            {
                Arguments = new Dictionary<CommandParameter, object?>()
                {
                    [command.Parameters[0]] = eventArgs.TargetMessage
                },
                Channel = eventArgs.Interaction.Channel,
                Command = command,
                Extension = _extension,
                Interaction = eventArgs.Interaction,
                Options = [],
                ServiceScope = scope,
                User = eventArgs.Interaction.User
            };

            await _extension.CommandExecutor.ExecuteAsync(context);
        }

        public static async Task<DiscordApplicationCommand> ToApplicationCommandAsync(CommandAllExtension extension, Command command)
        {
            IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
            if (command.Attributes.OfType<SlashLocalizerAttribute>().FirstOrDefault() is SlashLocalizerAttribute localizerAttribute)
            {
                AsyncServiceScope scope = extension.ServiceProvider.CreateAsyncScope();
                nameLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.name");
                await scope.DisposeAsync();
            }

            return new(
                name: command.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName ?? command.Name,
                description: string.Empty,
                type: ApplicationCommandType.MessageContextMenu,
                name_localizations: nameLocalizations,
                allowDMUsage: command.Attributes.Any(x => x is AllowDMUsageAttribute),
                defaultMemberPermissions: command.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? Permissions.None,
                nsfw: command.Attributes.Any(x => x is RequireNsfwAttribute)
            );
        }
    }
}
