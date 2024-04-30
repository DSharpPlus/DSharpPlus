namespace DSharpPlus.Commands;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Commands.Processors.UserCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using CheckFunc = System.Func
<
    object,
    DSharpPlus.Commands.ContextChecks.ContextCheckAttribute,
    CommandContext,
    System.Threading.Tasks.ValueTask<string?>
>;

using ParameterCheckFunc = System.Func
<
    object,
    DSharpPlus.Commands.ContextChecks.ParameterChecks.ParameterCheckAttribute,
    DSharpPlus.Commands.ContextChecks.ParameterChecks.ParameterCheckInfo,
    CommandContext,
    System.Threading.Tasks.ValueTask<string?>
>;

/// <summary>
/// An all in one extension for managing commands.
/// </summary>
public sealed class CommandsExtension : BaseExtension
{
    /// <inheritdoc cref="CommandsConfiguration.ServiceProvider"/>
    public IServiceProvider ServiceProvider { get; init; }

    /// <inheritdoc cref="CommandsConfiguration.DebugGuildId"/>
    public ulong DebugGuildId { get; init; }

    /// <inheritdoc cref="CommandsConfiguration.UseDefaultCommandErrorHandler"/>
    public bool UseDefaultCommandErrorHandler { get; init; }

    /// <inheritdoc cref="CommandsConfiguration.RegisterDefaultCommandProcessors"/>
    public bool RegisterDefaultCommandProcessors { get; init; }

    public ICommandExecutor CommandExecutor { get; init; }

    /// <summary>
    /// The registered commands that the users can execute.
    /// </summary>
    public IReadOnlyDictionary<string, Command> Commands { get; private set; } = new Dictionary<string, Command>();
    private readonly List<CommandBuilder> _commandBuilders = [];

    /// <summary>
    /// All registered command processors.
    /// </summary>
    public IReadOnlyDictionary<Type, ICommandProcessor> Processors => _processors;
    private readonly Dictionary<Type, ICommandProcessor> _processors = [];

    public IReadOnlyList<ContextCheckMapEntry> Checks => checks;
    private readonly List<ContextCheckMapEntry> checks = [];

    public IReadOnlyList<ParameterCheckMapEntry> ParameterChecks => parameterChecks;
    private readonly List<ParameterCheckMapEntry> parameterChecks = [];

    /// <summary>
    /// Executed everytime a command is finished executing.
    /// </summary>
    public event AsyncEventHandler<CommandsExtension, CommandExecutedEventArgs> CommandExecuted { add => _commandExecuted.Register(value); remove => _commandExecuted.Unregister(value); }
    internal readonly AsyncEvent<CommandsExtension, CommandExecutedEventArgs> _commandExecuted = new("COMMANDS_COMMAND_EXECUTED", EverythingWentWrongErrorHandler);

    /// <summary>
    /// Executed everytime a command has errored.
    /// </summary>
    public event AsyncEventHandler<CommandsExtension, CommandErroredEventArgs> CommandErrored { add => _commandErrored.Register(value); remove => _commandErrored.Unregister(value); }
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

        ServiceProvider = configuration.ServiceProvider;
        DebugGuildId = configuration.DebugGuildId;
        UseDefaultCommandErrorHandler = configuration.UseDefaultCommandErrorHandler;
        RegisterDefaultCommandProcessors = configuration.RegisterDefaultCommandProcessors;
        CommandExecutor = configuration.CommandExecutor;
        if (UseDefaultCommandErrorHandler)
        {
            CommandErrored += DefaultCommandErrorHandlerAsync;
        }

