using System.Reflection;
using DSharpPlus.CH.Message.Conditions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandHandler
{
    private MessageCommandModule _module = null!; // Will be set by the factory.
    private DiscordMessage? _newMessage;
    private DiscordMessage _message;
    private MessageCommandMethodData _data;
    private IServiceScope _scope;
    private DiscordClient _client;
    private Dictionary<string, string?> _values;
    private string _name;
    private readonly IReadOnlyList<Func<IServiceProvider, IMessageCondition>> _conditionBuilders;

    public MessageCommandHandler(DiscordMessage message, MessageCommandMethodData data, IServiceScope scope,
        DiscordClient client, Dictionary<string, string?> values, string name,
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

    internal async Task TurnResultIntoActionAsync(IMessageCommandResult result)
    {
        DiscordMessageBuilder msgBuilder = new();
        if (result.Type != MessageCommandResultType.Empty)
        {
            if (result.Content is not null)
            {
                msgBuilder.WithContent(result.Content);
            }

            if (result.Embeds is not null)
            {
                msgBuilder.AddEmbeds(result.Embeds);
            }
        }
        
        switch (result.Type)
        {
            case MessageCommandResultType.Empty: return;
            case MessageCommandResultType.Reply:
                msgBuilder.WithReply(_module.Message.Id);

                _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                _module.NewestMessage = _newMessage;
                break;
            case MessageCommandResultType.NoMentionReply:
                msgBuilder.WithReply(_module.Message.Id, true);

                _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                _module.NewestMessage = _newMessage;
                break;
            case MessageCommandResultType.Send:

                _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                _module.NewestMessage = _newMessage;
                break;
            case MessageCommandResultType.FollowUp:
                if (_newMessage is not null)
                {
                    msgBuilder.WithReply(_newMessage.Id);
                }

                await _module.Message.Channel.SendMessageAsync(msgBuilder);
                break;
            case MessageCommandResultType.Edit:
                if (_newMessage is not null)
                {
                    await _newMessage.ModifyAsync(msgBuilder);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal async Task BuildModuleAndExecuteCommandAsync()
    {
        try
        {
            long startTime = Stopwatch.GetTimestamp();

            MessageCommandModuleData moduleData = _data.Module;

            _module = (MessageCommandModule)moduleData.Factory.Invoke(_scope.ServiceProvider, null);
            _module.Message = _message;
            _module._handler = this;
            _module.Client = _client;

            object?[]? parameters = null;
            if (_data.Parameters.Count != 0)
            {
                MessageConvertValues conversion = new(_values, _data, _message, _client);
                try
                {
                    parameters = await conversion.StartConversionAsync();
                }
                catch (Exceptions.ConversionFailedException e)
                {
                    await _scope.ServiceProvider.GetRequiredService<IErrorHandler>().HandleConversionAsync(
                        new InvalidMessageConvertionError
                        {
                            Name = e.Name,
                            IsPositionalArgument = e.IsPositionalArgument,
                            Value = e.Value,
                            Type = e.Type,
                        }
                        , _message);
                    return;
                }
            }

            if (_conditionBuilders.Count != 0)
            {
                MessageConditionHandler conditionHandler =
                    new(_conditionBuilders);
                bool shouldContinue =
                    await conditionHandler.StartGoingThroughConditionsAsync(
                        new MessageContext { Message = _message, Data = new(_name, _data.Method), Client = _client },
                        _scope);
                if (!shouldContinue)
                {
                    return;
                }
            }

            TimeSpan elapsedTime = Stopwatch.GetElapsedTime(startTime);
            _client.Logger.LogDebug("Took {NsExecution}ns to process everything", elapsedTime.TotalNanoseconds);

            try
            {
                if (_data.ReturnsNothing)
                {
                    if (_data.IsAsync)
                    {
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
                        Task<IMessageCommandResult> task =
                            (Task<IMessageCommandResult>)_data.Method.Invoke(_module,
                                BindingFlags.OptionalParamBinding, null,
                                parameters, null)!;
                        IMessageCommandResult result = await task;
                        await TurnResultIntoActionAsync(result);
                    }
                    else
                    {
                        IMessageCommandResult result =
                            (IMessageCommandResult)_data.Method.Invoke(_module, parameters)!;
                        await TurnResultIntoActionAsync(result);
                    }
                }
            }
            catch (TargetInvocationException e)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw e.InnerException;
            }
            catch (AggregateException e)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw e.InnerException;
            }
        }
        catch (Exception e)
        {
            await _scope.ServiceProvider.GetRequiredService<IErrorHandler>()
                .HandleUnhandledExceptionAsync(e, _message);

            _client.Logger.LogError(
                "Exception was thrown while trying to execute command {MethodName}. Was thrown from {Exception}",
                _data.Method.Name, e.ToString());
        }
        finally
        {
            _scope.Dispose();
        }
    }
}
