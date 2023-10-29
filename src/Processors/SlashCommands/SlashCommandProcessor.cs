using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public sealed class SlashCommandProcessor : ICommandProcessor<InteractionCreateEventArgs>
    {
        public IReadOnlyDictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> Converters { get; private set; } = new Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>>();
        public IReadOnlyDictionary<Type, ApplicationCommandOptionType> TypeMappings { get; private set; } = new Dictionary<Type, ApplicationCommandOptionType>();

        private readonly Dictionary<Type, ISlashArgumentConverter> _converters = [];
        private bool _eventsRegistered;

        public void AddConverter<T>(ISlashArgumentConverter<T> converter) => _converters.Add(typeof(T), converter);
        public void AddConverter(Type type, ISlashArgumentConverter converter)
        {
            if (!converter.GetType().IsAssignableTo(typeof(ISlashArgumentConverter<>).MakeGenericType(type)))
            {
                throw new ArgumentException($"Type '{converter.GetType().Name}' Converter must implement '{typeof(ISlashArgumentConverter<>).MakeGenericType(type).Name}'", nameof(converter));
            }

            _converters.Add(type, converter);
        }
        public void AddConverters(Assembly assembly) => AddConverters(assembly.GetTypes());
        public void AddConverters(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                Type? genericArgumentConverter = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISlashArgumentConverter<>));
                if (type.IsAbstract || type.IsInterface || genericArgumentConverter is null)
                {
                    continue;
                }

                try
                {
                    object? converter = Activator.CreateInstance(type);
                    if (converter is null)
                    {
                        continue;
                    }

                    AddConverter(genericArgumentConverter.GenericTypeArguments[0], (ISlashArgumentConverter)converter);
                }
                catch (Exception) { }
            }
        }

        public Task ConfigureAsync(CommandAllExtension extension, ConfigureCommandsEventArgs eventArgs)
        {
            AddConverters(typeof(SlashCommandProcessor).Assembly);

            Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> converters = [];
            Dictionary<Type, ApplicationCommandOptionType> typeMappings = [];
            foreach ((Type type, ISlashArgumentConverter converter) in _converters)
            {
                Type genericConverter = typeof(ISlashArgumentConverter<>).MakeGenericType(type);
                MethodInfo? convertAsyncMethod = genericConverter.GetMethod("ConvertAsync", [typeof(ConverterContext), typeof(InteractionCreateEventArgs)]) ?? throw new InvalidOperationException($"Converter {converter.GetType().Name} does not implement {genericConverter.Name}");

                MethodInfo? executeConvertAsyncMethod = typeof(SlashCommandProcessor).GetMethod(nameof(ExecuteConvertAsync), BindingFlags.Public | BindingFlags.Static) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
                executeConvertAsyncMethod = executeConvertAsyncMethod.MakeGenericMethod(type);

                converters.Add(type, executeConvertAsyncMethod.CreateDelegate<ConverterDelegate<InteractionCreateEventArgs>>(converter));
                typeMappings.Add(type, converter.ArgumentType);
            }

            Converters = converters.ToFrozenDictionary();
            TypeMappings = typeMappings.ToFrozenDictionary();
            if (_eventsRegistered)
            {
                return RegisterSlashCommandsAsync(extension);
            }

            _eventsRegistered = true;
            extension.Client.GuildDownloadCompleted += async (client, eventArgs) => await RegisterSlashCommandsAsync(extension);
            extension.Client.InteractionCreated += async (client, eventArgs) =>
            {
                ConverterContext? converterContext = await CreateConverterContext(extension, eventArgs);
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

        public static Task<ConverterContext?> CreateConverterContext(CommandAllExtension extension, InteractionCreateEventArgs eventArgs)
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

            return Task.FromResult<ConverterContext?>(new SlashConverterContext()
            {
                Extension = extension,
                Command = command,
                Interaction = eventArgs.Interaction,
                Channel = eventArgs.Interaction.Channel,
                User = eventArgs.Interaction.User,
            });
        }

        public async Task<CommandContext?> ParseArgumentsAsync(ConverterContext converterContext, InteractionCreateEventArgs eventArgs)
        {
            List<object?> parameters = [];
            while (converterContext.NextArgument())
            {
                IOptional optional = await Converters[converterContext.Argument.Type](converterContext, eventArgs);
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

        public DiscordApplicationCommandOption ToApplicationArgument(Command command) => new(
            name: command.Name,
            description: command.Description,
            type: command.Subcommands.Any() ? ApplicationCommandOptionType.SubCommandGroup : ApplicationCommandOptionType.SubCommand,
            options: command.Subcommands.Select(ToApplicationArgument)
        );

        public DiscordApplicationCommandOption ToApplicationArgument(CommandArgument argument) => new(
            name: argument.Name,
            description: argument.Description,
            type: TypeMappings.TryGetValue(argument.Type, out ApplicationCommandOptionType type) ? type : throw new InvalidOperationException($"No type mapping found for argument type '{argument.Type.Name}'"),
            required: !argument.DefaultValue.HasValue
        );

        public static async Task<IOptional> ExecuteConvertAsync<T>(ISlashArgumentConverter<T> converter, ConverterContext context, InteractionCreateEventArgs eventArgs) => await converter.ConvertAsync(context, eventArgs);
    }
}
