namespace DSharpPlus.CommandAll.Processors.UserCommands;
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

public sealed class UserCommandProcessor : ICommandProcessor<InteractionCreateEventArgs>
{
    public IReadOnlyDictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> Converters => this._slashCommandProcessor?.ConverterDelegates ?? new Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>>();
    private CommandAllExtension? _extension;
    private SlashCommandProcessor? _slashCommandProcessor;

    public async Task ConfigureAsync(CommandAllExtension extension)
    {
        this._extension = extension;
        this._extension.Client.ContextMenuInteractionCreated += this.ExecuteInteractionAsync;
        this._slashCommandProcessor = this._extension.GetProcessor<SlashCommandProcessor>() ?? new SlashCommandProcessor();
        await this._slashCommandProcessor.ConfigureAsync(this._extension);

        ILogger<UserCommandProcessor> logger = this._extension.ServiceProvider.GetService<ILogger<UserCommandProcessor>>() ?? NullLogger<UserCommandProcessor>.Instance;
        List<DiscordApplicationCommand> applicationCommands = [];
        foreach (Command command in this._extension.Commands.Values)
        {
            // User commands must be explicitly defined as such, otherwise they are ignored.
            if (!command.Attributes.Any(x => x is SlashCommandTypesAttribute slashCommandTypesAttribute && slashCommandTypesAttribute.ApplicationCommandTypes.Contains(ApplicationCommandType.UserContextMenu)))
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
            else if (command.Parameters.Count != 1 || (command.Parameters[0].Type != typeof(DiscordUser) && command.Parameters[0].Type != typeof(DiscordMember)))
            {
                logger.LogError("User command '{CommandName}' must have a single parameter of type DiscordUser or DiscordMember.", command.Name);
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
        else if (eventArgs.Interaction.Type is not InteractionType.ApplicationCommand || eventArgs.Interaction.Data.Type is not ApplicationCommandType.UserContextMenu)
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

        SlashCommandContext context = new()
        {
            Arguments = new Dictionary<CommandParameter, object?>()
            {
                [command.Parameters[0]] = eventArgs.TargetUser
            },
            Channel = eventArgs.Interaction.Channel,
            Command = command,
            Extension = this._extension,
            Interaction = eventArgs.Interaction,
            Options = [],
            ServiceScope = scope,
            User = eventArgs.Interaction.User
        };

        await this._extension.CommandExecutor.ExecuteAsync(context);
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
            type: ApplicationCommandType.UserContextMenu,
            name_localizations: nameLocalizations,
            allowDMUsage: command.Attributes.Any(x => x is AllowDMUsageAttribute),
            defaultMemberPermissions: command.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? Permissions.None,
            nsfw: command.Attributes.Any(x => x is RequireNsfwAttribute)
        );
    }
}
