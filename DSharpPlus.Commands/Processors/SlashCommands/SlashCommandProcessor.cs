using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.Commands.ArgumentModifiers;
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

public sealed partial class SlashCommandProcessor : BaseCommandProcessor<InteractionCreatedEventArgs, ISlashArgumentConverter, InteractionConverterContext, SlashCommandContext>
{
    // Required for GuildDownloadCompleted event
    public const DiscordIntents RequiredIntents = DiscordIntents.Guilds;

    public IReadOnlyDictionary<Type, DiscordApplicationCommandOptionType> TypeMappings { get; private set; } = new Dictionary<Type, DiscordApplicationCommandOptionType>();
    public IReadOnlyDictionary<ulong, Command> ApplicationCommandMapping => applicationCommandMapping;

    /// <inheritdoc/>
    public override IReadOnlyList<Command> Commands => this.ApplicationCommandMapping.Values.ToList(); //TODO: alloc free?

    private static readonly List<DiscordApplicationCommand> applicationCommands = [];
    private static FrozenDictionary<ulong, Command> applicationCommandMapping;

    [GeneratedRegex(@"^[-_\p{L}\p{N}\p{IsDevanagari}\p{IsThai}]{1,32}$")]
    private partial Regex NameLocalizationRegex();

    [GeneratedRegex("^.{1,100}$")]
    private partial Regex DescrtiptionLocalizationRegex();

    private bool configured;

    /// <inheritdoc />
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
            extension.Client.InteractionCreated += ExecuteInteractionAsync;
            extension.Client.GuildDownloadCompleted += async (client, eventArgs) =>
            {
                if (client.ShardId == 0)
                {
                    await RegisterSlashCommandsAsync(extension);
                }
            };
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

        AsyncServiceScope serviceScope = this.extension.ServiceProvider.CreateAsyncScope();
        if (!TryFindCommand(eventArgs.Interaction, out Command? command, out IEnumerable<DiscordInteractionDataOption>? options))
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
            AutoCompleteContext? autoCompleteContext = await ParseAutoCompleteArgumentsAsync(converterContext, eventArgs);
            if (autoCompleteContext is not null)
            {
                IEnumerable<DiscordAutoCompleteChoice> choices = await autoCompleteContext.AutoCompleteArgument.Attributes.OfType<SlashAutoCompleteProviderAttribute>().First().AutoCompleteAsync(autoCompleteContext);
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
            }

