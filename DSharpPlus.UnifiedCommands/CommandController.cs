using System.Linq.Expressions;
using System.Reflection;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.UnifiedCommands.Application.Internals;
using DSharpPlus.UnifiedCommands.Exceptions;
using DSharpPlus.UnifiedCommands.Internals;
using DSharpPlus.UnifiedCommands.Message;
using DSharpPlus.UnifiedCommands.Message.Conditions;
using DSharpPlus.UnifiedCommands.Message.Internals;
using DSharpPlus.UnifiedCommands.Internals.Trees;
using Remora.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.UnifiedCommands;

using ConverterLambda = Func<IServiceProvider, DiscordClient, DiscordMessage, ArraySegment<char>, ValueTask<IResult>>;
using ConversionLambda = Func<IResult, object?>;

public sealed class CommandController
{
    // Static info for getting servince info 
    private static readonly MethodInfo _getServiceInfo = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService),
                            BindingFlags.Instance | BindingFlags.Public)!;

    private readonly string[] _prefixes;
    private readonly List<DiscordApplicationCommand> _commands = new();
    private readonly ulong[]? _guildIds;

    internal static readonly Dictionary<Type, (ConverterLambda, ConversionLambda)> ConverterList = new();
    internal MessageFactory MessageFactory { get; init; }
    internal ApplicationFactory ApplicationFactory { get; init; }

    public IServiceProvider Services { get; init; }

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

        using IServiceScope scope = Services.CreateScope();
        foreach (ITreeChild<MessageMethodData> child in MessageFactory.GetTree().List)
        {
            ValidateMessageCommands(child, scope);
        }

        client.MessageCreated += HandleMessageCreationAsync;
        client.InteractionCreated += HandleInteractionCreateAsync;
        client.MessageReactionAdded += MessageReactionHandler.MessageReactionEventAsync;
        client.Ready += HandleReadyAsync;
    }

    private void ValidateMessageCommands(ITreeChild<MessageMethodData> child, IServiceScope scope)
    {
        if (child.Value is not null)
        {
            MessageMethodData data = child.Value;
            foreach (MessageParameterData param in data.Parameters)
            {
                if (scope.ServiceProvider.GetService(param.ConverterType) is null)
                {
                    throw new Exception($"Couldn't find converter {param.ConverterType.Name} for parameter name {param.Name} in method {data.Method.Name}");
                }
                else
                {
                    if (!ConverterList.ContainsKey(param.ConverterType))
                    {
                        List<Expression> expressions = new(2);

                        // Gets all required parameters
                        ConstantExpression converterType = Expression.Constant(param.ConverterType, typeof(Type));
                        ParameterExpression serviceProvider = Expression.Parameter(typeof(IServiceProvider));
                        ParameterExpression client = Expression.Parameter(typeof(DiscordClient));
                        ParameterExpression message = Expression.Parameter(typeof(DiscordMessage));
                        ParameterExpression segment = Expression.Parameter(typeof(ArraySegment<char>));

                        // Makes a call to the service provider to get the converter type
                        MethodCallExpression serviceMethodCall = Expression.Call(serviceProvider, _getServiceInfo, new[] { converterType });
                        expressions.Add(serviceMethodCall);

                        // Makes a call to the converter ConvertValueAsync method
                        UnaryExpression convertConverter = Expression.Convert(serviceMethodCall, param.ConverterType);
                        MethodCallExpression converterMethodCall = Expression.Call(convertConverter,
                            param.ConverterType.GetMethod(nameof(IMessageConverter<object>.ConvertValueAsync))!,
                            new[] { client, message, segment }
                        );
                        expressions.Add(converterMethodCall);

                        // Compiles it
                        ConverterLambda converterLambda = Expression.Lambda<ConverterLambda>(
                                Expression.Block(expressions), false, serviceProvider, client, message, segment
                            ).Compile();

                        // Code that converts the result returned by 
                        expressions.Clear();

                        // Constants and parameters init
                        Type resultType = typeof(Result<>).MakeGenericType(param.Type);
                        ParameterExpression resultParam = Expression.Parameter(typeof(IResult));

                        // Converts value into the result type
                        UnaryExpression convertExpression = Expression.Convert(resultParam, resultType);
                        MemberExpression memberExpression = Expression.Property(convertExpression, resultType.GetProperty("Entity")!);

                        // This is suppose to be a nullable object (object?) but typeof(object?) is invalid code
                        UnaryExpression convertResultExpression = Expression.Convert(memberExpression, typeof(object));
                        expressions.Add(convertExpression);
                        expressions.Add(memberExpression);
                        expressions.Add(convertResultExpression);

                        // Compiles it
                        ConversionLambda conversionLambda = 
                            Expression.Lambda<ConversionLambda>(Expression.Block(expressions), false, resultParam).Compile();

                        ConverterList.Add(param.ConverterType, (converterLambda, conversionLambda));
                    }
                }
            }
        }

        if (child is ITreeParent<MessageMethodData> m)
        {
            foreach (ITreeChild<MessageMethodData> treeChild in m.List)
            {
                ValidateMessageCommands(treeChild, scope);
            }
        }
    }

    public CommandController UseMessageCondition<T>() where T : IMessageCondition
    {
        // This code builds a lambda that returns a IServiceProvider class. ObjectFactory can be used instead of course.
        // But this is here more for 3ns micro performance. Can always be changed into a ObjectFactory if it is more convenient.
        Type type = typeof(T);
        List<Expression> expressions = new();

        // Adds a parameter for the service provider.
        ParameterExpression serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

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
            // Adds all the call expression used for parameters when calling the constructor.
            List<Expression> parameters = new();
            foreach (ParameterInfo parameter in _getServiceInfo.GetParameters())
            {
                parameters.Add(Expression.Call(_getServiceInfo, Expression.Constant(typeof(Type),
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
            content, ranges);
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
