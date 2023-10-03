using System;
using DSharpPlus.AsyncEvents;
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

        /// <summary>
        /// Executed everytime a command is finished executing.
        /// </summary>
        public event AsyncEventHandler<CommandAllExtension, AsyncEventArgs> CommandExecuted { add => _commandExecuted.Register(value); remove => _commandExecuted.Unregister(value); }
        internal readonly AsyncEvent<CommandAllExtension, AsyncEventArgs> _commandExecuted = new("COMMANDALL_COMMAND_EXECUTED", EverythingWentWrongErrorHandler);

        /// <summary>
        /// Executed before registering slash commands.
        /// </summary>
        public event AsyncEventHandler<CommandAllExtension, AsyncEventArgs> ConfigureCommands { add => _configureCommands.Register(value); remove => _configureCommands.Unregister(value); }
        internal readonly AsyncEvent<CommandAllExtension, AsyncEventArgs> _configureCommands = new("COMMANDALL_CONFIGURE_COMMANDS", EverythingWentWrongErrorHandler);

        /// <summary>
        /// Executed everytime a command errored and <see cref="BaseCommand.OnErrorAsync(CommandContext, Exception)"/> also errored.
        /// </summary>
        public event AsyncEventHandler<CommandAllExtension, AsyncEventArgs> CommandErrored { add => _commandErrored.Register(value); remove => _commandErrored.Unregister(value); }
        internal readonly AsyncEvent<CommandAllExtension, AsyncEventArgs> _commandErrored = new("COMMANDALL_COMMAND_ERRORED", EverythingWentWrongErrorHandler);

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
