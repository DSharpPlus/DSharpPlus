using System.Reflection;
using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Message.Conditions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.ExceptionServices;
#if DEBUG
using Stopwatch = System.Diagnostics.Stopwatch;
#endif
using Remora.Results;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageHandler
{
    private MessageModule _module = null!; // Will be set by the factory.
    private DiscordMessage? _newMessage;
    private readonly DiscordMessage _message;
    private readonly MessageMethodData _data;
    private readonly IServiceScope _scope;
    private readonly DiscordClient _client;
    private readonly IReadOnlyList<(Type, ArraySegment<char>?)> _values;
    private readonly string _name;
    private readonly IReadOnlyList<Func<IServiceProvider, IMessageCondition>> _conditionBuilders;

    public MessageHandler(DiscordMessage message, MessageMethodData data, IServiceScope scope,
        DiscordClient client, IReadOnlyList<(Type, ArraySegment<char>?)> values, string name,
        IReadOnlyList<Func<IServiceProvider, IMessageCondition>> conditionBuilders)
    {
        _message = message;
        _data = data;
        _scope = scope;
        _client = client;
        _values = values;
        _name = name;
        _conditionBuilders = conditionBuilders;
    }

    internal async Task TurnResultIntoActionAsync(IMessageResult result)
    {
        switch (result.Type)
        {
            case MessageResultType.Empty: return;
            case MessageResultType.Reply:
                result.Builder.WithReply(_module.Message.Id);

                _newMessage = await _module.Message.Channel.SendMessageAsync(result.Builder);
                _module.NewestMessage = _newMessage;
                break;
            case MessageResultType.NoMentionReply:
                result.Builder.WithReply(_module.Message.Id, true);

                _newMessage = await _module.Message.Channel.SendMessageAsync(result.Builder);
                _module.NewestMessage = _newMessage;
                break;
            case MessageResultType.Send:

                _newMessage = await _module.Message.Channel.SendMessageAsync(result.Builder);
                _module.NewestMessage = _newMessage;
                break;
            case MessageResultType.FollowUp:
                if (_newMessage is not null)
                {
                    result.Builder.WithReply(_newMessage.Id);
                }

                await _module.Message.Channel.SendMessageAsync(result.Builder);
                break;
            case MessageResultType.Edit:
                if (_newMessage is not null)
                {
                    await _newMessage.ModifyAsync(result.Builder);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal async ValueTask BuildModuleAndExecuteCommandAsync()
    {
        try
        {
#if DEBUG
            long startTime = Stopwatch.GetTimestamp();
#endif

            MessageModuleData moduleData = _data.Module;

            _module = (MessageModule)moduleData.Factory.Invoke(_scope.ServiceProvider, null);
            _module.Message = _message;
            _module._handler = this;
            _module.Client = _client;

            object?[]? parameters = null;
            if (_data.Parameters.Count != 0)
            {
                MessageConvertValues conversion = new(_values, _data, _message, _client, _scope);

                Result<object?[]> parametersResult = await conversion.StartConversionAsync();
                if (parametersResult.IsSuccess)
                {
                    parameters = parametersResult.Entity;
                }
                else
                {
                    IErrorHandler errorHandler = _scope.ServiceProvider.GetRequiredService<IErrorHandler>();
                    await errorHandler.HandleMessageErrorAsync(parametersResult.Error, _message, _client);
                    return;
                }
            }

            if (_conditionBuilders.Count != 0)
            {
                MessageConditionHandler conditionHandler =
                    new(_conditionBuilders);
                bool shouldContinue =
                    await conditionHandler.IterateConditionsAsync(
                        new MessageContext { Message = _message, Data = new(_name, _data.Method), Client = _client },
                        _scope);
                if (!shouldContinue)
                {
                    return;
                }
            }

#if DEBUG
            TimeSpan elapsedTime = Stopwatch.GetElapsedTime(startTime);
            _client.Logger.LogDebug("Took {NsExecution}ns to process everything", elapsedTime.TotalNanoseconds);
#endif

            try
            {
                if (_data.ReturnsNothing)
                {
                    if (_data.IsAsync)
                    {
                        // TODO: Add support for ValueTask invoking
                        await (Task)_data.Method.Invoke(_module, BindingFlags.OptionalParamBinding,
                            null, parameters, null)!;
                    }
                    else
                    {
                        _data.Method.Invoke(_module, BindingFlags.OptionalParamBinding, null, parameters, null);
                    }
                }
                else
                {
                    if (_data.IsAsync)
                    {
                        Task<IMessageResult> task =
                            (Task<IMessageResult>)_data.Method.Invoke(_module,
                                BindingFlags.OptionalParamBinding, null,
                                parameters, null)!;
                        IMessageResult result = await task;
                        await TurnResultIntoActionAsync(result);
                    }
                    else
                    {
                        IMessageResult result =
                            (IMessageResult)_data.Method.Invoke(_module, parameters)!;
                        await TurnResultIntoActionAsync(result);
                    }
                }
            }
            catch (TargetInvocationException e)
            {
                // ExceptionDispatchInfo can be used to rethrow a error and perceive all the info in it.
                // This is done here to rethrow the inner error and show where it is so people who don't read in depth their stacktrace doesn't blame us.
                ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw e.InnerException;
            }
            catch (AggregateException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw e.InnerException;
            }
        }
        catch (Exception e)
        {
            await _scope.ServiceProvider.GetRequiredService<IErrorHandler>()
                .HandleMessageErrorAsync(new ExceptionError(e), _message, _client);

            _client.Logger.LogError(e,
                "Exception was thrown while trying to execute command {MethodName}. Was thrown from:",
                _data.Method.Name);
        }
        finally
        {
            _scope.Dispose();
        }
    }
}
