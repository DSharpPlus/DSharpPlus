namespace DSharpPlus.Commands.Processors.MessageCommands;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Commands;
using DSharpPlus.Commands.Commands.Attributes;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class MessageCommandProcessor : ICommandProcessor<InteractionCreateEventArgs>
{
    public IReadOnlyDictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> Converters => this._slashCommandProcessor?.ConverterDelegates ?? new Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>>();
    private CommandsExtension? _extension;
    private SlashCommandProcessor? _slashCommandProcessor;

    public async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        this._extension = extension;
        this._extension.Client.ContextMenuInteractionCreated += this.ExecuteInteractionAsync;
        this._slashCommandProcessor = this._extension.GetProcessor<SlashCommandProcessor>() ?? new SlashCommandProcessor();
        await this._slashCommandProcessor.ConfigureAsync(this._extension);

        ILogger<MessageCommandProcessor> logger = this._extension.ServiceProvider.GetService<ILogger<MessageCommandProcessor>>() ?? NullLogger<MessageCommandProcessor>.Instance;
        List<DiscordApplicationCommand> applicationCommands = [];
        foreach (Command command in this._extension.Commands.Values)
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

            applicationCommands.Add(await ToApplicationCommandAsync(this._extension, command));
        }

        this._slashCommandProcessor.AddApplicationCommands(applicationCommands);
    }

    public async Task ExecuteInteractionAsync(DiscordClient client, ContextMenuInteractionCreateEventArgs eventArgs)
    {
        if (this._extension is null || this._slashCommandProcessor is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }
        else if (eventArgs.Interaction.Type is not InteractionType.ApplicationCommand || eventArgs.Interaction.Data.Type is not ApplicationCommandType.MessageContextMenu)
        {
            return;
        }

        AsyncServiceScope scope = this._extension.ServiceProvider.CreateAsyncScope();
        if (!this._slashCommandProcessor.TryFindCommand(eventArgs.Interaction, out Command? command, out IEnumerable<DiscordInteractionDataOption>? options))
        {
            await this._extension._commandErrored.InvokeAsync(this._extension, new CommandErroredEventArgs()
            {
                Context = new SlashCommandContext()
                {
                    Arguments = new Dictionary<CommandParameter, object?>(),
                    Channel = eventArgs.Interaction.Channel,
                    Command = null!,
                    Extension = this._extension,
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

        SlashCommandContext commandContext = new()
        {
            Arguments = new Dictionary<CommandParameter, object?>()
            {
                [command.Parameters[0]] = eventArgs.TargetMessage
            },
            Channel = eventArgs.Interaction.Channel,
            Command = command,
            Extension = this._extension,
            Interaction = eventArgs.Interaction,
            Options = [],
            ServiceScope = scope,
            User = eventArgs.Interaction.User
        };

        await this._extension.CommandExecutor.ExecuteAsync(commandContext);
    }

    public static async Task<DiscordApplicationCommand> ToApplicationCommandAsync(CommandsExtension extension, Command command)
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
