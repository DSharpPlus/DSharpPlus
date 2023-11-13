namespace DSharpPlus.Commands;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// An all in one extension for managing commands.
/// </summary>
public sealed class CommandsExtension : BaseExtension
{
    /// <inheritdoc cref="CommandsConfiguration.ServiceProvider"/>
    public IServiceProvider ServiceProvider { get; init; }

    /// <inheritdoc cref="CommandsConfiguration.DebugGuildId"/>
    public ulong? DebugGuildId { get; init; }

    /// <inheritdoc cref="CommandsConfiguration.UseDefaultCommandErrorHandler"/>
    public bool UseDefaultCommandErrorHandler { get; init; }

    public CommandExecutor CommandExecutor { get; init; } = new();

    /// <summary>
    /// The registered commands that the users can execute.
    /// </summary>
    public IReadOnlyDictionary<string, Command> Commands { get; private set; } = new Dictionary<string, Command>();
    private readonly List<CommandBuilder> _commandBuilders = [];

    public IReadOnlyDictionary<Type, ICommandProcessor> Processors { get; private set; } = new Dictionary<Type, ICommandProcessor>();
    private readonly Dictionary<Type, ICommandProcessor> _processors = [];

    /// <summary>
    /// Executed everytime a command is finished executing.
    /// </summary>
    public event AsyncEventHandler<CommandsExtension, CommandExecutedEventArgs> CommandExecuted { add => this._commandExecuted.Register(value); remove => this._commandExecuted.Unregister(value); }
    internal readonly AsyncEvent<CommandsExtension, CommandExecutedEventArgs> _commandExecuted = new("COMMANDS_COMMAND_EXECUTED", EverythingWentWrongErrorHandler);

    /// <summary>
    /// Executed everytime a command has errored.
    /// </summary>
    public event AsyncEventHandler<CommandsExtension, CommandErroredEventArgs> CommandErrored
    {
        add
        {
            if (this.UseDefaultCommandErrorHandler)
            {
                this._commandErrored.Unregister(DefaultCommandErrorHandlerAsync);
            }

            this._commandErrored.Register(value);
        }
        remove => this._commandErrored.Unregister(value);
    }
    internal readonly AsyncEvent<CommandsExtension, CommandErroredEventArgs> _commandErrored = new("COMMANDS_COMMAND_ERRORED", EverythingWentWrongErrorHandler);

    /// <summary>
    /// Used to log messages from this extension.
    /// </summary>
    private readonly ILogger<CommandsExtension> _logger;

    /// <summary>
    /// Creates a new instance of the <see cref="CommandsExtension"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    internal CommandsExtension(CommandsConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        this.ServiceProvider = configuration.ServiceProvider;
        this.DebugGuildId = configuration.DebugGuildId;
        this.UseDefaultCommandErrorHandler = configuration.UseDefaultCommandErrorHandler;
        if (this.UseDefaultCommandErrorHandler)
        {
            // We don't do `this.CommandErrored += DefaultCommandErrorHandlerAsync` because that'll immediately remove the event handler.
            this._commandErrored.Register(DefaultCommandErrorHandlerAsync);
        }

        // Attempt to get the user defined logging, otherwise setup a null logger since the D#+ Default Logger is internal.
        this._logger = this.ServiceProvider.GetService<ILogger<CommandsExtension>>() ?? NullLogger<CommandsExtension>.Instance;
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
        else if (this.Client is not null)
        {
            throw new InvalidOperationException("Commands Extension is already initialized.");
        }

