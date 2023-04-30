using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandHandler
{
    private MessageCommandModule _module = null!; // Will be set by the factory.
    private DiscordMessage? _newMessage;
    private DiscordMessage _message;
    private MessageCommandMethodData _data;
    private IServiceScope _scope;
    private DiscordClient _client;
    private Dictionary<string, string> _values;
    private CHConfiguration _configuration;
    private string _name;

    public MessageCommandHandler(DiscordMessage message, MessageCommandMethodData data, IServiceScope scope,
        DiscordClient client, Dictionary<string, string> values, CHConfiguration configuration, string name)
    {
        _message = message;
        _data = data;
        _scope = scope;
        _client = client;
        _values = values;
        _configuration = configuration;
        _name = name;
    }

    internal async Task TurnResultIntoActionAsync(IMessageCommandModuleResult result)
    {
        // [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void SetContentAndEmbeds(IMessageCommandModuleResult result, DiscordMessageBuilder messageBuilder)
        {
            if (result.Content is not null)
            {
                messageBuilder.WithContent(result.Content);
            }

            if (result.Embeds is not null)
            {
                messageBuilder.AddEmbeds(result.Embeds);
            }
        }

        DiscordMessageBuilder msgBuilder = new();
        switch (result.Type)
        {
            case MessageCommandModuleResultType.Empty: return;
            case MessageCommandModuleResultType.Reply:
                SetContentAndEmbeds(result, msgBuilder);
                msgBuilder.WithReply(_module.Message.Id, false);

                _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                break;
            case MessageCommandModuleResultType.NoMentionReply:
                SetContentAndEmbeds(result, msgBuilder);
                msgBuilder.WithReply(_module.Message.Id, true);

                _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                break;
            case MessageCommandModuleResultType.Send:
                SetContentAndEmbeds(result, msgBuilder);

                _newMessage = await _module.Message.Channel.SendMessageAsync(msgBuilder);
                break;
            case MessageCommandModuleResultType.FollowUp:
                SetContentAndEmbeds(result, msgBuilder);
                if (_newMessage is not null)
                {
                    msgBuilder.WithReply(_newMessage.Id);
                }

                await _module.Message.Channel.SendMessageAsync(msgBuilder);
                break;
            case MessageCommandModuleResultType.Edit:
                SetContentAndEmbeds(result, msgBuilder);
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
        MessageCommandModuleData moduleData = _data.Module;
        ConstructorInfo[] constructors = moduleData.Type.GetConstructors();
        if (constructors.Length != 0)
        {
            ConstructorInfo constructor = constructors[0];
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            object?[] constructorParams = new object[constructorParameters.Length];
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                Type type = constructorParameters[i].ParameterType;
                constructorParams[i] = _scope.ServiceProvider.GetService(type);
            }

            _module = (Activator.CreateInstance(moduleData.Type, constructorParams) as MessageCommandModule)!;
        }
        else
        {
            _module = (Activator.CreateInstance(moduleData.Type, null) as MessageCommandModule)!;
        }

        _module.Message = _message;
        _module._handler = this;
        _module.Client = _client;

        object?[]? parameters = null;
        if (_data.Parameters.Count != 0)
        {
            MessageConvertValues conversion = new(_values, _data, _message, _client);
            parameters = await conversion.StartConversionAsync();
        }

        if (_configuration.Middlewares.Count != 0)
        {
            MessageMiddlewareHandler middlewareHandler = new(_configuration.Middlewares);
            bool shouldContinue =
                await middlewareHandler.StartGoingThroughMiddlewaresAsync(new MessageContext
                {
                    Message = _message, Data = new(_name, _data.Method)
                }, _scope);
            if (!shouldContinue)
            {
                return;
            } 
        }
        
        
        if (_data.IsAsync)
        {
            Task<IMessageCommandModuleResult> task =
                (Task<IMessageCommandModuleResult>)_data.Method.Invoke(_module, parameters)!;
            IMessageCommandModuleResult result = await task;
            await TurnResultIntoActionAsync(result);
        }
        else
        {
            IMessageCommandModuleResult result = (IMessageCommandModuleResult)_data.Method.Invoke(_module, parameters)!;
            await TurnResultIntoActionAsync(result);
        }
    }
}
