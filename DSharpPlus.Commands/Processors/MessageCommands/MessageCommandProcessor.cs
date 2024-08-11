using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Commands.Processors.MessageCommands;

public sealed class MessageCommandProcessor : ICommandProcessor<InteractionCreatedEventArgs>
{
    public IReadOnlyDictionary<Type, ConverterDelegate<InteractionCreatedEventArgs>> Converters => this.slashCommandProcessor?.ConverterDelegates ?? new Dictionary<Type, ConverterDelegate<InteractionCreatedEventArgs>>();
    private CommandsExtension? extension;
    private SlashCommandProcessor? slashCommandProcessor;

    public IReadOnlyList<Command> Commands => this.commands;
    private List<Command> commands = [];

    /// <inheritdoc />
    public Type ContextType => typeof(SlashCommandContext);

    /// <inheritdoc />
    public async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        this.extension = extension;
        this.slashCommandProcessor = this.extension.GetProcessor<SlashCommandProcessor>() ?? new SlashCommandProcessor();

        ILogger<MessageCommandProcessor> logger = this.extension.ServiceProvider.GetService<ILogger<MessageCommandProcessor>>() ?? NullLogger<MessageCommandProcessor>.Instance;
        List<DiscordApplicationCommand> applicationCommands = [];

        IReadOnlyList<Command> commands = this.extension.GetCommandsForProcessor(this);
        IEnumerable<Command> flattenCommands = commands.SelectMany(x => x.Flatten());

        foreach (Command command in flattenCommands)
        {
            // Message commands must be explicitly defined as such, otherwise they are ignored.
            if (!command.Attributes.Any(x => x is SlashCommandTypesAttribute slashCommandTypesAttribute && slashCommandTypesAttribute.ApplicationCommandTypes.Contains(DiscordApplicationCommandType.MessageContextMenu)))
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
            else if (command.Parameters.Count < 1 || command.Parameters[0].Type != typeof(DiscordMessage))
            {
                logger.LogError("Message command '{CommandName}' must have the signature (CommandContext, DiscordMessage). Any additional parameter must have a default value.", command.Name);
                continue;
            }
            else if (!command.Parameters.Skip(1).All(parameter => parameter.DefaultValue.HasValue))
            {
                logger.LogError("Message command '{CommandName}' must have the signature (CommandContext, DiscordMessage). Any additional parameter must have a default value.", command.Name);
                continue;
            }
            else if (!command.Method!.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(SlashCommandContext)))
            {
                logger.LogError("Message command '{CommandName}' has an incompatible CommandContext.", command.Name);
                continue;
            }

            this.commands.Add(command);
            applicationCommands.Add(await ToApplicationCommandAsync(command));
        }

        this.slashCommandProcessor.AddApplicationCommands(applicationCommands);
    }

    public async Task ExecuteInteractionAsync(DiscordClient client, ContextMenuInteractionCreatedEventArgs eventArgs)
    {
        if (this.extension is null || this.slashCommandProcessor is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }
        else if (eventArgs.Interaction.Type is not DiscordInteractionType.ApplicationCommand || eventArgs.Interaction.Data.Type is not DiscordApplicationCommandType.MessageContextMenu)
        {
            return;
        }

        AsyncServiceScope scope = this.extension.ServiceProvider.CreateAsyncScope();

        if (this.slashCommandProcessor.ApplicationCommandMapping == FrozenDictionary<ulong, Command>.Empty)
        {
            ILogger<MessageCommandProcessor> logger = this.extension.ServiceProvider.GetService<ILogger<MessageCommandProcessor>>() ?? NullLogger<MessageCommandProcessor>.Instance;
            logger.LogWarning("Received an interaction for a user command, but commands have not been registered yet. Ignoring interaction");
        }

        if (!this.slashCommandProcessor.TryFindCommand(eventArgs.Interaction, out Command? command, out _))
        {
            await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
            {
                Context = new SlashCommandContext()
                {
                    Arguments = new Dictionary<CommandParameter, object?>(),
                    Channel = eventArgs.Interaction.Channel,
                    Command = null!,
                    Extension = this.extension,
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
            { command.Parameters[0], eventArgs.TargetMessage }
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
            Extension = this.extension,
            Interaction = eventArgs.Interaction,
            Options = [],
            ServiceScope = scope,
            User = eventArgs.Interaction.User
        };

        await this.extension.CommandExecutor.ExecuteAsync(commandContext);
    }

    public async Task<DiscordApplicationCommand> ToApplicationCommandAsync(Command command)
    {
        IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
        if (command.Attributes.OfType<InteractionLocalizerAttribute>().FirstOrDefault() is InteractionLocalizerAttribute localizerAttribute)
        {
            nameLocalizations = await this.slashCommandProcessor!.ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.name");
        }

        return new(
            name: command.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName ?? command.FullName,
            description: string.Empty,
            type: DiscordApplicationCommandType.MessageContextMenu,
            name_localizations: nameLocalizations,
            allowDMUsage: command.Attributes.Any(x => x is AllowDMUsageAttribute),
            defaultMemberPermissions: command.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? DiscordPermissions.None,
            nsfw: command.Attributes.Any(x => x is RequireNsfwAttribute),
            contexts: command.Attributes.OfType<InteractionAllowedContextsAttribute>().FirstOrDefault()?.AllowedContexts,
            integrationTypes: command.Attributes.OfType<InteractionInstallTypeAttribute>().FirstOrDefault()?.InstallTypes
        );
    }
}