        this.Client = client;
        this.Client.SessionCreated += async (_, _) => await this.RefreshAsync();
    }

    public void AddCommand(CommandBuilder command) => this._commandBuilders.Add(command);
    public void AddCommand(Delegate commandDelegate) => this._commandBuilders.Add(CommandBuilder.From(commandDelegate));
    public void AddCommands(IEnumerable<CommandBuilder> commands) => this._commandBuilders.AddRange(commands);
    public void AddCommands(Assembly assembly) => this.AddCommands(assembly.GetTypes());
    public void AddCommands(params CommandBuilder[] commands) => this._commandBuilders.AddRange(commands);
    public void AddCommands(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            if (type.GetCustomAttribute<CommandAttribute>() is not null)
            {
                this._commandBuilders.Add(CommandBuilder.From(type));
                continue;
            }

            foreach (MethodInfo method in type.GetMethods())
            {
                if (method.GetCustomAttribute<CommandAttribute>() is not null)
                {
                    this._commandBuilders.Add(CommandBuilder.From(method));
                }
            }
        }
    }

    public void AddProcessor(ICommandProcessor processor) => this._processors.Add(processor.GetType(), processor);
    public void AddProcessors(params ICommandProcessor[] processors) => this.AddProcessors((IEnumerable<ICommandProcessor>)processors);
    public void AddProcessors(IEnumerable<ICommandProcessor> processors)
    {
        foreach (ICommandProcessor processor in processors)
        {
            this._processors.Add(processor.GetType(), processor);
        }
    }

    public TProcessor GetProcessor<TProcessor>() where TProcessor : ICommandProcessor => (TProcessor)this._processors[typeof(TProcessor)];

    public async Task RefreshAsync()
    {
        Dictionary<string, Command> commands = [];
        foreach (CommandBuilder commandBuilder in this._commandBuilders)
        {
            try
            {
                Command command = commandBuilder.Build();
                commands.Add(command.Name, command);
            }
            catch (Exception error)
            {
                this._logger.LogError(error, "Failed to build command '{CommandBuilder}'", commandBuilder.Name);
            }
        }

        this.Commands = commands.ToFrozenDictionary();

        foreach (ICommandProcessor processor in this._processors.Values)
        {
            await processor.ConfigureAsync(this);
        }
    }

    // TODO: Disposable pattern for processors
    /// <inheritdoc />
    public override void Dispose()
    {
        return;
    }

    /// <summary>
    /// The event handler used to log all unhandled exceptions, usually from when <see cref="_commandErrored"/> itself errors.
    /// </summary>
    /// <param name="asyncEvent">The event that errored.</param>
    /// <param name="error">The error that occurred.</param>
    /// <param name="handler">The handler/method that errored.</param>
    /// <param name="sender">The extension.</param>
    /// <param name="eventArgs">The event arguments passed to <paramref name="handler"/>.</param>
    private static void EverythingWentWrongErrorHandler<TArgs>(AsyncEvent<CommandsExtension, TArgs> asyncEvent, Exception error, AsyncEventHandler<CommandsExtension, TArgs> handler, CommandsExtension sender, TArgs eventArgs) where TArgs : AsyncEventArgs => sender._logger.LogError(error, "Event handler '{Method}' for event {AsyncEvent} threw an unhandled exception.", handler.Method, asyncEvent.Name);

    /// <summary>
    /// The default command error handler. Only used if <see cref="UseDefaultCommandErrorHandler"/> is set to true.
    /// </summary>
    /// <param name="extension">The extension.</param>
    /// <param name="eventArgs">The event arguments containing the exception.</param>
    private static async Task DefaultCommandErrorHandlerAsync(CommandsExtension extension, CommandErroredEventArgs eventArgs)
    {
        StringBuilder stringBuilder = new();
        DiscordMessageBuilder messageBuilder = new();

        // Error message
        stringBuilder.Append(eventArgs.Exception switch
        {
            CommandNotFoundException commandNotFoundException => $"Command {Formatter.InlineCode(Formatter.Sanitize(commandNotFoundException.CommandName))} was not found.",
            ArgumentParseException argumentParseException => $"Failed to parse argument {Formatter.InlineCode(Formatter.Sanitize(argumentParseException.Parameter.Name))}.",
            CheckFailedException checkFailedException => $"Check {Formatter.InlineCode(Formatter.Sanitize(checkFailedException.Check.GetType().Name))} failed.",
            DiscordException discordException when discordException.Response is not null && (int)discordException.Response.StatusCode >= 500 && (int)discordException.Response.StatusCode < 600 => $"Discord API error {discordException.Response.StatusCode} occurred: {discordException.JsonMessage ?? "No further information was provided."}",
            DiscordException discordException when discordException.Response is not null => $"Discord API error {discordException.Response.StatusCode} occurred: {discordException.JsonMessage ?? discordException.Message}",
            _ => $"An unexpected error occurred: {eventArgs.Exception.Message}"
        });

        // Stack trace
        if (!string.IsNullOrWhiteSpace(eventArgs.Exception.StackTrace))
        {
            // If the stack trace can fit inside a codeblock
            if (8 + eventArgs.Exception.StackTrace.Length + stringBuilder.Length >= 2000)
            {
                stringBuilder.Append($"```\n{eventArgs.Exception.StackTrace}\n```");
            }
            // Otherwise, we'll have to upload it as a file
            else
            {
                messageBuilder.AddFile("StackTrace.txt", new MemoryStream(Encoding.UTF8.GetBytes(eventArgs.Exception.StackTrace)), AddFileOptions.CloseStream);
            }
        }

        messageBuilder.WithContent(stringBuilder.ToString());
        await eventArgs.Context.RespondAsync(messageBuilder);
    }
}
