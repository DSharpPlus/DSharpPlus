using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.EventProcessors;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.CommandAll
{
    /// <summary>
    /// Because not everyone can decide between slash commands and text commands.
    /// </summary>
    public sealed class CommandAllExtension : BaseExtension
    {
        /// <inheritdoc cref="CommandAllConfiguration.ServiceProvider"/>
        public IServiceProvider ServiceProvider { get; init; }

        /// <inheritdoc cref="CommandAllConfiguration.DebugGuildId"/>
        public ulong? DebugGuildId { get; init; }

        public CommandExecutor CommandExecutor { get; init; } = new();

        /// <summary>
        /// The argument converters to use when parsing user input.
        /// </summary>
        public IReadOnlyDictionary<Type, ConverterDelegate> Converters { get; private set; } = new Dictionary<Type, ConverterDelegate>();
        private readonly Dictionary<Type, ConverterDelegate> _converters = new();

        /// <summary>
        /// The registered commands that the users can execute.
        /// </summary>
        public IReadOnlyDictionary<string, Command> Commands { get; private set; } = new Dictionary<string, Command>();
        private readonly List<CommandBuilder> _commandBuilders = new();

        public TextCommandProcessor? TextCommandProcessor { get; init; }
        public SlashCommandProcessor? SlashCommandProcessor { get; init; }

        /// <summary>
        /// Executed before registering slash commands.
        /// </summary>
        public event AsyncEventHandler<CommandAllExtension, ConfigureCommandsEventArgs> ConfigureCommands { add => _configureCommands.Register(value); remove => _configureCommands.Unregister(value); }
        internal readonly AsyncEvent<CommandAllExtension, ConfigureCommandsEventArgs> _configureCommands = new("COMMANDALL_CONFIGURE_COMMANDS", EverythingWentWrongErrorHandler);

        /// <summary>
        /// Executed everytime a command is finished executing.
        /// </summary>
        public event AsyncEventHandler<CommandAllExtension, CommandExecutedEventArgs> CommandExecuted { add => _commandExecuted.Register(value); remove => _commandExecuted.Unregister(value); }
        internal readonly AsyncEvent<CommandAllExtension, CommandExecutedEventArgs> _commandExecuted = new("COMMANDALL_COMMAND_EXECUTED", EverythingWentWrongErrorHandler);

        /// <summary>
        /// Executed everytime a command errored and <see cref="BaseCommand.OnErrorAsync(CommandContext, Exception)"/> also errored.
        /// </summary>
        public event AsyncEventHandler<CommandAllExtension, CommandErroredEventArgs> CommandErrored { add => _commandErrored.Register(value); remove => _commandErrored.Unregister(value); }
        internal readonly AsyncEvent<CommandAllExtension, CommandErroredEventArgs> _commandErrored = new("COMMANDALL_COMMAND_ERRORED", EverythingWentWrongErrorHandler);

        /// <summary>
        /// Used to log messages from this extension.
        /// </summary>
        private readonly ILogger<CommandAllExtension> _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="CommandAllExtension"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        internal CommandAllExtension(CommandAllConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ServiceProvider = configuration.ServiceProvider;
            DebugGuildId = configuration.DebugGuildId;

            if (configuration.TextCommandsConfiguration.Enabled)
            {
                TextCommandProcessor = new()
                {
                    Options = configuration.TextCommandsConfiguration.Options
                };

                ConfigureCommands += TextCommandProcessor.ConfigureAsync;
            }

            if (configuration.SlashCommandsConfiguration.Enabled)
            {
                SlashCommandProcessor = new()
                {
                    Configuration = configuration.SlashCommandsConfiguration
                };

                ConfigureCommands += SlashCommandProcessor.ConfigureAsync;
            }

            AddConverters(typeof(CommandAllExtension).Assembly);

            // Attempt to get the user defined logging, otherwise setup a null logger since the D#+ Default Logger is internal.
            _logger = ServiceProvider.GetService<ILogger<CommandAllExtension>>() ?? NullLogger<CommandAllExtension>.Instance;
        }

        /// <summary>
        /// Sets up the extension to use the specified <see cref="DiscordClient"/>.
        /// </summary>
        /// <param name="client">The client to register our event handlers too.</param>
        protected override void Setup(DiscordClient client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            else if (Client is not null)
            {
                throw new InvalidOperationException("CommandAll Extension is already initialized.");
            }

            Client = client;
            Client.SessionCreated += async (_, _) => await RefreshAsync();
        }

        public void AddConverter<T>(ConverterDelegate converter) => _converters.Add(typeof(T), converter);
        public void AddConverter(Type type) => AddConverters(new[] { type });
        public void AddConverters(Assembly assembly) => AddConverters(assembly.GetTypes());
        public void AddConverters(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.GetCustomAttribute<ConverterAttribute>() is not ConverterAttribute converterAttribute
                        || method.ReturnType != typeof(Task<IOptional>))
                    {
                        continue;
                    }

                    if (method.IsStatic)
                    {
                        _converters.Add(converterAttribute.ParameterType, method.CreateDelegate<ConverterDelegate>(null));
                    }
                    else
                    {
                        object instance = ActivatorUtilities.CreateInstance(ServiceProvider, method.DeclaringType!);
                        _converters.Add(converterAttribute.ParameterType, method.CreateDelegate<ConverterDelegate>(instance));
                    }
                }
            }
        }

        public void AddCommand(CommandBuilder command) => _commandBuilders.Add(command);
        public void AddCommand(Delegate commandDelegate) => _commandBuilders.Add(CommandBuilder.From(commandDelegate));
        public void AddCommands(IEnumerable<CommandBuilder> commands) => _commandBuilders.AddRange(commands);
        public void AddCommands(Assembly assembly) => AddCommands(assembly.GetTypes());
        public void AddCommands(params CommandBuilder[] commands) => _commandBuilders.AddRange(commands);
        public void AddCommands(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                if (type.GetCustomAttribute<CommandAttribute>() is not null)
                {
                    _commandBuilders.Add(CommandBuilder.From(type));
                    continue;
                }

                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.GetCustomAttribute<CommandAttribute>() is not null)
                    {
                        _commandBuilders.Add(CommandBuilder.From(method));
                    }
                }
            }
        }

        public async Task RefreshAsync()
        {
            Converters = _converters.ToFrozenDictionary();
            await _configureCommands.InvokeAsync(this, new ConfigureCommandsEventArgs()
            {
                Extension = this,
                CommandBuilders = _commandBuilders
            });

            Dictionary<string, Command> commands = new();
            foreach (CommandBuilder commandBuilder in _commandBuilders)
            {
                try
                {
                    Command command = commandBuilder.Build();
                    commands.Add(command.Name, command);
                }
                catch (Exception error)
                {
                    _logger.LogError(error, "Failed to build command '{CommandBuilder}'", commandBuilder.Name);
                }
            }

            Commands = commands.ToFrozenDictionary();
        }

        /// <summary>
        /// The event handler used to log all unhandled exceptions, usually from when <see cref="_commandErrored"/> itself errors.
        /// </summary>
        /// <param name="asyncEvent">The event that errored.</param>
        /// <param name="error">The error that occurred.</param>
        /// <param name="handler">The handler/method that errored.</param>
        /// <param name="sender">The extension.</param>
        /// <param name="eventArgs">The event arguments passed to <paramref name="handler"/>.</param>
        private static void EverythingWentWrongErrorHandler<TArgs>(AsyncEvent<CommandAllExtension, TArgs> asyncEvent, Exception error, AsyncEventHandler<CommandAllExtension, TArgs> handler, CommandAllExtension sender, TArgs eventArgs) where TArgs : AsyncEventArgs => sender._logger.LogError(error, "Event handler '{Method}' for event {AsyncEvent} threw an unhandled exception.", handler.Method, asyncEvent.Name);

        /// <inheritdoc />
        public override void Dispose()
        {
            return;
        }
    }
}
