using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public sealed class SlashCommandProcessor : BaseCommandProcessor<InteractionCreateEventArgs, ISlashArgumentConverter, InteractionConverterContext, SlashCommandContext>
{
    // Required for GuildDownloadCompleted event
    public const DiscordIntents RequiredIntents = DiscordIntents.Guilds;

    public IReadOnlyDictionary<Type, DiscordApplicationCommandOptionType> TypeMappings { get; private set; } = new Dictionary<Type, DiscordApplicationCommandOptionType>();
    public IReadOnlyDictionary<ulong, Command> Commands { get; private set; } = new Dictionary<ulong, Command>();

    private readonly List<DiscordApplicationCommand> applicationCommands = [];
    private bool configured;

    public override async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        await base.ConfigureAsync(extension);

        Dictionary<Type, DiscordApplicationCommandOptionType> typeMappings = [];
        foreach (LazyConverter lazyConverter in this.lazyConverters.Values)
        {
            ISlashArgumentConverter converter = lazyConverter.GetConverter(this.extension.ServiceProvider);
            typeMappings.Add(lazyConverter.ParameterType, converter.ParameterType);
        }

        this.TypeMappings = typeMappings.ToFrozenDictionary();
        if (!this.configured)
        {
            this.configured = true;
            extension.Client.InteractionCreated += this.ExecuteInteractionAsync;
            extension.Client.GuildDownloadCompleted += async (client, eventArgs) =>
            {
                if (client.ShardId == 0)
                {
                    await this.RegisterSlashCommandsAsync(extension);
                }
            };
        }
    }

    public async Task ExecuteInteractionAsync(DiscordClient client, InteractionCreateEventArgs eventArgs)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }
        else if (eventArgs.Interaction.Type is not DiscordInteractionType.ApplicationCommand and not DiscordInteractionType.AutoComplete || eventArgs.Interaction.Data.Type is not DiscordApplicationCommandType.SlashCommand)
        {
            return;
        }

        AsyncServiceScope serviceScope = this.extension.ServiceProvider.CreateAsyncScope();
        if (!this.TryFindCommand(eventArgs.Interaction, out Command? command, out IEnumerable<DiscordInteractionDataOption>? options))
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
            Options = options.ToList(),
            ServiceScope = serviceScope,
            User = eventArgs.Interaction.User
        };

        if (eventArgs.Interaction.Type is DiscordInteractionType.AutoComplete)
        {
            AutoCompleteContext? autoCompleteContext = await this.ParseAutoCompleteArgumentsAsync(converterContext, eventArgs);
            if (autoCompleteContext is not null)
            {
                IEnumerable<DiscordAutoCompleteChoice> choices = await autoCompleteContext.AutoCompleteArgument.Attributes.OfType<SlashAutoCompleteProviderAttribute>().First().AutoCompleteAsync(autoCompleteContext);
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
            }

            converterContext.ServiceScope.Dispose();
        }
        else
        {
            CommandContext? commandContext = await this.ParseArgumentsAsync(converterContext, eventArgs);
            if (commandContext is null)
            {
                converterContext.ServiceScope.Dispose();
                return;
            }

            await this.extension.CommandExecutor.ExecuteAsync(commandContext);
        }
    }

    public bool TryFindCommand(DiscordInteraction interaction, [NotNullWhen(true)] out Command? command, [NotNullWhen(true)] out IEnumerable<DiscordInteractionDataOption>? options)
    {
        if (!this.Commands.TryGetValue(interaction.Data.Id, out command))
        {
            options = null;
            return false;
        }

        // Resolve subcommands, which do not have id's.
        options = interaction.Data.Options ?? [];
        while (options.Any())
        {
            DiscordInteractionDataOption option = options.First();
            if (option.Type is not DiscordApplicationCommandOptionType.SubCommandGroup and not DiscordApplicationCommandOptionType.SubCommand)
            {
                break;
            }

            command = command.Subcommands.First(x => ToSnakeCase(x.Name) == option.Name);
            options = option.Options ?? [];
        }

        return true;
    }

    public void AddApplicationCommands(params DiscordApplicationCommand[] applicationCommands) => this.applicationCommands.AddRange(applicationCommands);
    public void AddApplicationCommands(IEnumerable<DiscordApplicationCommand> applicationCommands) => this.applicationCommands.AddRange(applicationCommands);

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

    public async Task RegisterSlashCommandsAsync(CommandsExtension extension)
    {
        List<DiscordApplicationCommand> globalApplicationCommands = [];
        Dictionary<ulong, List<DiscordApplicationCommand>> guildsApplicationCommands = [];
        globalApplicationCommands.AddRange(this.applicationCommands);
        foreach (Command command in extension.Commands.Values)
        {
            // If there is a SlashCommandTypesAttribute, check if it contains SlashCommandTypes.ApplicationCommand
            // If there isn't, default to SlashCommands
            if (command.Attributes.OfType<SlashCommandTypesAttribute>().FirstOrDefault() is SlashCommandTypesAttribute slashCommandTypesAttribute &&
                !slashCommandTypesAttribute.ApplicationCommandTypes.Contains(DiscordApplicationCommandType.SlashCommand))
            {
                continue;
            }

            DiscordApplicationCommand applicationCommand = await this.ToApplicationCommandAsync(command);

            if (command.GuildIds.Count == 0)
            {
                globalApplicationCommands.Add(applicationCommand);
                continue;
            }

            foreach (ulong guildId in command.GuildIds)
            {
                if (!guildsApplicationCommands.TryGetValue(guildId, out List<DiscordApplicationCommand>? guildCommands))
                {
                    guildCommands = [];
                    guildsApplicationCommands.Add(guildId, guildCommands);
                }

                guildCommands.Add(applicationCommand);
            }
        }

        List<DiscordApplicationCommand> discordCommands = [];
        if (extension.DebugGuildId == 0)
        {
            discordCommands.AddRange(await extension.Client.BulkOverwriteGlobalApplicationCommandsAsync(globalApplicationCommands));

            foreach ((ulong guildId, List<DiscordApplicationCommand> guildCommands) in guildsApplicationCommands)
            {
                discordCommands.AddRange(await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(guildId, guildCommands));
            }
        }
        else
        {
            discordCommands.AddRange(await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId, globalApplicationCommands));

            //If the same command is registered in multiple guilds, only add it once to the debug guild
            discordCommands.AddRange(await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId, guildsApplicationCommands
                .SelectMany(x => x.Value)
                .GroupBy(x => x.Name)
                .Select(x => x.First())));
        }

        Dictionary<ulong, Command> commandsDictionary = [];
        foreach (DiscordApplicationCommand discordCommand in discordCommands)
        {
            bool commandFound = false;
            foreach (Command command in extension.Commands.Values)
            {
                string snakeCaseCommandName = ToSnakeCase(command.Name);
                if (snakeCaseCommandName == ToSnakeCase(discordCommand.Name) || ToSnakeCase(command.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName) == snakeCaseCommandName)
                {
                    commandsDictionary.Add(discordCommand.Id, command);
                    commandFound = true;
                    break;
                }
            }

            if (!commandFound)
            {
                SlashLogging.UnknownCommandName(this.logger, discordCommand.Name, null);
                continue;
            }
        }

        this.Commands = commandsDictionary.ToFrozenDictionary();
        SlashLogging.RegisteredCommands(this.logger, this.Commands.Count, null);
    }

    public async Task<DiscordApplicationCommand> ToApplicationCommandAsync(Command command)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        // Translate the command's name and description.
        Dictionary<string, string> nameLocalizations = [];
        Dictionary<string, string> descriptionLocalizations = [];
        if (command.Attributes.OfType<InteractionLocalizerAttribute>().FirstOrDefault() is InteractionLocalizerAttribute localizerAttribute)
        {

            nameLocalizations = await this.ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.name");
            descriptionLocalizations = await this.ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.description");
        }

        // Convert the subcommands or parameters into application options
        List<DiscordApplicationCommandOption> options = [];
        if (command.Subcommands.Any())
        {
            foreach (Command subCommand in command.Subcommands)
            {
                options.Add(await this.ToApplicationParameterAsync(subCommand));
            }
        }
        else
        {
            foreach (CommandParameter parameter in command.Parameters)
            {
                if (parameter.Attributes.OfType<ParamArrayAttribute>().Any())
                {
                    // Fill til 25
                    for (int i = options.Count; i < 24; i++)
                    {
                        options.Add(await this.ToApplicationParameterAsync(command, parameter, i));
                    }
                }

                options.Add(await this.ToApplicationParameterAsync(command, parameter));
            }
        }

        if (!descriptionLocalizations.TryGetValue("en-US", out string? description))
        {
            description = command.Description;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            description = "No description provided.";
        }

        // Create the top level application command.
        return new(
            name: ToSnakeCase(nameLocalizations.TryGetValue("en-US", out string? name) ? name : command.Name),
            description: description,
            options: options,
            type: DiscordApplicationCommandType.SlashCommand,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            allowDMUsage: command.Attributes.Any(x => x is AllowDMUsageAttribute),
            defaultMemberPermissions: command.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? DiscordPermissions.None,
            nsfw: command.Attributes.Any(x => x is RequireNsfwAttribute),
            contexts: command.Attributes.OfType<InteractionAllowedContextsAttribute>().FirstOrDefault()?.AllowedContexts,
            integrationTypes: command.Attributes.OfType<InteractionInstallTypeAttribute>().FirstOrDefault()?.InstallTypes
        );
    }

    public async Task<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        // Convert the subcommands or parameters into application options
        List<DiscordApplicationCommandOption> options = [];
        if (command.Subcommands.Any())
        {
            foreach (Command subCommand in command.Subcommands)
            {
                options.Add(await this.ToApplicationParameterAsync(subCommand));
            }
        }
        else
        {
            foreach (CommandParameter parameter in command.Parameters)
            {
                options.Add(await this.ToApplicationParameterAsync(command, parameter));
            }
        }

        // Translate the subcommand's name and description.
        Dictionary<string, string> nameLocalizations = [];
        Dictionary<string, string> descriptionLocalizations = [];
        if (command.Attributes.OfType<InteractionLocalizerAttribute>().FirstOrDefault() is InteractionLocalizerAttribute localizerAttribute)
        {
            nameLocalizations = await this.ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.name");
            descriptionLocalizations = await this.ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.description");
        }

        if (!descriptionLocalizations.TryGetValue("en-US", out string? description))
        {
            description = command.Description;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            description = "No description provided.";
        }

        return new(
            name: ToSnakeCase(nameLocalizations.TryGetValue("en-US", out string? name) ? name : command.Name),
            description: description,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            type: command.Subcommands.Any() ? DiscordApplicationCommandOptionType.SubCommandGroup : DiscordApplicationCommandOptionType.SubCommand,
            options: options
        );
    }

    private async Task<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command, CommandParameter parameter, int? i = null)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        if (!this.TypeMappings.TryGetValue(this.GetConverterFriendlyBaseType(parameter.Type), out DiscordApplicationCommandOptionType type))
        {
            throw new InvalidOperationException($"No type mapping found for parameter type '{parameter.Type.Name}'");
        }

        SlashMinMaxValueAttribute? minMaxValue = parameter.Attributes.OfType<SlashMinMaxValueAttribute>().FirstOrDefault();
        SlashMinMaxLengthAttribute? minMaxLength = parameter.Attributes.OfType<SlashMinMaxLengthAttribute>().FirstOrDefault();

        // Translate the parameter's name and description.
        Dictionary<string, string> nameLocalizations = [];
        Dictionary<string, string> descriptionLocalizations = [];
        if (parameter.Attributes.OfType<InteractionLocalizerAttribute>().FirstOrDefault() is InteractionLocalizerAttribute localizerAttribute)
        {
            StringBuilder localeIdBuilder = new();
            localeIdBuilder.Append($"{command.FullName}.parameters.{parameter.Name}");
            if (i.HasValue)
            {
                localeIdBuilder.Append($".{i}");
            }

            nameLocalizations = await this.ExecuteLocalizerAsync(localizerAttribute.LocalizerType, localeIdBuilder.ToString() + ".name");
            descriptionLocalizations = await this.ExecuteLocalizerAsync(localizerAttribute.LocalizerType, localeIdBuilder.ToString() + ".description");
        }

        IEnumerable<DiscordApplicationCommandOptionChoice> choices = [];
        if (parameter.Attributes.OfType<SlashChoiceProviderAttribute>().FirstOrDefault() is SlashChoiceProviderAttribute choiceAttribute)
        {
            using AsyncServiceScope scope = this.extension.ServiceProvider.CreateAsyncScope();
            choices = await choiceAttribute.GrabChoicesAsync(scope.ServiceProvider, parameter);
        }

        if (!nameLocalizations.TryGetValue("en-US", out string? name))
        {
            name = i.HasValue ? $"{parameter.Name}_{i}" : parameter.Name;
        }

        if (!descriptionLocalizations.TryGetValue("en-US", out string? description))
        {
            description = parameter.Description;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            description = "No description provided.";
        }

        object maxValue = minMaxValue?.MaxValue!;
        object minValue = minMaxValue?.MinValue!;

        maxValue = maxValue switch
        {
            byte value => Math.Min(value, byte.MaxValue),
            sbyte value => Math.Min(value, sbyte.MaxValue),
            short value => Math.Min(value, short.MaxValue),
            ushort value => Math.Min(value, ushort.MaxValue),
            int value => Math.Min(value, int.MaxValue),
            uint value => Math.Min(value, uint.MaxValue),
            _ => maxValue,
        };

        minValue = minValue switch
        {
            byte value => Math.Min(value, byte.MinValue),
            sbyte value => Math.Max(value, sbyte.MinValue),
            short value => Math.Max(value, short.MinValue),
            ushort value => Math.Min(value, ushort.MinValue),
            int value => Math.Max(value, int.MinValue),
            uint value => Math.Min(value, uint.MinValue),
            _ => minValue,
        };

        return new(
            name: ToSnakeCase(name),
            description: description,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            autocomplete: parameter.Attributes.Any(x => x is SlashAutoCompleteProviderAttribute),
            channelTypes: parameter.Attributes.OfType<SlashChannelTypesAttribute>().FirstOrDefault()?.ChannelTypes ?? [],
            choices: choices,
            maxLength: minMaxLength?.MaxLength,
            maxValue: maxValue, // Incorrect nullable annotations within the lib
            minLength: minMaxLength?.MinLength,
            minValue: minValue,
            required: !parameter.DefaultValue.HasValue,
            type: type
        );
    }

    internal async ValueTask<Dictionary<string, string>> ExecuteLocalizerAsync(Type localizer, string name)
    {
        using AsyncServiceScope scope = this.extension!.ServiceProvider.CreateAsyncScope();
        IInteractionLocalizer instance;
        try
        {
            instance = (IInteractionLocalizer)ActivatorUtilities.CreateInstance(scope.ServiceProvider, localizer);
        }
        catch (Exception)
        {
            ILogger<InteractionLocalizerAttribute> logger = this.extension!.ServiceProvider
                .GetService<ILogger<InteractionLocalizerAttribute>>() ?? NullLogger<InteractionLocalizerAttribute>.Instance;

            logger.LogWarning("Failed to create an instance of {TypeName} for localization of {SymbolName}.", localizer, name);
            return [];
        }

        Dictionary<string, string> localized = [];
        foreach ((DiscordLocale locale, string translation) in await instance.TranslateAsync(name.Replace(' ', '.').ToLowerInvariant()))
        {
            localized.Add(locale.ToString().Replace('_', '-'), translation);
        }

        return localized;
    }

    private async ValueTask<AutoCompleteContext?> ParseAutoCompleteArgumentsAsync(InteractionConverterContext converterContext, InteractionCreateEventArgs eventArgs)
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
                //TODO: Change this comparison to use StringComparison.Ordinal once the automatic conversion to snake_case is no longer applied.
                DiscordInteractionDataOption? option = converterContext.Options.FirstOrDefault(x => x.Name.Equals(converterContext.Parameter.Name, StringComparison.OrdinalIgnoreCase));
                if (option is null)
                {
                    continue;
                }

                if (option.Focused)
                {
                    autoCompleteParameter = converterContext.Parameter;
                    autoCompleteOption = option;
                    break;
                }

                IOptional optional = await this.ConverterDelegates[this.GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext, eventArgs);
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
                    Channel = eventArgs.Interaction.Channel,
                    Command = converterContext.Command,
                    Extension = converterContext.Extension,
                    Interaction = eventArgs.Interaction,
                    Options = converterContext.Options,
                    ServiceScope = converterContext.ServiceScope,
                    User = eventArgs.Interaction.User
                },
                Exception = new ArgumentParseException(converterContext.Parameter, error),
                CommandObject = null
            });

            return null;
        }

        return new AutoCompleteContext()
        {
            Arguments = parsedArguments,
            AutoCompleteArgument = autoCompleteParameter,
            Channel = eventArgs.Interaction.Channel,
            Command = converterContext.Command,
            Extension = converterContext.Extension,
            Interaction = eventArgs.Interaction,
            Options = converterContext.Options,
            ServiceScope = converterContext.ServiceScope,
            User = eventArgs.Interaction.User,
            UserInput = autoCompleteOption.RawValue
        };
    }

    public override SlashCommandContext CreateCommandContext
    (
        InteractionConverterContext converterContext,
        InteractionCreateEventArgs eventArgs,
        Dictionary<CommandParameter, object?> parsedArguments
    )
    {
        return new()
        {
            Arguments = parsedArguments,
            Channel = eventArgs.Interaction.Channel,
            Command = converterContext.Command,
            Extension = this.extension ?? throw new InvalidOperationException("SlashCommandProcessor has not been configured."),
            Interaction = eventArgs.Interaction,
            Options = converterContext.Options,
            ServiceScope = converterContext.ServiceScope,
            User = eventArgs.Interaction.User
        };
    }

    private static string ToSnakeCase(ReadOnlySpan<char> str)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < str.Length; i++)
        {
            char character = str[i];

            // camelCase, PascalCase
            if (i != 0 && char.IsUpper(character))
            {
                stringBuilder.Append('_');
            }

            stringBuilder.Append(char.ToLowerInvariant(character));
        }

        return stringBuilder.ToString();
    }
}
