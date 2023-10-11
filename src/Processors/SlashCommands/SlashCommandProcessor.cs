using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Commands.Contexts;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors
{
    public sealed class SlashCommandProcessor : ICommandProcessor<InteractionCreateEventArgs>
    {
        public static readonly IReadOnlyDictionary<Type, ApplicationCommandOptionType> DefaultTypeMappings = new Dictionary<Type, ApplicationCommandOptionType>()
        {
            [typeof(string)] = ApplicationCommandOptionType.String,
            [typeof(long)] = ApplicationCommandOptionType.Integer,
            [typeof(bool)] = ApplicationCommandOptionType.Boolean,
            [typeof(double)] = ApplicationCommandOptionType.Number,
            [typeof(Enum)] = ApplicationCommandOptionType.String,
            [typeof(TimeSpan)] = ApplicationCommandOptionType.String,
            [typeof(DiscordUser)] = ApplicationCommandOptionType.User,
            [typeof(DiscordRole)] = ApplicationCommandOptionType.Role,
            [typeof(DiscordEmoji)] = ApplicationCommandOptionType.String,
            [typeof(DiscordChannel)] = ApplicationCommandOptionType.Channel,
            [typeof(DiscordAttachment)] = ApplicationCommandOptionType.Attachment,
            [typeof(SnowflakeObject)] = ApplicationCommandOptionType.Mentionable,
        };

        public SlashCommandConfiguration Configuration { get; init; } = new();
        public IReadOnlyDictionary<Type, ApplicationCommandOptionType> TypeMappings { get; private set; } = DefaultTypeMappings;
        private bool _eventsRegistered;

        public Task ConfigureAsync(CommandAllExtension extension, ConfigureCommandsEventArgs eventArgs)
        {
            Dictionary<Type, ApplicationCommandOptionType> typeMappings = new(DefaultTypeMappings);
            foreach ((Type type, ConverterDelegate converter) in eventArgs.Extension.Converters)
            {
                if (converter.Method.GetCustomAttribute<SlashConverterAttribute>() is SlashConverterAttribute converterAttribute)
                {
                    typeMappings.Add(type, converterAttribute.ParameterType);
                }
            }

            TypeMappings = typeMappings;
            if (_eventsRegistered)
            {
                return RegisterSlashCommandsAsync(extension);
            }

            _eventsRegistered = true;
            extension.Client.GuildDownloadCompleted += async (client, eventArgs) => await RegisterSlashCommandsAsync(extension);
            extension.Client.InteractionCreated += async (client, eventArgs) =>
            {
                ConverterContext? converterContext = await ConvertEventAsync(extension, eventArgs);
                if (converterContext is null)
                {
                    return;
                }

                CommandContext? commandContext = await ParseArgumentsAsync(converterContext, eventArgs);
                if (commandContext is null)
                {
                    return;
                }

                await extension.CommandExecutor.ExecuteAsync(commandContext);
            };

            return Task.CompletedTask;
        }

        public Task<ConverterContext?> ConvertEventAsync(CommandAllExtension extension, InteractionCreateEventArgs eventArgs)
        {
            if (eventArgs.Interaction.Type != InteractionType.ApplicationCommand || !extension.Commands.TryGetValue(eventArgs.Interaction.Data.Name, out Command? command) || command is null)
            {
                return Task.FromResult<ConverterContext?>(null);
            }

            IEnumerable<DiscordInteractionDataOption> options = eventArgs.Interaction.Data.Options;
            while (options is not null && options.Any())
            {
                DiscordInteractionDataOption option = options.First();
                if (option.Type is not ApplicationCommandOptionType.SubCommandGroup and not ApplicationCommandOptionType.SubCommand)
                {
                    break;
                }

                command = command.Subcommands.First(x => x.Name == option.Name);
                options = option.Options;
            }

            return Task.FromResult<ConverterContext?>(new ConverterContext(extension, eventArgs, command));
        }

        public async Task<CommandContext?> ParseArgumentsAsync(ConverterContext converterContext, InteractionCreateEventArgs eventArgs)
        {
            List<object?> parameters = new();
            while (converterContext.NextArgument())
            {
                IOptional optional = await converterContext.Extension.Converters[converterContext.Argument!.Type](converterContext);
                if (!optional.HasValue)
                {
                    break;
                }

                parameters.Add(optional.RawValue);
            }

            return parameters.Count != converterContext.Command.Arguments.Count
                ? null
                : new SlashContext()
                {
                    User = eventArgs.Interaction.User,
                    Channel = eventArgs.Interaction.Channel,
                    Extension = converterContext.Extension,
                    Command = converterContext.Command,
                    Arguments = converterContext.Command.Arguments.ToDictionary(x => x, x => parameters[converterContext.Command.Arguments.IndexOf(x)]),
                    Interaction = eventArgs.Interaction
                };
        }

        public Task RegisterSlashCommandsAsync(CommandAllExtension extension)
        {
            DiscordApplicationCommand[] applicationCommands = extension.Commands.Values.Select(ToApplicationCommand).ToArray();
            return extension.DebugGuildId is null
                ? extension.Client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommands)
                : extension.Client.BulkOverwriteGuildApplicationCommandsAsync(extension.DebugGuildId.Value, applicationCommands);
        }

        public DiscordApplicationCommand ToApplicationCommand(Command command) => new(
            name: command.Name,
            description: command.Description,
            options: command.Subcommands.Any() ? command.Subcommands.Select(ToApplicationArgument) : command.Arguments.Select(ToApplicationArgument),
            type: ApplicationCommandType.SlashCommand,
            //name_localizations: command.Attributes.FirstOrDefault(x => x is SlashLocalizerAttribute slashLocalizer && slashLocalizer.Localize(command.Name))
            //description_localizations: command.Attributes.FirstOrDefault(x => x is SlashLocalizerAttribute slashLocalizer && slashLocalizer.Localize(command.Description))
            //allowDMUsage: command.Attributes.Any(x => x is SlashCommandAllowDMUsageAttribute)
            defaultMemberPermissions: command.Attributes.OfType<PermissionsAttribute>().FirstOrDefault()?.UserPermissions ?? Permissions.None,
            nsfw: command.Attributes.Any(x => x is NsfwAttribute)
        );

        private DiscordApplicationCommandOption ToApplicationArgument(Command command) => new(
            name: command.Name,
            description: command.Description,
            type: command.Subcommands.Any() ? ApplicationCommandOptionType.SubCommandGroup : ApplicationCommandOptionType.SubCommand,
            options: command.Subcommands.Select(ToApplicationArgument)

        );

        private DiscordApplicationCommandOption ToApplicationArgument(CommandArgument argument) => new(
            name: argument.Name,
            description: argument.Description,
            type: TypeMappings.TryGetValue(argument.Type, out ApplicationCommandOptionType type) ? type : ApplicationCommandOptionType.String,
            required: !argument.DefaultValue.HasValue
        );
    }
}
