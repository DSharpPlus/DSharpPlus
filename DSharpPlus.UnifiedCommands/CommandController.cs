using System.Linq.Expressions;
using System.Reflection;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.UnifiedCommands.Application.Conditions;
using DSharpPlus.UnifiedCommands.Application.Internals;
using DSharpPlus.UnifiedCommands.Exceptions;
using DSharpPlus.UnifiedCommands.Internals;
using DSharpPlus.UnifiedCommands.Message.Conditions;
using DSharpPlus.UnifiedCommands.Message.Internals;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.UnifiedCommands;

public class CommandController
{
    private readonly string[] _prefixes;
    private readonly List<DiscordApplicationCommand> _commands = new();
    private readonly ulong[]? _guildIds;

    internal MessageFactory MessageFactory { get; private set; }
    internal ApplicationFactory ApplicationFactory { get; private set; }

    public IServiceProvider Services { get; private set; }

    public CommandController(DiscordClient client, IServiceProvider services,
        IReadOnlyCollection<Assembly> assemblies, string[] prefixes, ulong[]? guildIds, bool registerSlashCommands)
    {
        _prefixes = prefixes;
        Services = services;
        _guildIds = guildIds;
        
        MessageFactory = new MessageFactory(Services);
        ApplicationFactory = new ApplicationFactory(Services);

        CommandModuleRegister.RegisterMessageCommands(MessageFactory, assemblies);
        if (registerSlashCommands)
        {
            _commands = CommandModuleRegister.RegisterApplicationCommands(ApplicationFactory, assemblies, client);
        }

        client.MessageCreated += HandleMessageCreationAsync;
        client.InteractionCreated += HandleInteractionCreateAsync;
        client.MessageReactionAdded += MessageReactionHandler.MessageReactionEventAsync;
        client.Ready += HandleReadyAsync;
    }

    public CommandController UseMessageCondition<T>() where T : IMessageCondition
    {
        // This code builds a lambda that returns a IServiceProvider class. ObjectFactory can be used instead of course.
        // But this is here more for 3ns micro performance. Can always be changed into a ObjectFactory if it is more convenient.
        Type type = typeof(T);
        List<Expression> expressions = new();

        // Adds a parameter for the service provider.
        ParameterExpression serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        expressions.Add(serviceProviderParam);

        // Gets the first constructor. There will always be one constructor.
        ConstructorInfo info = type.GetConstructors()[0];

        if (!info.IsPublic)
        {
            throw new InvalidConditionException("The first constructor in the condition needs to be public.");
        }

        if (info.GetParameters().Length == 0)
        {
            // Creates the object and returns it.
            expressions.Add(Expression.New(info));
        }
        else
        {
            // Gets the method for getting a service.
            MethodInfo method = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService),
                BindingFlags.Instance | BindingFlags.Public)!;

            // Adds all the call expression used for parameters when calling the constructor.
            List<Expression> parameters = new();
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                parameters.Add(Expression.Call(method, Expression.Constant(typeof(Type),
                    parameter.ParameterType)));
            }

            // Calls the constructor with the named parameters and returns it.
            expressions.Add(Expression.New(info, parameters));
        }

        // Builds the expression into IR that then can be executed as a lambda. And then adds that to the message condition builder.
        Func<IServiceProvider, IMessageCondition> func =
            Expression.Lambda<Func<IServiceProvider, IMessageCondition>>(Expression.Block(expressions), false,
                serviceProviderParam).Compile();
        MessageFactory.AddMessageConditionBuilder(func);
        return this;
    }

    internal Task HandleMessageCreationAsync(DiscordClient client, MessageCreateEventArgs msg)
    {
        if (_prefixes.Length == 0)
        {
            return Task.CompletedTask;
        }

        if (msg.Author.IsBot)
        {
            return Task.CompletedTask;
        }

        ReadOnlySpan<char> content = msg.Message.Content.AsSpan();
        if (content.Length == 0)
        {
            return Task.CompletedTask;
        }

        string? prefix = null;
        foreach (string prefixCandidate in _prefixes)
        {
            if (content.StartsWith(prefixCandidate))
            {
                prefix = prefixCandidate;
                break;
            }
        }

        if (prefix is null)
        {
            return Task.CompletedTask;
        }

        // TODO: Use new .NET 8 type when updating to .NET 8
        List<Range> ranges = new();
        Index last = prefix.Length;
        for (int i = last.Value; i < content.Length; i++)
        {
            if (content[i] == ' ')
            {
                ranges.Add(new Range(last, i));
                last = i + 1;
            }
        }

        ranges.Add(new Range(last, content.Length));

        MessageFactory.ConstructAndExecuteCommand(msg.Message, client,
            ref content, ranges);
        return Task.CompletedTask;
    }

    internal Task HandleInteractionCreateAsync(DiscordClient client, InteractionCreateEventArgs e)
    {
        ApplicationFactory.ExecuteCommand(e.Interaction, client);
        return Task.CompletedTask;
    }

    internal async Task HandleReadyAsync(DiscordClient client, ReadyEventArgs e)
    {
        try
        {
            if (_guildIds is null)
            {
                await client.BulkOverwriteGlobalApplicationCommandsAsync(_commands);
                client.Logger.LogTrace("Registered commands");
            }
            else
            {
                foreach (ulong guildId in _guildIds)
                {
                    await client.BulkOverwriteGuildApplicationCommandsAsync(guildId, _commands);
                    client.Logger.LogTrace("Registered commands in guild {GuildId}", guildId);
                }
            }
        }
        catch (BadRequestException exception)
        {
            client.Logger.LogError(exception, "Error message: ");
        }
    }
}
