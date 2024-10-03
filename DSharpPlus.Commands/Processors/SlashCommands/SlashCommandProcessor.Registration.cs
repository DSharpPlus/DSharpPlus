using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Commands.Processors.SlashCommands.Metadata;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public sealed partial class SlashCommandProcessor : BaseCommandProcessor<ISlashArgumentConverter, InteractionConverterContext, SlashCommandContext>
{
    [GeneratedRegex(@"^[-_\p{L}\p{N}\p{IsDevanagari}\p{IsThai}]{1,32}$")]
    private static partial Regex NameLocalizationRegex();

    private static FrozenDictionary<ulong, Command> applicationCommandMapping = FrozenDictionary<ulong, Command>.Empty;
    private static readonly List<DiscordApplicationCommand> applicationCommands = [];

    /// <summary>
    /// The mapping of application command ids to <see cref="Command"/> objects.
    /// </summary>
    public IReadOnlyDictionary<ulong, Command> ApplicationCommandMapping => applicationCommandMapping;

    public void AddApplicationCommands(params DiscordApplicationCommand[] commands) => applicationCommands.AddRange(commands);
    public void AddApplicationCommands(IEnumerable<DiscordApplicationCommand> commands) => applicationCommands.AddRange(commands);

    /// <summary>
    /// Registers <see cref="CommandsExtension.Commands"/> as application commands.
    /// This will registers regardless of <see cref="SlashCommandConfiguration.RegisterCommands"/>'s value.
    /// </summary>
    /// <param name="extension">The extension to read the commands from.</param>
    public async ValueTask RegisterSlashCommandsAsync(CommandsExtension extension)
    {
        if (this.isApplicationCommandsRegistered)
        {
            return;
        }

        this.isApplicationCommandsRegistered = true;

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
            // 2. GroupBy name
            // 3. Only take the first command per name
            IEnumerable<DiscordApplicationCommand> distinctCommands = guildsApplicationCommands
                .SelectMany(x => x.Value)
                .Concat(globalApplicationCommands)
                .GroupBy(x => x.Name)
                .Select(x => x.First());

            discordCommands.AddRange(await extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId, distinctCommands));
        }

        applicationCommandMapping = MapApplicationCommands(discordCommands).ToFrozenDictionary();
        SlashLogging.registeredCommands(this.logger, applicationCommandMapping.Count, applicationCommandMapping.Values.SelectMany(command => command.Flatten()).Count(), null);
    }

    /// <summary>
    /// Matches the application commands to the commands in the command tree.
    /// </summary>
    /// <param name="applicationCommands">The application commands obtained from Discord. Accepts both global and guild commands.</param>
    /// <returns>A dictionary mapping the application command id to the command in the command tree.</returns>
    public IReadOnlyDictionary<ulong, Command> MapApplicationCommands(IReadOnlyList<DiscordApplicationCommand> applicationCommands)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        Dictionary<ulong, Command> commandsDictionary = [];
        IReadOnlyList<Command> processorSpecificCommands = this.extension!.GetCommandsForProcessor(this);
        IReadOnlyList<Command> flattenCommands = processorSpecificCommands.SelectMany(x => x.Flatten()).ToList();
        foreach (DiscordApplicationCommand discordCommand in applicationCommands)
        {
            bool commandFound = false;
            string discordCommandName = this.Configuration.ParameterNamePolicy.TransformText(discordCommand.Name).ToString();
            if (discordCommand.Type is DiscordApplicationCommandType.MessageContextMenu or DiscordApplicationCommandType.UserContextMenu)
            {
                foreach (Command command in flattenCommands)
                {
                    string commandName = this.Configuration.ParameterNamePolicy.GetCommandName(command);
                    if (commandName == discordCommandName)
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
                    string commandName = this.Configuration.ParameterNamePolicy.GetCommandName(command);
                    if (commandName == discordCommandName)
                    {
                        commandsDictionary.Add(discordCommand.Id, command);
                        commandFound = true;
                        break;
                    }
                }
            }

            if (!commandFound)
            {
                // TODO: How do we report this to the user? Return a custom object perhaps?
                SlashLogging.unknownCommandName(this.logger, discordCommandName, null);
            }
        }

        return commandsDictionary;
    }


    /// <summary>
    /// Only use this for commands of type <see cref="DiscordApplicationCommandType.SlashCommand "/>.
    /// It will cut out every subcommands which are considered to be not a SlashCommand
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask<DiscordApplicationCommand> ToApplicationCommandAsync(Command command)
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
                if (subCommand.Attributes.OfType<SlashCommandTypesAttribute>().FirstOrDefault() is SlashCommandTypesAttribute slashCommandTypesAttribute
                    && !slashCommandTypesAttribute.ApplicationCommandTypes.Contains(DiscordApplicationCommandType.SlashCommand))
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
                // Check if the parameter is using a multi-argument attribute.
                // If it is we need to add the parameter multiple times.
                MultiArgumentAttribute? multiArgumentAttribute = parameter.Attributes.FirstOrDefault(attribute => attribute is MultiArgumentAttribute) as MultiArgumentAttribute;

                // Check if the parameter is using params instead of a multi-argument attribute.
                if (multiArgumentAttribute is null)
                {
                    // This is just a normal parameter.
                    if (!parameter.Attributes.Any(attribute => attribute is ParamArrayAttribute))
                    {
                        options.Add(await ToApplicationParameterAsync(command, parameter));
                        continue;
                    }

                    multiArgumentAttribute = new MultiArgumentAttribute(int.MaxValue, 1);
                }

                // Add the multi-argument parameter multiple times until we reach the maximum argument count.
                for (int i = 0; i < Math.Min(multiArgumentAttribute.MaximumArgumentCount - 1, 24 - options.Count); i++)
                {
                    options.Add(await ToApplicationParameterAsync(command, parameter, i));
                }
            }
        }

        string? description = command.Description;
        if (string.IsNullOrWhiteSpace(description))
        {
            description = "No description provided.";
        }

        // Create the top level application command.
        return new(
            name: this.Configuration.ParameterNamePolicy.GetCommandName(command),
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

    public async ValueTask<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command)
        => await ToApplicationParameterAsync(command, 0);

    private async ValueTask<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command, int depth = 1)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        // Convert the subcommands or parameters into application options
        List<DiscordApplicationCommandOption> options = [];
        if (command.Subcommands.Count > 0)
        {
            if (depth >= 3)
            {
                throw new InvalidOperationException($"Slash command failed validation: Command '{command.Name}' nests too deeply. Discord only supports up to 3 levels of nesting.");
            }

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
                // Check if the parameter is using a multi-argument attribute.
                // If it is we need to add the parameter multiple times.
                MultiArgumentAttribute? multiArgumentAttribute = parameter.Attributes.FirstOrDefault(attribute => attribute is MultiArgumentAttribute) as MultiArgumentAttribute;

                // Check if the parameter is using params instead of a multi-argument attribute.
                if (multiArgumentAttribute is null)
                {
                    // This is just a normal parameter.
                    if (!parameter.Attributes.Any(attribute => attribute is ParamArrayAttribute))
                    {
                        options.Add(await ToApplicationParameterAsync(command, parameter));
                        continue;
                    }

                    multiArgumentAttribute = new MultiArgumentAttribute(int.MaxValue, 1);
                }

                // Add the multi-argument parameter multiple times until we reach the maximum argument count.
                for (int i = 0; i < Math.Min(multiArgumentAttribute.MaximumArgumentCount - 1, 24 - options.Count); i++)
                {
                    options.Add(await ToApplicationParameterAsync(command, parameter, i));
                }
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

        string? description = command.Description;
        if (string.IsNullOrWhiteSpace(description))
        {
            description = "No description provided.";
        }

        return new(
            name: this.Configuration.ParameterNamePolicy.GetCommandName(command),
            description: description,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            type: command.Subcommands.Count > 0 ? DiscordApplicationCommandOptionType.SubCommandGroup : DiscordApplicationCommandOptionType.SubCommand,
            options: options
        );
    }

    private async ValueTask<DiscordApplicationCommandOption> ToApplicationParameterAsync(Command command, CommandParameter parameter, int i = -1)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("SlashCommandProcessor has not been configured.");
        }

        // Fucking scope man. Let me else if in peace
        // We need the converter to grab the parameter type's application command option type value.
        if (!this.Converters.TryGetValue(GetConverterFriendlyBaseType(parameter.Type), out ISlashArgumentConverter? slashArgumentConverter))
        {
            throw new InvalidOperationException($"No converter found for parameter type '{parameter.Type.Name}'");
        }

        // Translate the parameter's name and description.
        Dictionary<string, string> nameLocalizations = [];
        Dictionary<string, string> descriptionLocalizations = [];
        if (parameter.Attributes.OfType<InteractionLocalizerAttribute>().FirstOrDefault() is InteractionLocalizerAttribute localizerAttribute)
        {
            StringBuilder localeIdBuilder = new();
            localeIdBuilder.Append($"{command.FullName}.parameters.{parameter.Name}");
            if (i != -1)
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

        string? description = parameter.Description;
        if (string.IsNullOrWhiteSpace(description))
        {
            description = "No description provided.";
        }

        MinMaxLengthAttribute? minMaxLength = parameter.Attributes.OfType<MinMaxLengthAttribute>().FirstOrDefault();
        MinMaxValueAttribute? minMaxValue = parameter.Attributes.OfType<MinMaxValueAttribute>().FirstOrDefault();
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
            name: this.Configuration.ParameterNamePolicy.GetParameterName(parameter, i),
            description: description,
            name_localizations: nameLocalizations,
            description_localizations: descriptionLocalizations,
            autocomplete: parameter.Attributes.Any(x => x is SlashAutoCompleteProviderAttribute),
            channelTypes: parameter.Attributes.OfType<ChannelTypesAttribute>().FirstOrDefault()?.ChannelTypes ?? [],
            choices: choices,
            maxLength: minMaxLength?.MaxLength,
            maxValue: maxValue,
            minLength: minMaxLength?.MinLength,
            minValue: minValue,
            required: !parameter.DefaultValue.HasValue,
            type: slashArgumentConverter.ParameterType
        );
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
                if (subCommand.Attributes.OfType<SlashCommandTypesAttribute>().FirstOrDefault() is SlashCommandTypesAttribute slashCommandTypesAttribute &&
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

            if (descriptionLocalization.Key.Length is < 1 or > 100)
            {
                throw new InvalidOperationException
                (
                    $"Slash command failed validation: {command.Name} description localization key is longer than 100 characters.\n" +
                    $"(Description localization key ({descriptionLocalization.Key}) is {descriptionLocalization.Key.Length - 100} characters too long)"
                );

                // Come back to this when we have actual validation that does more than a length check
                //throw new InvalidOperationException
                //(
                //    $"Slash command failed validation: {command.Name} description localization key contains invalid characters.\n" +
                //    $"(Description localization key ({descriptionLocalization.Key}) contains invalid characters)"
                //);
            }
        }

        if (!NameLocalizationRegex().IsMatch(command.Name))
        {
            throw new InvalidOperationException($"Slash command failed validation: {command.Name} name contains invalid characters.");
        }

        if (command.Description?.Length is < 1 or > 100)
        {
            throw new InvalidOperationException($"Slash command failed validation: {command.Name} description is longer than 100 characters.");

            // Come back to this when we have actual validation that does more than a length check
            //throw new InvalidOperationException
            //(
            //    $"Slash command failed validation: {command.Name} description contains invalid characters."
            //);
        }
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
}