        // Attempt to get the user defined logging, otherwise setup a null logger since the D#+ Default Logger is internal.
        _logger = ServiceProvider.GetService<ILogger<CommandsExtension>>() ?? NullLogger<CommandsExtension>.Instance;
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
            throw new InvalidOperationException("Commands Extension is already initialized.");
        }

        Client = client;
        Client.SessionCreated += async (_, _) => await RefreshAsync();

        AddCheck<DirectMessageUsageCheck>();
        AddCheck<RequireApplicationOwnerCheck>();
        AddCheck<RequireGuildCheck>();
        AddCheck<RequireNsfwCheck>();
        AddCheck<RequirePermissionsCheck>();
        AddCheck<TextMessageReplyCheck>();

        AddParameterCheck<RequireHierarchyCheck>();
    }

    public void AddCommand(Delegate commandDelegate) => _commandBuilders.Add(CommandBuilder.From(commandDelegate));
    public void AddCommand(Delegate commandDelegate, params ulong[] guildIds) => _commandBuilders.Add(CommandBuilder.From(commandDelegate, guildIds));

    public void AddCommand(Type type) => _commandBuilders.Add(CommandBuilder.From(type));
    public void AddCommand(Type type, params ulong[] guildIds) => _commandBuilders.Add(CommandBuilder.From(type, guildIds));

    public void AddCommand(CommandBuilder command) => _commandBuilders.Add(command);
    public void AddCommands(IEnumerable<CommandBuilder> commands) => _commandBuilders.AddRange(commands);
    public void AddCommands(params CommandBuilder[] commands) => _commandBuilders.AddRange(commands);

    public void AddCommands(Assembly assembly) => AddCommands(assembly.GetTypes());
    public void AddCommands(Assembly assembly, params ulong[] guildIds) => AddCommands(assembly.GetTypes(), guildIds);

    public void AddCommands(Type type) => AddCommands([type]);
    public void AddCommands(Type type, params ulong[] guildIds) => AddCommands([type], guildIds);

    public void AddCommands<T>() => _commandBuilders.Add(CommandBuilder.From<T>());
    public void AddCommands<T>(params ulong[] guildIds) => _commandBuilders.Add(CommandBuilder.From<T>(guildIds));

    public void AddCommands(IEnumerable<Type> types) => AddCommands(types, []);
    public void AddCommands(IEnumerable<Type> types, params ulong[] guildIds)
    {
        foreach (Type type in types)
        {
            if (type.GetCustomAttribute<CommandAttribute>() is not null)
            {
                _commandBuilders.Add(CommandBuilder.From(type, guildIds));
                continue;
            }

            foreach (MethodInfo method in type.GetMethods())
            {
                if (method.GetCustomAttribute<CommandAttribute>() is not null)
                {
                    _commandBuilders.Add(CommandBuilder.From(method, guildIds: guildIds));
                }
            }
        }
    }

    public async ValueTask AddProcessorAsync(ICommandProcessor processor)
    {
        _processors.Add(processor.GetType(), processor);
        await processor.ConfigureAsync(this);
    }

    public ValueTask AddProcessorsAsync(params ICommandProcessor[] processors) => AddProcessorsAsync((IEnumerable<ICommandProcessor>)processors);
    public async ValueTask AddProcessorsAsync(IEnumerable<ICommandProcessor> processors)
    {
        foreach (ICommandProcessor processor in processors)
        {
            _processors.Add(processor.GetType(), processor);
            await processor.ConfigureAsync(this);
        }
    }

    public TProcessor GetProcessor<TProcessor>() where TProcessor : ICommandProcessor => (TProcessor)_processors[typeof(TProcessor)];

    /// <summary>
    /// Adds all public checks from the provided assembly to the extension.
    /// </summary>
    public void AddChecks(Assembly assembly)
    {
        foreach (Type t in assembly.GetTypes())
        {
            if (t.GetInterface("DSharpPlus.Commands.ContextChecks.IContextCheck`1") is not null)
            {
                AddCheck(t);
            }
        }
    }

    /// <summary>
    /// Adds a new check to the extension.
    /// </summary>
    public void AddCheck<T>()
        where T : IContextCheck
        => AddCheck(typeof(T));

    /// <summary>
    /// Adds a new check to the extension.
    /// </summary>
    public void AddCheck(Type checkType)
    {
        // get all implemented check interfaces, we can pretty easily handle having multiple checks in one type
        foreach (Type t in checkType.GetInterfaces())
        {
            if (t.Namespace != "DSharpPlus.Commands.ContextChecks" || t.Name != "IContextCheck`1")
            {
                continue;
            }

            Type attributeType = t.GetGenericArguments()[0];

            MethodInfo method = checkType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(x => x.Name == "ExecuteCheckAsync" && x.GetParameters()[0].ParameterType == attributeType);

            // create the func for invoking the check here, during startup
            ParameterExpression check = Expression.Parameter(checkType);
            ParameterExpression attribute = Expression.Parameter(attributeType);
            ParameterExpression context = Expression.Parameter(typeof(CommandContext));

            MethodCallExpression call = Expression.Call
            (
                instance: check,
                method: method,
                arg0: attribute,
                arg1: context
            );

            Type delegateType = typeof(Func<,,,>).MakeGenericType(checkType, attributeType, typeof(CommandContext), typeof(ValueTask<string>));

            CheckFunc func = Unsafe.As<CheckFunc>(Expression.Lambda(delegateType, call, check, attribute, context).Compile());

            checks.Add
            (
                new()
                {
                    AttributeType = attributeType,
                    CheckType = checkType,
                    ExecuteCheckAsync = func
                }
            );
        }
    }

    /// <summary>
    /// Adds all parameter checks from the provided assembly to the extension.
    /// </summary>
    public void AddParameterChecks(Assembly assembly)
    {
        foreach (Type t in assembly.GetTypes())
        {
            if (t.GetInterface("DSharpPlus.Commands.ContextChecks.ParameterChecks.IParameterCheck`1") is not null)
            {
                AddParameterCheck(t);
            }
        }
    }

    /// <summary>
    /// Adds a new check to the extension.
    /// </summary>
    public void AddParameterCheck<T>()
        where T : IParameterCheck
        => AddParameterCheck(typeof(T));

    /// <summary>
    /// Adds a new check to the extension.
    /// </summary>
    public void AddParameterCheck(Type checkType)
    {
        // get all implemented check interfaces, we can pretty easily handle having multiple checks in one type
        foreach (Type t in checkType.GetInterfaces())
        {
            if (t.Namespace != "DSharpPlus.Commands.ContextChecks.ParameterChecks" || t.Name != "IParameterCheck`1")
            {
                continue;
            }

            Type attributeType = t.GetGenericArguments()[0];

            MethodInfo method = checkType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(x => x.Name == "ExecuteCheckAsync" && x.GetParameters()[0].ParameterType == attributeType);

            // create the func for invoking the check here, during startup
            ParameterExpression check = Expression.Parameter(checkType);
            ParameterExpression attribute = Expression.Parameter(attributeType);
            ParameterExpression info = Expression.Parameter(typeof(ParameterCheckInfo));
            ParameterExpression context = Expression.Parameter(typeof(CommandContext));

            MethodCallExpression call = Expression.Call
            (
                instance: check,
                method: method,
                arg0: attribute,
                arg1: info,
                arg2: context
            );

            Type delegateType = typeof(Func<,,,,>).MakeGenericType
            (
                checkType,
                attributeType,
                typeof(ParameterCheckInfo),
                typeof(CommandContext),
                typeof(ValueTask<string>)
            );

            ParameterCheckFunc func = Unsafe.As<ParameterCheckFunc>(Expression.Lambda(delegateType, call, check, attribute, info, context)
                .Compile());

            parameterChecks.Add
            (
                new()
                {
                    AttributeType = attributeType,
                    CheckType = checkType,
                    ExecuteCheckAsync = func
                }
            );
        }
    }

    public async Task RefreshAsync()
    {
        Dictionary<string, Command> commands = [];
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
        if (RegisterDefaultCommandProcessors)
        {
            _processors.TryAdd(typeof(TextCommandProcessor), new TextCommandProcessor());
            _processors.TryAdd(typeof(SlashCommandProcessor), new SlashCommandProcessor());
            _processors.TryAdd(typeof(MessageCommandProcessor), new MessageCommandProcessor());
            _processors.TryAdd(typeof(UserCommandProcessor), new UserCommandProcessor());
        }

        foreach (ICommandProcessor processor in _processors.Values)
        {
            await processor.ConfigureAsync(this);
        }
    }

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
            ChecksFailedException checksFailedException when checksFailedException.Errors.Count == 1 => $"The following error occurred: {Formatter.InlineCode(Formatter.Sanitize(checksFailedException.Errors[0].ErrorMessage))}",
            ChecksFailedException checksFailedException => $"The following context checks failed: {Formatter.InlineCode(Formatter.Sanitize(string.Join("\n\n", checksFailedException.Errors.Select(x => x.ErrorMessage))))}.",
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
                messageBuilder.WithContent(stringBuilder.ToString());
            }
            // If the exception message exceeds the message character limit, cram it all into an attatched file with a simple message in the content.
            else if (stringBuilder.Length >= 2000)
            {
                messageBuilder.WithContent("Exception Message exceeds character limit, see attached file.");
                string formattedFile = $"{stringBuilder}{Environment.NewLine}{Environment.NewLine}Stack Trace:{Environment.NewLine}{eventArgs.Exception.StackTrace}";
                messageBuilder.AddFile("MessageAndStackTrace.txt", new MemoryStream(Encoding.UTF8.GetBytes(formattedFile)), AddFileOptions.CloseStream);
            }
            // Otherwise, display the exception message in the content and the trace in an attached file
            else
            {
                messageBuilder.WithContent(stringBuilder.ToString());
                messageBuilder.AddFile("StackTrace.txt", new MemoryStream(Encoding.UTF8.GetBytes(eventArgs.Exception.StackTrace)), AddFileOptions.CloseStream);
            }
        }
        // If no stack trace, and the message is still too long, attatch a file with the message and use a simple message in the content.
        else if (stringBuilder.Length >= 2000)
        {
            messageBuilder.WithContent("Exception Message exceeds character limit, see attached file.");
            messageBuilder.AddFile("Message.txt", new MemoryStream(Encoding.UTF8.GetBytes(stringBuilder.ToString())), AddFileOptions.CloseStream);
        }
        // Otherwise, if no stack trace and the Exception message will fit, send the message as content
        else
        {
            messageBuilder.WithContent(stringBuilder.ToString());
        }

        await eventArgs.Context.RespondAsync(messageBuilder);
    }
}
