namespace DSharpPlus.Commands.Processors.SlashCommands;

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
using DSharpPlus.Commands.Processors.SlashCommands.Attributes;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class SlashCommandProcessor : BaseCommandProcessor<InteractionCreateEventArgs, ISlashArgumentConverter, InteractionConverterContext, SlashCommandContext>
{
    // Required for GuildDownloadCompleted event
    public const DiscordIntents RequiredIntents = DiscordIntents.Guilds;

    public IReadOnlyDictionary<Type, ApplicationCommandOptionType> TypeMappings { get; private set; } = new Dictionary<Type, ApplicationCommandOptionType>();
    public IReadOnlyDictionary<ulong, Command> Commands { get; private set; } = new Dictionary<ulong, Command>();

    private readonly List<DiscordApplicationCommand> _applicationCommands = [];
    private bool _configured;

    public override async ValueTask ConfigureAsync(CommandsExtension extension)
    {
        await base.ConfigureAsync(extension);

        Dictionary<Type, ApplicationCommandOptionType> typeMappings = [];
        foreach (LazyConverter lazyConverter in this._lazyConverters.Values)
        {
            ISlashArgumentConverter converter = lazyConverter.GetConverter(this._extension.ServiceProvider);
            typeMappings.Add(lazyConverter.ParameterType, converter.ParameterType);
        }

        this.TypeMappings = typeMappings.ToFrozenDictionary();
        if (!this._configured)
        {
            this._configured = true;
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
        if (this._extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }
        else if (eventArgs.Interaction.Type is not InteractionType.ApplicationCommand and not InteractionType.AutoComplete || eventArgs.Interaction.Data.Type is not ApplicationCommandType.SlashCommand)
        {
            return;
        }

        AsyncServiceScope serviceScope = this._extension.ServiceProvider.CreateAsyncScope();
        if (!this.TryFindCommand(eventArgs.Interaction, out Command? command, out IEnumerable<DiscordInteractionDataOption>? options))
        {
            await this._extension._commandErrored.InvokeAsync(this._extension, new CommandErroredEventArgs()
            {
                Context = new SlashCommandContext()
                {
                    Arguments = new Dictionary<CommandParameter, object?>(),
                    Channel = eventArgs.Interaction.Channel,
                    Command = null!,
                    Extension = this._extension,
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
            Extension = this._extension,
            Interaction = eventArgs.Interaction,
            Options = options.ToList(),
            ServiceScope = serviceScope,
            User = eventArgs.Interaction.User
        };

        if (eventArgs.Interaction.Type is InteractionType.AutoComplete)
        {
            AutoCompleteContext? autoCompleteContext = await this.ParseAutoCompleteArgumentsAsync(converterContext, eventArgs);
            if (autoCompleteContext is not null)
            {
                IEnumerable<DiscordAutoCompleteChoice> choices = await autoCompleteContext.AutoCompleteArgument.Attributes.OfType<SlashAutoCompleteProviderAttribute>().First().AutoCompleteAsync(autoCompleteContext);
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
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

            await this._extension.CommandExecutor.ExecuteAsync(commandContext);
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
            if (option.Type is not ApplicationCommandOptionType.SubCommandGroup and not ApplicationCommandOptionType.SubCommand)
            {
                break;
            }

            command = command.Subcommands.First(x => ToSnakeCase(x.Name) == option.Name);
            options = option.Options ?? [];
        }

        return true;
    }

    public void AddApplicationCommands(params DiscordApplicationCommand[] applicationCommands) => this._applicationCommands.AddRange(applicationCommands);
    public void AddApplicationCommands(IEnumerable<DiscordApplicationCommand> applicationCommands) => this._applicationCommands.AddRange(applicationCommands);

    public async ValueTask ClearDiscordSlashCommandsAsync(bool clearGuildCommands = false)
    {
        if (this._extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        await this._extension.Client.BulkOverwriteGlobalApplicationCommandsAsync(new List<DiscordApplicationCommand>());
        if (!clearGuildCommands)
        {
            return;
        }

        foreach (ulong guildId in this._extension.Client.Guilds.Keys)
        {
            await this._extension.Client.BulkOverwriteGuildApplicationCommandsAsync(guildId, new List<DiscordApplicationCommand>());
        }
    }

    public async Task RegisterSlashCommandsAsync(CommandsExtension extension)
    {
        List<DiscordApplicationCommand> applicationCommands = [];
        applicationCommands.AddRange(this._applicationCommands);
        foreach (Command command in extension.Commands.Values)
        {
            // If there is a SlashCommandTypesAttribute, check if it contains SlashCommandTypes.ApplicationCommand
            // If there isn't, default to SlashCommands
            if (command.Attributes.OfType<SlashCommandTypesAttribute>().FirstOrDefault() is SlashCommandTypesAttribute slashCommandTypesAttribute && !slashCommandTypesAttribute.ApplicationCommandTypes.Contains(ApplicationCommandType.SlashCommand))
            {
                continue;
            }

            applicationCommands.Add(await this.ToApplicationCommandAsync(command));
        }

        IReadOnlyList<DiscordApplicationCommand> discordCommands = extension.DebugGuildId is 0
            ? await extension.Client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommands)
            : await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId, applicationCommands);

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
                SlashLogging.UnknownCommandName(this._logger, discordCommand.Name, null);
                continue;
            }
        }

        this.Commands = commandsDictionary.ToFrozenDictionary();
        SlashLogging.RegisteredCommands(this._logger, this.Commands.Count, null);
    }

    public async Task<DiscordApplicationCommand> ToApplicationCommandAsync(Command command)
    {
        if (this._extension is null)
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
            type: ApplicationCommandType.SlashCommand,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            allowDMUsage: command.Attributes.Any(x => x is AllowDMUsageAttribute),
            defaultMemberPermissions: command.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? Permissions.None,
            nsfw: command.Attributes.Any(x => x is RequireNsfwAttribute),
            contexts: command.Attributes.OfType<InteractionAllowedContextsAttribute>().FirstOrDefault()?.AllowedContexts,
            integrationTypes: command.Attributes.OfType<InteractionInstallTypeAttribute>().FirstOrDefault()?.InstallTypes
        );
    }

    public async Task<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command)
    {
        if (this._extension is null)
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
            type: command.Subcommands.Any() ? ApplicationCommandOptionType.SubCommandGroup : ApplicationCommandOptionType.SubCommand,
            options: options
        );
    }

    private async Task<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command, CommandParameter parameter, int? i = null)
    {
        if (this._extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        if (!this.TypeMappings.TryGetValue(this.GetConverterFriendlyBaseType(parameter.Type), out ApplicationCommandOptionType type))
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
            using AsyncServiceScope scope = this._extension.ServiceProvider.CreateAsyncScope();
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

        return new(
            name: ToSnakeCase(name),
            description: description,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            autocomplete: parameter.Attributes.Any(x => x is SlashAutoCompleteProviderAttribute),
            channelTypes: parameter.Attributes.OfType<SlashChannelTypesAttribute>().FirstOrDefault()?.ChannelTypes ?? [],
            choices: choices,
            maxLength: minMaxLength?.MaxLength,
            maxValue: minMaxValue?.MaxValue!, // Incorrect nullable annotations within the lib
            minLength: minMaxLength?.MinLength,
            minValue: minMaxValue?.MinValue!, // Incorrect nullable annotations within the lib
            required: !parameter.DefaultValue.HasValue,
            type: type
        );
    }

    internal async ValueTask<Dictionary<string, string>> ExecuteLocalizerAsync(Type localizer, string name)
    {
        using AsyncServiceScope scope = this._extension!.ServiceProvider.CreateAsyncScope();
        IInteractionLocalizer instance;
        try
        {
            instance = (IInteractionLocalizer)ActivatorUtilities.CreateInstance(scope.ServiceProvider, localizer);
        }
        catch (Exception)
        {
            ILogger<InteractionLocalizerAttribute> logger = this._extension!.ServiceProvider
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
        if (this._extension is null)
        {
            return null;
        }

        Dictionary<CommandParameter, object?> parsedArguments = [];
        try
        {
            // Parse until we find the parameter that the user is currently typing
            while (converterContext.NextParameter() && !converterContext.Options.ElementAt(converterContext.ParameterIndex).Focused)
            {
                IOptional optional = await this.ConverterDelegates[this.GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext, eventArgs);
                parsedArguments.Add(converterContext.Parameter, optional.HasValue
                    ? optional.RawValue
                    : converterContext.Parameter.DefaultValue
                );
            }
        }
        catch (Exception error)
        {
            await this._extension._commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
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
            AutoCompleteArgument = converterContext.Parameter,
            Channel = eventArgs.Interaction.Channel,
            Command = converterContext.Command,
            Extension = converterContext.Extension,
            Interaction = eventArgs.Interaction,
            Options = converterContext.Options,
            ServiceScope = converterContext.ServiceScope,
            User = eventArgs.Interaction.User,
            UserInput = converterContext.Options.ElementAt(converterContext.ParameterIndex).RawValue
        };
    }

    public override SlashCommandContext CreateCommandContext(InteractionConverterContext converterContext, InteractionCreateEventArgs eventArgs, Dictionary<CommandParameter, object?> parsedArguments) => new()
    {
        Arguments = parsedArguments,
        Channel = eventArgs.Interaction.Channel,
        Command = converterContext.Command,
        Extension = this._extension ?? throw new InvalidOperationException("SlashCommandProcessor has not been configured."),
        Interaction = eventArgs.Interaction,
        Options = converterContext.Options,
        ServiceScope = converterContext.ServiceScope,
        User = eventArgs.Interaction.User
    };

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
