using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Converters.Results;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public sealed partial class SlashCommandProcessor : BaseCommandProcessor<ISlashArgumentConverter, InteractionConverterContext, SlashCommandContext>
{
    // Required for GuildDownloadCompleted event
    public const DiscordIntents RequiredIntents = DiscordIntents.Guilds;

    /// <inheritdoc/>
    public override IReadOnlyList<Command> Commands => applicationCommandMapping.Values;

    /// <summary>
    /// The configuration values being used for this processor.
    /// </summary>
    public SlashCommandConfiguration Configuration { get; init; }

    /// <summary>
    /// Whether the application commands have been registered.
    /// </summary>
    private bool isApplicationCommandsRegistered;

    /// <summary>
    /// Creates a new instance of <see cref="SlashCommandProcessor"/>.
    /// </summary>
    public SlashCommandProcessor() : this(new()) { }

    /// <summary>
    /// Creates a new instance of <see cref="SlashCommandProcessor"/>.
    /// </summary>
    /// <param name="configuration">The configuration values to use for this processor.</param>
    public SlashCommandProcessor(SlashCommandConfiguration configuration) => this.Configuration = configuration;

    /// <inheritdoc />
    public override async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        // Find all converters and prepare other helpful data.
        await base.ConfigureAsync(extension);

        // Register the commands if the user wants to.
        if (!this.isApplicationCommandsRegistered && this.Configuration.RegisterCommands)
        {
            await RegisterSlashCommandsAsync(extension);
        }
    }

    public async Task ExecuteInteractionAsync(DiscordClient client, InteractionCreatedEventArgs eventArgs)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }
        else if (eventArgs.Interaction.Type is not DiscordInteractionType.ApplicationCommand and not DiscordInteractionType.AutoComplete || eventArgs.Interaction.Data.Type is not DiscordApplicationCommandType.SlashCommand)
        {
            return;
        }
        else if (this.ConverterDelegates.Count == 0)
        {
            SlashLogging.interactionReceivedBeforeConfigured(this.logger, null);
            return;
        }

        AsyncServiceScope serviceScope = this.extension.ServiceProvider.CreateAsyncScope();
        if (!TryFindCommand(eventArgs.Interaction, out Command? command, out IReadOnlyList<DiscordInteractionDataOption>? options))
        {
            await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
            {
                Context = new SlashCommandContext()
                {
                    Arguments = new Dictionary<CommandParameter, object?>(),
                    Channel = eventArgs.Interaction.Channel,
                    Command = null!,
                    Extension = this.extension,
                    ServiceScope = serviceScope,
                    User = eventArgs.Interaction.User,
                    Interaction = eventArgs.Interaction,
                    Options = eventArgs.Interaction.Data.Options ?? []
                },
                CommandObject = null,
                Exception = new CommandNotFoundException(eventArgs.Interaction.Data.Name)
            });

            await serviceScope.DisposeAsync();
            return;
        }

        InteractionConverterContext converterContext = new()
        {
            Channel = eventArgs.Interaction.Channel,
            Command = command,
            Extension = this.extension,
            Interaction = eventArgs.Interaction,
            Options = options,
            ParameterNamePolicy = this.Configuration.ParameterNamePolicy,
            ServiceScope = serviceScope,
            User = eventArgs.Interaction.User
        };

        if (eventArgs.Interaction.Type is DiscordInteractionType.AutoComplete)
        {
            AutoCompleteContext? autoCompleteContext = await ParseAutoCompleteArgumentsAsync(converterContext);
            if (autoCompleteContext is not null)
            {
                foreach (Attribute attribute in autoCompleteContext.Parameter.Attributes)
                {
                    if (attribute is SlashAutoCompleteProviderAttribute autoCompleteProviderAttribute)
                    {
                        await eventArgs.Interaction.CreateResponseAsync(
                            DiscordInteractionResponseType.AutoCompleteResult,
                            new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(await autoCompleteProviderAttribute.AutoCompleteAsync(autoCompleteContext))
                        );

                        break;
                    }
                }
            }

            converterContext.ServiceScope.Dispose();
            return;
        }

        IReadOnlyDictionary<CommandParameter, object?> parsedArguments = await ParseParametersAsync(converterContext);
        SlashCommandContext commandContext = CreateCommandContext(converterContext, parsedArguments);

        // Iterate over all arguments and check if any of them failed to parse.
        foreach (KeyValuePair<CommandParameter, object?> argument in parsedArguments)
        {
            if (argument.Value is ArgumentFailedConversionResult argumentFailedConversionResult)
            {
                await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
                {
                    Context = commandContext,
                    CommandObject = null,
                    Exception = new ArgumentParseException(argument.Key, argumentFailedConversionResult.Error)
                });

                await serviceScope.DisposeAsync();
                return;
            }
            else if (argument.Value is ArgumentNotParsedResult)
            {
                await this.extension.commandErrored.InvokeAsync(this.extension, new CommandErroredEventArgs()
                {
                    Context = commandContext,
                    CommandObject = null,
                    Exception = new ArgumentParseException(argument.Key, new ArgumentException("Argument could not be parsed."))
                });

                await serviceScope.DisposeAsync();
                return;
            }
        }

        await this.extension.CommandExecutor.ExecuteAsync(commandContext);
    }

    public bool TryFindCommand(DiscordInteraction interaction, [NotNullWhen(true)] out Command? command, [NotNullWhen(true)] out IReadOnlyList<DiscordInteractionDataOption>? options)
    {
        if (!applicationCommandMapping.TryGetValue(interaction.Data.Id, out command))
        {
            options = null;
            return false;
        }

        // Resolve subcommands, which do not have subcommand id's.
        options = interaction.Data.Options ?? [];
        while (options.Any())
        {
            DiscordInteractionDataOption option = options[0];
            if (option.Type is not DiscordApplicationCommandOptionType.SubCommandGroup and not DiscordApplicationCommandOptionType.SubCommand)
            {
                break;
            }

            command = command.Subcommands.First(subcommandName => this.Configuration.ParameterNamePolicy.GetCommandName(subcommandName) == option.Name);
            options = option.Options ?? [];
        }

        return true;
    }

    public async ValueTask ClearDiscordSlashCommandsAsync(bool clearGuildCommands = false)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        await this.extension.Client.BulkOverwriteGlobalApplicationCommandsAsync(new List<DiscordApplicationCommand>());
        if (!clearGuildCommands)
        {
            return;
        }

        foreach (ulong guildId in this.extension.Client.Guilds.Keys)
        {
            await this.extension.Client.BulkOverwriteGuildApplicationCommandsAsync(guildId, new List<DiscordApplicationCommand>());
        }
    }

    private async ValueTask<AutoCompleteContext?> ParseAutoCompleteArgumentsAsync(InteractionConverterContext converterContext)
    {
        if (this.extension is null)
        {
            return null;
        }

        Dictionary<CommandParameter, object?> parsedArguments = [];
        CommandParameter? autoCompleteParameter = null;
        DiscordInteractionDataOption? autoCompleteOption = null;
        try
        {
            // Parse until we find the parameter that the user is currently typing
            while (converterContext.NextParameter())
            {
                DiscordInteractionDataOption? option = converterContext.Options.FirstOrDefault(discordOption =>
                    discordOption.Name.Equals(converterContext
                        .Extension.GetProcessor<SlashCommandProcessor>().Configuration
                            .ParameterNamePolicy.GetParameterName(converterContext.Parameter, -1), StringComparison.OrdinalIgnoreCase));

                if (option is null)
                {
                    continue;
                }
                else if (option.Focused)
                {
                    autoCompleteParameter = converterContext.Parameter;
                    autoCompleteOption = option;
                    break;
                }

                IOptional optional = await this.ConverterDelegates[IArgumentConverter.GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext);
                parsedArguments.Add(converterContext.Parameter, optional.HasValue
                    ? optional.RawValue
                    : converterContext.Parameter.DefaultValue
                );
            }

            if (autoCompleteParameter is null || autoCompleteOption is null)
            {
                this.logger.LogWarning("Cannot find the auto complete parameter that the user is currently typing - this should be reported to library developers.");
                return null;
            }
        }
        catch (Exception error)
        {
            await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
            {
                Context = new SlashCommandContext()
                {
                    Arguments = parsedArguments,
                    Channel = converterContext.Interaction.Channel,
                    Command = converterContext.Command,
                    Extension = converterContext.Extension,
                    Interaction = converterContext.Interaction,
                    Options = converterContext.Options,
                    ServiceScope = converterContext.ServiceScope,
                    User = converterContext.Interaction.User
                },
                Exception = new ArgumentParseException(converterContext.Parameter, error),
                CommandObject = null
            });

            return null;
        }

        return new AutoCompleteContext()
        {
            Arguments = parsedArguments,
            Parameter = autoCompleteParameter,
            Channel = converterContext.Interaction.Channel,
            Command = converterContext.Command,
            Extension = converterContext.Extension,
            Interaction = converterContext.Interaction,
            Options = converterContext.Options,
            ServiceScope = converterContext.ServiceScope,
            User = converterContext.Interaction.User,
            UserInput = autoCompleteOption.RawValue
        };
    }

    public override SlashCommandContext CreateCommandContext(InteractionConverterContext converterContext, IReadOnlyDictionary<CommandParameter, object?> parsedArguments)
    {
        return new()
        {
            Arguments = parsedArguments,
            Channel = converterContext.Interaction.Channel,
            Command = converterContext.Command,
            Extension = this.extension ?? throw new InvalidOperationException("SlashCommandProcessor has not been configured."),
            Interaction = converterContext.Interaction,
            Options = converterContext.Options,
            ServiceScope = converterContext.ServiceScope,
            User = converterContext.Interaction.User
        };
    }
}
