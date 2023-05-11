using System.Reflection;
using System.Linq.Expressions;
using DSharpPlus.CH.Application.Internals;
using DSharpPlus.CH.Message.Conditions;
using DSharpPlus.EventArgs;
using DSharpPlus.CH.Internals;
using DSharpPlus.CH.Message.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.CH;

public class CommandController
{
    private string[] _prefixes;
    
    internal MessageCommandFactory _messageCommandFactory { get; private set; }
    internal ApplicationFactory _applicationFactory { get; private set; }
    
    public IServiceProvider Services { get; private set; }

    public CommandController(DiscordClient client, IServiceProvider services,
        Assembly assembly, string[] prefixes)
    {
        _prefixes = prefixes;
        Services = services;

        _messageCommandFactory = new MessageCommandFactory(Services);
        _applicationFactory = new ApplicationFactory(Services);
        
        CommandModuleRegister.RegisterMessageCommands(_messageCommandFactory, assembly);
        CommandModuleRegister.RegisterApplicationCommands(_applicationFactory, assembly, client);
        
        client.MessageCreated += HandleMessageCreationAsync;
        client.InteractionCreated += HandleInteractionCreateAsync;
        client.MessageReactionAdded += Message.Internals.MessageReactionHandler.MessageReactionEventAsync;
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
        _messageCommandFactory.AddMessageConditionBuilder(func);
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

        _messageCommandFactory.ConstructAndExecuteCommand(msg.Message, client,
            ref content, ranges);
        return Task.CompletedTask;
    }

    internal Task HandleInteractionCreateAsync(DiscordClient client, InteractionCreateEventArgs e)
    {
        _applicationFactory.ExecuteCommand(e.Interaction, client);
        return Task.CompletedTask;
    }
}
