namespace DSharpPlus.Commands.Processors.UserCommands;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Attributes;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class UserCommandProcessor : ICommandProcessor<InteractionCreateEventArgs>
{
    public IReadOnlyDictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> Converters => this._slashCommandProcessor?.ConverterDelegates ?? new Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>>();
    private CommandsExtension? _extension;
    private SlashCommandProcessor? _slashCommandProcessor;

    public async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        if (this._extension is null)
        {
            extension.Client.ContextMenuInteractionCreated += this.ExecuteInteractionAsync;
        }

        this._extension = extension;
        this._slashCommandProcessor = this._extension.GetProcessor<SlashCommandProcessor>() ?? new SlashCommandProcessor();
        await this._slashCommandProcessor.ConfigureAsync(this._extension);

        ILogger<UserCommandProcessor> logger = this._extension.ServiceProvider.GetService<ILogger<UserCommandProcessor>>() ?? NullLogger<UserCommandProcessor>.Instance;
        List<DiscordApplicationCommand> applicationCommands = [];
        foreach (Command command in this._extension.Commands.Values)
        {
            // User commands must be explicitly defined as such, otherwise they are ignored.
            if (!command.Attributes.Any(x => x is SlashCommandTypesAttribute slashCommandTypesAttribute && slashCommandTypesAttribute.ApplicationCommandTypes.Contains(DiscordApplicationCommandType.UserContextMenu)))
            {
                continue;
            }
            // Ensure there are no subcommands.
            else if (command.Subcommands.Count != 0)
            {
                logger.LogError("User command '{CommandName}' cannot have subcommands.", command.Name);
                continue;
            }
            // Check to see if the method signature is valid.
            else if (command.Parameters[0].Type != typeof(DiscordUser) && command.Parameters[0].Type != typeof(DiscordMember))
            {
                logger.LogError("User command '{CommandName}' must have a single parameter of type DiscordUser or DiscordMember.", command.Name);
                continue;
            }
            else if (!command.Parameters.Skip(1).All(parameter => parameter.DefaultValue.HasValue))
            {
                logger.LogError("User command '{CommandName}' must have all parameters after the first contain a default value.", command.Name);
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
        else if (eventArgs.Interaction.Type is not DiscordInteractionType.ApplicationCommand || eventArgs.Interaction.Data.Type is not DiscordApplicationCommandType.UserContextMenu)
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

        Dictionary<CommandParameter, object?> arguments = new()
        {
            { command.Parameters[0], eventArgs.TargetUser }
        };

        for (int i = 1; i < command.Parameters.Count; i++)
        {
            // We verify at startup that all parameters have default values.
            arguments.Add(command.Parameters[i], command.Parameters[i].DefaultValue.Value);
        }

        SlashCommandContext commandContext = new()
        {
            Arguments = arguments,
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

    public async Task<DiscordApplicationCommand> ToApplicationCommandAsync(CommandsExtension extension, Command command)
    {
        IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
        if (command.Attributes.OfType<InteractionLocalizerAttribute>().FirstOrDefault() is InteractionLocalizerAttribute localizerAttribute)
        {
            nameLocalizations = await this._slashCommandProcessor!.ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.name");
        }

        return new(
            name: command.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName ?? command.Name,
            description: string.Empty,
            type: DiscordApplicationCommandType.UserContextMenu,
            name_localizations: nameLocalizations,
            allowDMUsage: command.Attributes.Any(x => x is AllowDMUsageAttribute),
            defaultMemberPermissions: command.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? DiscordPermissions.None,
            nsfw: command.Attributes.Any(x => x is RequireNsfwAttribute),
            contexts: command.Attributes.OfType<InteractionAllowedContextsAttribute>().FirstOrDefault()?.AllowedContexts,
            integrationTypes: command.Attributes.OfType<InteractionInstallTypeAttribute>().FirstOrDefault()?.InstallTypes
        );
    }
}