            converterContext.ServiceScope.Dispose();
        }
        else
        {
            CommandContext? commandContext = await ParseArgumentsAsync(converterContext, eventArgs);
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
        if (!this.ApplicationCommandMapping.TryGetValue(interaction.Data.Id, out command))
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

    public void AddApplicationCommands(params DiscordApplicationCommand[] commands) => applicationCommands.AddRange(commands);
    public void AddApplicationCommands(IEnumerable<DiscordApplicationCommand> commands) => applicationCommands.AddRange(commands);

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

    [MemberNotNull(nameof(applicationCommands))]
    public async Task RegisterSlashCommandsAsync(CommandsExtension extension)
    {
        IReadOnlyList<Command> processorSpecificCommands = extension.GetCommandsForProcessor(this);
        List<DiscordApplicationCommand> globalApplicationCommands = [];
        Dictionary<ulong, List<DiscordApplicationCommand>> guildsApplicationCommands = [];
        globalApplicationCommands.AddRange(applicationCommands);
        foreach (Command command in processorSpecificCommands)
        {
            // If there is a SlashCommandTypesAttribute, check if it contains SlashCommandTypes.ApplicationCommand
            // If there isn't, default to SlashCommands
            if (command.Attributes.OfType<SlashCommandTypesAttribute>().FirstOrDefault() is SlashCommandTypesAttribute slashCommandTypesAttribute &&
                !slashCommandTypesAttribute.ApplicationCommandTypes.Contains(DiscordApplicationCommandType.SlashCommand))
            {
                continue;
            }

            DiscordApplicationCommand applicationCommand = await ToApplicationCommandAsync(command);

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
            // 1. Aggregate all guild specific and global commands
            // 2. Groupby name
            // 3. Only take the first command per name
            IEnumerable<DiscordApplicationCommand> distinctCommands = guildsApplicationCommands
                .SelectMany(x => x.Value)
                .Concat(globalApplicationCommands)
                .GroupBy(x => x.Name)
                .Select(x => x.First());
            
            discordCommands.AddRange(await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId, distinctCommands));
        }

        Dictionary<ulong, Command> commandsDictionary = [];
        IReadOnlyList<Command> flattenCommands = processorSpecificCommands.SelectMany(x => x.Flatten()).ToList();
        foreach (DiscordApplicationCommand discordCommand in discordCommands)
        {
            bool commandFound = false;

            if (discordCommand.Type is DiscordApplicationCommandType.MessageContextMenu
                                    or DiscordApplicationCommandType.UserContextMenu)
            {
                foreach (Command command in flattenCommands)
                {
                    if (command.FullName == discordCommand.Name || command.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName == discordCommand.Name)
                    {
                        commandsDictionary.Add(discordCommand.Id, command);
                        commandFound = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (Command command in processorSpecificCommands)
                {
                    string snakeCaseCommandName = ToSnakeCase(command.Name);
                    if (snakeCaseCommandName == ToSnakeCase(discordCommand.Name) || ToSnakeCase(command.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName) == snakeCaseCommandName)
                    {
                        commandsDictionary.Add(discordCommand.Id, command);
                        commandFound = true;
                        break;
                    }
                }
            }

            if (!commandFound)
            {
                SlashLogging.unknownCommandName(this.logger, discordCommand.Name, null);
            }
        }

        applicationCommandMapping = commandsDictionary.ToFrozenDictionary();
        SlashLogging.registeredCommands(this.logger, this.ApplicationCommandMapping.Count, this.ApplicationCommandMapping.Values.SelectMany(command => command.Flatten()).Count(), null);
    }


    /// <summary>
    /// Only use this for commands of type <see cref="DiscordApplicationCommandType.SlashCommand "/>.
    /// It will cut out every subcommands which are considered to be not a SlashCommand 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
            nameLocalizations = await ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.name");
            descriptionLocalizations = await ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.description");
        }

        ValidateSlashCommand(command, nameLocalizations, descriptionLocalizations);

        // Convert the subcommands or parameters into application options
        List<DiscordApplicationCommandOption> options = [];
        if (command.Subcommands.Any())
        {
            foreach (Command subCommand in command.Subcommands)
            {
                // If there is a SlashCommandTypesAttribute, check if it contains SlashCommandTypes.ApplicationCommand
                // If there isn't, default to SlashCommands
                if (command.Attributes.OfType<SlashCommandTypesAttribute>().FirstOrDefault() is SlashCommandTypesAttribute slashCommandTypesAttribute &&
                    !slashCommandTypesAttribute.ApplicationCommandTypes.Contains(DiscordApplicationCommandType.SlashCommand))
                {
                    continue;
                }
                
                options.Add(await ToApplicationParameterAsync(subCommand));
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
                        options.Add(await ToApplicationParameterAsync(command, parameter, i));
                    }
                }

                options.Add(await ToApplicationParameterAsync(command, parameter));
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

    [StackTraceHidden]
    public async Task<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command)
        => await this.ToApplicationParameterAsync(command, 0);

    private async Task<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command, int depth = 1)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        // Convert the subcommands or parameters into application options
        List<DiscordApplicationCommandOption> options = [];

        bool hasSubCommands = command.Subcommands.Count > 0;

        if (hasSubCommands && depth >= 3)
        {
            throw new InvalidOperationException
            (
                $"Slash command failed validation: {command.Name} nests too deeply. Discord only supports up to 3 levels of nesting."
            );
        }

        if (hasSubCommands)
        {
            depth++;
            foreach (Command subCommand in command.Subcommands)
            {
                options.Add(await ToApplicationParameterAsync(subCommand, depth));
            }
        }
        else
        {
            foreach (CommandParameter parameter in command.Parameters)
            {
                options.Add(await ToApplicationParameterAsync(command, parameter));
            }
        }

        // Translate the subcommand's name and description.
        Dictionary<string, string> nameLocalizations = [];
        Dictionary<string, string> descriptionLocalizations = [];
        if (command.Attributes.OfType<InteractionLocalizerAttribute>().FirstOrDefault() is InteractionLocalizerAttribute localizerAttribute)
        {
            nameLocalizations = await ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.name");
            descriptionLocalizations = await ExecuteLocalizerAsync(localizerAttribute.LocalizerType, $"{command.FullName}.description");
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
            type: hasSubCommands ? DiscordApplicationCommandOptionType.SubCommandGroup : DiscordApplicationCommandOptionType.SubCommand,
            options: options
        );
    }

    private async Task<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command, CommandParameter parameter, int? i = null)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        if (!this.TypeMappings.TryGetValue(GetConverterFriendlyBaseType(parameter.Type), out DiscordApplicationCommandOptionType type))
        {
            throw new InvalidOperationException($"No type mapping found for parameter type '{parameter.Type.Name}'");
        }

        MinMaxLengthAttribute? minMaxLength = parameter.Attributes.OfType<MinMaxLengthAttribute>().FirstOrDefault();
        MinMaxValueAttribute? minMaxValue = parameter.Attributes.OfType<MinMaxValueAttribute>().FirstOrDefault();

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

            nameLocalizations = await ExecuteLocalizerAsync(localizerAttribute.LocalizerType, localeIdBuilder.ToString() + ".name");
            descriptionLocalizations = await ExecuteLocalizerAsync(localizerAttribute.LocalizerType, localeIdBuilder.ToString() + ".description");
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
            channelTypes: parameter.Attributes.OfType<ChannelTypesAttribute>().FirstOrDefault()?.ChannelTypes ?? [],
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

    private async ValueTask<AutoCompleteContext?> ParseAutoCompleteArgumentsAsync(InteractionConverterContext converterContext, InteractionCreatedEventArgs eventArgs)
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
                SnakeCasedNameAttribute attribute = converterContext.Parameter.Attributes
                    .OfType<SnakeCasedNameAttribute>()
                    .Single();

                DiscordInteractionDataOption? option = converterContext.Options
                    .FirstOrDefault(x => x.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase));

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

                IOptional optional = await this.ConverterDelegates[GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext, eventArgs);

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
        InteractionCreatedEventArgs eventArgs,
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

    internal static string ToSnakeCase(ReadOnlySpan<char> str)
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

    /// <inheritdoc/>
    protected override async ValueTask<IOptional> ExecuteConverterAsync<T>
    (
        ISlashArgumentConverter converter,
        InteractionConverterContext converterContext,
        InteractionCreatedEventArgs eventArgs
    )
    {
        if (converter is not ISlashArgumentConverter<T> typedConverter)
        {
            throw new InvalidOperationException("The provided converter was of the wrong type.");
        }

        if (!converterContext.Parameter.Attributes.OfType<ParamArrayAttribute>().Any())
        {
            return await typedConverter.ConvertAsync(converterContext, eventArgs);
        }

        List<T> values = [];

        do
        {
            Optional<T> optional = await typedConverter.ConvertAsync(converterContext, eventArgs);

            if (!optional.HasValue)
            {
                break;
            }

            values.Add(optional.Value);
        } while (converterContext.NextParameter());

        return Optional.FromValue(values.ToArray());
    }

    /// <inheritdoc/>
    public override async ValueTask<SlashCommandContext?> ParseArgumentsAsync
    (
        InteractionConverterContext converterContext,
        InteractionCreatedEventArgs eventArgs
    )
    {
        if (this.extension is null)
        {
            return null;
        }

        Dictionary<CommandParameter, object?> parsedArguments = new(converterContext.Command.Parameters.Count);

        foreach (CommandParameter parameter in converterContext.Command.Parameters)
        {
            parsedArguments.Add(parameter, new ConverterSentinel());
        }

        try
        {
            while (converterContext.NextParameter())
            {
                if (converterContext.Argument is null)
                {
                    continue;
                }

                IOptional optional = await this.ConverterDelegates[GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext, eventArgs);

                if (!optional.HasValue)
                {
                    await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                    {
                        Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                        Exception = new ArgumentParseException(converterContext.Parameter, null, $"Argument Converter for type {converterContext.Parameter.Type.FullName} was unable to parse the argument."),
                        CommandObject = null
                    });

                    return null;
                }

                parsedArguments[converterContext.Parameter] = optional.RawValue;
            }

            if (parsedArguments.Any(x => x.Value is ConverterSentinel))
            {
                // Try to fill with default values
                foreach (CommandParameter parameter in converterContext.Command.Parameters)
                {
                    if (parsedArguments[parameter] is not ConverterSentinel)
                    {
                        continue;
                    }

                    if (!parameter.DefaultValue.HasValue)
                    {
                        await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                        {
                            Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                            Exception = new ArgumentParseException(converterContext.Parameter, null, "No value was provided for this parameter."),
                            CommandObject = null
                        });

                        return null;
                    }

                    parsedArguments[parameter] = parameter.DefaultValue.Value;
                }
            }
        }
        catch (Exception error)
        {
            await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
            {
                Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                Exception = new ArgumentParseException(converterContext.Parameter, error),
                CommandObject = null
            });

            return null;
        }

        return CreateCommandContext(converterContext, eventArgs, parsedArguments);
    }

    /// <summary>
    /// Only use this for commands of type <see cref="DiscordApplicationCommandType.SlashCommand "/>.
    /// It will NOT validate every subcommands which are considered to be a SlashCommand 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="nameLocalizations"></param>
    /// <param name="descriptionLocalizations"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void ValidateSlashCommand(Command command, IReadOnlyDictionary<string, string> nameLocalizations, IReadOnlyDictionary<string, string> descriptionLocalizations)
    {
        if (command.Subcommands.Count > 0)
        {
            foreach (Command subCommand in command.Subcommands)
            {
                // If there is a SlashCommandTypesAttribute, check if it contains SlashCommandTypes.ApplicationCommand
                // If there isn't, default to SlashCommands
                if (command.Attributes.OfType<SlashCommandTypesAttribute>().FirstOrDefault() is SlashCommandTypesAttribute slashCommandTypesAttribute &&
                    !slashCommandTypesAttribute.ApplicationCommandTypes.Contains(DiscordApplicationCommandType.SlashCommand))
                {
                    continue;
                }
                
                ValidateSlashCommand(subCommand, nameLocalizations, descriptionLocalizations);
            }
        }

        if (command.Name.Length > 32)
        {
            throw new InvalidOperationException
            (
                $"Slash command failed validation: {command.Name} is longer than 32 characters." +
                $"\n(Name is {command.Name.Length - 32} characters too long)"
            );
        }

        if (command.Description?.Length > 100)
        {
            throw new InvalidOperationException
            (
                $"Slash command failed validation: {command.Name} description is longer than 100 characters." +
                $"\n(Description is {command.Description.Length - 100} characters too long)"
            );
        }

        foreach (KeyValuePair<string, string> nameLocalization in nameLocalizations)
        {
            if (nameLocalization.Value.Length > 32)
            {
                throw new InvalidOperationException
                (
                    $"Slash command failed validation: {command.Name} name localization key is longer than 32 characters.\n" +
                    $"(Name localization key ({nameLocalization.Key}) is {nameLocalization.Key.Length - 32} characters too long)"
                );
            }

            if (!NameLocalizationRegex().IsMatch(nameLocalization.Key))
            {
                throw new InvalidOperationException
                (
                    $"Slash command failed validation: {command.Name} name localization key contains invalid characters.\n" +
                    $"(Name localization key ({nameLocalization.Key}) contains invalid characters)"
                );
            }
        }

        foreach (KeyValuePair<string, string> descriptionLocalization in descriptionLocalizations)
        {
            if (descriptionLocalization.Value.Length > 100)
            {
                throw new InvalidOperationException
                (
                    $"Slash command failed validation: {command.Name} description localization key is longer than 100 characters.\n" +
                    $"(Description localization key ({descriptionLocalization.Key}) is {descriptionLocalization.Key.Length - 100} characters too long)"
                );
            }

            if (!DescrtiptionLocalizationRegex().IsMatch(descriptionLocalization.Key))
            {
                throw new InvalidOperationException
                (
                    $"Slash command failed validation: {command.Name} description localization key contains invalid characters.\n" +
                    $"(Description localization key ({descriptionLocalization.Key}) contains invalid characters)"
                );
            }
        }

        if (!NameLocalizationRegex().IsMatch(command.Name))
        {
            throw new InvalidOperationException
            (
                $"Slash command failed validation: {command.Name} name contains invalid characters."
            );
        }

        if (!string.IsNullOrWhiteSpace(command.Description) && !DescrtiptionLocalizationRegex().IsMatch(command.Description))
        {
            throw new InvalidOperationException
            (
                $"Slash command failed validation: {command.Name} description contains invalid characters."
            );
        }
    }
}
