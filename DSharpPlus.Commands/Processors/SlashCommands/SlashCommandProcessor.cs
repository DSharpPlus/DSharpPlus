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
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;

public sealed class SlashCommandProcessor : BaseCommandProcessor<InteractionCreateEventArgs, ISlashArgumentConverter, SlashConverterContext, SlashCommandContext>
{
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

        SlashConverterContext converterContext = new()
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
                IEnumerable<DiscordAutoCompleteChoice> choices = await converterContext.Parameter.Attributes.OfType<SlashAutoCompleteProviderAttribute>().First().AutoCompleteAsync(autoCompleteContext);
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

            command = command.Subcommands.First(x => x.Name == option.Name);
            options = option.Options ?? [];
        }

        return true;
    }

    public void AddApplicationCommands(params DiscordApplicationCommand[] applicationCommands) => this._applicationCommands.AddRange(applicationCommands);
    public void AddApplicationCommands(IEnumerable<DiscordApplicationCommand> applicationCommands) => this._applicationCommands.AddRange(applicationCommands);

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

        IReadOnlyList<DiscordApplicationCommand> discordCommands = extension.DebugGuildId is null
            ? await extension.Client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommands)
            : await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId.Value, applicationCommands);

        Dictionary<ulong, Command> commandsDictionary = [];
        foreach (DiscordApplicationCommand discordCommand in discordCommands)
        {
            if (!extension.Commands.TryGetValue(discordCommand.Name, out Command? command))
            {
                foreach (Command aliasedCommand in extension.Commands.Values)
                {
                    if (aliasedCommand.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName == discordCommand.Name)
                    {
                        command = aliasedCommand;
                        break;
                    }
                }
            }

            if (command is null)
            {
                SlashLogging.UnknownCommandName(this._logger, discordCommand.Name, null);
                continue;
            }

            commandsDictionary.Add(discordCommand.Id, command);
        }

        this.Commands = commandsDictionary.ToFrozenDictionary();
        SlashLogging.RegisteredCommands(this._logger, this.Commands.Count, null);
    }

    [SuppressMessage("Roslyn", "CA1859", Justification = "Incorrect warning in NET 8-rc2. TODO: Open an issue in dotnet/runtime.")]
    public async Task<DiscordApplicationCommand> ToApplicationCommandAsync(Command command)
    {
        if (this._extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        // Translate the command's name and description.
        IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
        IReadOnlyDictionary<string, string> descriptionLocalizations = new Dictionary<string, string>();
        if (command.Attributes.OfType<SlashLocalizerAttribute>().FirstOrDefault() is SlashLocalizerAttribute localizerAttribute)
        {
            AsyncServiceScope scope = this._extension.ServiceProvider.CreateAsyncScope();
            nameLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.name");
            descriptionLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.description");
            await scope.DisposeAsync();
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

        // Create the top level application command.
        return new(
            name: nameLocalizations.TryGetValue("en-US", out string? name) ? name : command.Name,
            description: descriptionLocalizations.TryGetValue("en-US", out string? description) ? description : command.Description,
            options: options,
            type: ApplicationCommandType.SlashCommand,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            allowDMUsage: command.Attributes.Any(x => x is AllowDMUsageAttribute),
            defaultMemberPermissions: command.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? Permissions.None,
            nsfw: command.Attributes.Any(x => x is RequireNsfwAttribute)
        );
    }

    [SuppressMessage("Roslyn", "CA1859", Justification = "Incorrect warning in NET 8-rc2. TODO: Open an issue in dotnet/runtime.")]
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
        IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
        IReadOnlyDictionary<string, string> descriptionLocalizations = new Dictionary<string, string>();
        if (command.Attributes.OfType<SlashLocalizerAttribute>().FirstOrDefault() is SlashLocalizerAttribute localizerAttribute)
        {
            AsyncServiceScope scope = this._extension.ServiceProvider.CreateAsyncScope();
            nameLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.name");
            descriptionLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, $"{command.FullName}.description");
            await scope.DisposeAsync();
        }

        return new(
            name: nameLocalizations.TryGetValue("en-US", out string? name) ? name : command.Name,
            description: descriptionLocalizations.TryGetValue("en-US", out string? description) ? description : command.Description!,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            type: command.Subcommands.Any() ? ApplicationCommandOptionType.SubCommandGroup : ApplicationCommandOptionType.SubCommand,
            options: options
        );
    }

    [SuppressMessage("Roslyn", "CA1859", Justification = "Incorrect warning in NET 8-rc2. TODO: Open an issue in dotnet/runtime.")]
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
        AsyncServiceScope scope = this._extension.ServiceProvider.CreateAsyncScope();

        // Translate the parameter's name and description.
        IReadOnlyDictionary<string, string> nameLocalizations = new Dictionary<string, string>();
        IReadOnlyDictionary<string, string> descriptionLocalizations = new Dictionary<string, string>();
        if (parameter.Attributes.OfType<SlashLocalizerAttribute>().FirstOrDefault() is SlashLocalizerAttribute localizerAttribute)
        {
            StringBuilder localeIdBuilder = new();
            localeIdBuilder.Append($"{command.FullName}.parameters.{parameter.Name}");
            if (i.HasValue)
            {
                localeIdBuilder.Append($".{i}");
            }

            nameLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, localeIdBuilder.ToString() + ".name");
            descriptionLocalizations = await localizerAttribute.LocalizeAsync(scope.ServiceProvider, localeIdBuilder.ToString() + ".description");
        }

        IEnumerable<DiscordApplicationCommandOptionChoice> choices = [];
        if (parameter.Attributes.OfType<SlashChoiceProviderAttribute>().FirstOrDefault() is SlashChoiceProviderAttribute choiceAttribute)
        {
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

        await scope.DisposeAsync();
        return new(
            name: name,
            description: description!,
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

    private async ValueTask<AutoCompleteContext?> ParseAutoCompleteArgumentsAsync(SlashConverterContext converterContext, InteractionCreateEventArgs eventArgs)
    {
        if (this._extension is null)
        {
            return null;
        }

        Dictionary<CommandParameter, object?> parsedArguments = [];
        try
        {
            while (converterContext.NextParameter() && !converterContext.Options.ElementAt(converterContext.ParameterIndex).Focused)
            {
                IOptional optional = await this.ConverterDelegates[this.GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext, eventArgs);
                if (!optional.HasValue)
                {
                    break;
                }

                parsedArguments.Add(converterContext.Parameter, optional.RawValue);
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
            UserInput = converterContext.Options.ElementAt(converterContext.ParameterIndex).Value
        };
    }

    public override SlashCommandContext CreateCommandContext(SlashConverterContext converterContext, InteractionCreateEventArgs eventArgs, Dictionary<CommandParameter, object?> parsedArguments) => new()
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

    private static string ToSnakeCase(string str)
    {
        StringBuilder stringBuilder = new();
        foreach (char character in str)
        {
            // kebab-cased, somehow.
            if (character == '-')
            {
                stringBuilder.Append('_');
                continue;
            }

            // camelCase, PascalCase
            if (char.IsUpper(character))
            {
                stringBuilder.Append('_');
            }

            stringBuilder.Append(char.ToLowerInvariant(character));
        }

        return stringBuilder.ToString();
    }
}
