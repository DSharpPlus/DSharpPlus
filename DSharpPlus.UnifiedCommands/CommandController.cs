using System.Linq.Expressions;
using System.Reflection;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.UnifiedCommands.Application.Internals;
using DSharpPlus.UnifiedCommands.Internals;
using DSharpPlus.UnifiedCommands.Message.Conditions;
using DSharpPlus.UnifiedCommands.Message.Internals;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.UnifiedCommands;

public class CommandController
{
    private string[] _prefixes;
    private List<DiscordApplicationCommand> _commands = new();
    private ulong[]? _guildIds;

    internal MessageFactory MessageFactory { get; private set; }
    internal ApplicationFactory _applicationFactory { get; private set; }
    
    public IServiceProvider Services { get; private set; }

    public CommandController(DiscordClient client, IServiceProvider services,
        Assembly assembly, string[] prefixes, ulong[]? guildIds, bool registerSlashCommands)
    {
        _prefixes = prefixes;
        Services = services;

        MessageFactory = new MessageFactory(Services);
        _applicationFactory = new ApplicationFactory(Services);
        
        CommandModuleRegister.RegisterMessageCommands(MessageFactory, assembly);
        if (registerSlashCommands)
        {
            _commands = CommandModuleRegister.RegisterApplicationCommands(_applicationFactory, assembly, client);
        }

        client.MessageCreated += HandleMessageCreationAsync;
        client.InteractionCreated += HandleInteractionCreateAsync;
        client.MessageReactionAdded += MessageReactionHandler.MessageReactionEventAsync;
        client.Ready += HandleReadyAsync;
    }

    public CommandController UseMessageCondition<T>() where T : IMessageCondition
    {
        Type type = typeof(T);
        List<Expression> expressions = new();

        ParameterExpression serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        expressions.Add(serviceProviderParam);
        ConstructorInfo info = type.GetConstructors()[0];
        if (info.GetParameters().Length == 0)
        {
            expressions.Add(Expression.New(info));
        }
        else
        {
            MethodInfo method = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService),
                BindingFlags.Instance | BindingFlags.Public)!;

            List<Expression> parameters = new();
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                parameters.Add(Expression.Call(method, Expression.Constant(typeof(Type),
                    parameter.ParameterType)));
            }

            expressions.Add(Expression.New(info, parameters));
        }

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
        foreach (string maybeCorrectPrefix in _prefixes)
        {
            if (content.StartsWith(maybeCorrectPrefix))
            {
                prefix = maybeCorrectPrefix;
                break;
            }
        }

        if (prefix is null)
        {
            return Task.CompletedTask;
        }
        
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
        _applicationFactory.ExecuteCommand(e.Interaction, client);
        return Task.CompletedTask;
    }

    internal async Task HandleReadyAsync(DiscordClient client, ReadyEventArgs e)
    {
        if (_guildIds is null)
        {
            await client.BulkOverwriteGlobalApplicationCommandsAsync(_commands);
                    client.Logger.LogInformation("Registered commands");
        }
        else
        {
            foreach (ulong guildId in _guildIds)
            {
                await client.BulkOverwriteGuildApplicationCommandsAsync(guildId, _commands);
                client.Logger.LogTrace("Registered commands in guild {GuildId}", guildId);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
