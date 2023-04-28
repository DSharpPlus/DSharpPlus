using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandHandler
{
    private MessageCommandModule _module = null!; // Will be set by the factory.
    private DiscordMessage? _newMessage;

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

        DiscordMessageBuilder? msgBuilder = new();
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
        }
    }

    internal async Task BuildModuleAndExecuteCommandAsync(MessageCommandMethodData data, IServiceScope scope,
        DiscordMessage message, DiscordClient client, Dictionary<string, object> options, List<string> arguments)
    {
        MessageCommandModuleData? moduleData = data.Module;
        ConstructorInfo[]? constructors = moduleData.Type.GetConstructors();
        if (constructors is not null && constructors.Length != 0)
        {
            ConstructorInfo? constructor = constructors[0];
            ParameterInfo[]? constructorParameters = constructor.GetParameters();
            object?[]? constructorParams = new object[constructorParameters.Length];
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                Type? type = constructorParameters[i].ParameterType;
                constructorParams[i] = scope.ServiceProvider.GetService(type);
            }

            _module = (Activator.CreateInstance(moduleData.Type, constructorParams) as MessageCommandModule)!;
        }
        else
        {
            _module = (Activator.CreateInstance(moduleData.Type, null) as MessageCommandModule)!;
        }

        _module.Message = message;
        _module._handler = this;
        _module.Client = client;

        object?[]? parameters = null;
        if (data.Parameters.Count != 0)
        {
            int argumentPosition = 0;
            parameters = new object?[data.Parameters.Count];
            for (int i = 0; i < data.Parameters.Count; i++)
            {
                MessageCommandParameterData? parameter = data.Parameters[i];

                if (parameter.IsPositionalArgument)
                {
                    try
                    {
                        parameters[i] = await ParseParameterAsync(parameter, arguments[argumentPosition], client);
                        argumentPosition++;
                    }
                    catch (Exceptions.ConvertionFailedException e)
                    {
                        InvalidMessageConvertionError? error = new()
                        {
                            Value = e.Value,
                            Type = e.Type,
                            Name = parameter.Name,
                            IsPositionalArgument = parameter.IsPositionalArgument
                        };
                        await scope.ServiceProvider.GetRequiredService<IFailedConvertion>()
                            .HandleErrorAsync(error, _module.Message);
                        return;
                    }
                }
                else
                {
                    if (options.TryGetValue(parameter.Name, out object? value))
                    {
                        try
                        {
                            parameters[i] = await ParseParameterAsync(parameter, value, client);
                        }
                        catch (Exceptions.ConvertionFailedException e)
                        {
                            InvalidMessageConvertionError? error = new()
                            {
                                Value = e.Value,
                                Type = e.Type,
                                Name = parameter.Name,
                                IsPositionalArgument = parameter.IsPositionalArgument
                            };
                            await scope.ServiceProvider.GetRequiredService<IFailedConvertion>()
                                .HandleErrorAsync(error, _module.Message);
                            return;
                        }
                    }
                    else if (parameter.ShorthandOptionName is not null &&
                             options.TryGetValue(parameter.ShorthandOptionName, out object? val))
                    {
                        try
                        {
                            parameters[i] = await ParseParameterAsync(parameter, val, client);
                        }
                        catch (Exceptions.ConvertionFailedException e)
                        {
                            InvalidMessageConvertionError? error = new()
                            {
                                Value = e.Value,
                                Type = e.Type,
                                Name = parameter.Name,
                                IsPositionalArgument = parameter.IsPositionalArgument
                            };
                            await scope.ServiceProvider.GetRequiredService<IFailedConvertion>()
                                .HandleErrorAsync(error, _module.Message);
                            return;
                        }
                    }
                    else if (parameter.Type == MessageCommandParameterDataType.Bool)
                    {
                        parameters[i] = false;
                    }
                    else if (parameter.CanBeNull)
                    {
                        parameters[i] = null;
                    }
                }
            }
        }

        if (data.IsAsync)
        {
            Task<IMessageCommandModuleResult>? task =
                (Task<IMessageCommandModuleResult>)data.Method.Invoke(_module, parameters)!;
            IMessageCommandModuleResult? result = await task;
            await TurnResultIntoActionAsync(result);
        }
        else
        {
            IMessageCommandModuleResult? result = (IMessageCommandModuleResult)data.Method.Invoke(_module, parameters)!;
            await TurnResultIntoActionAsync(result);
        }
    }

    private async Task<object> ParseParameterAsync(MessageCommandParameterData data, object value, DiscordClient client)
    {
        object obj = new();

        if (data.Type == MessageCommandParameterDataType.Bool && value.GetType() != typeof(bool))
        {
            throw new Exceptions.ConvertionFailedException(((bool)value).ToString(),
                InvalidMessageConvertionType.BoolShouldNotHaveValue);
        }
        else if (data.Type != MessageCommandParameterDataType.Bool && value.GetType() == typeof(bool))
        {
            throw new Exceptions.ConvertionFailedException(((bool)value).ToString(),
                InvalidMessageConvertionType.NoValueProvided);
        }

        switch (data.Type)
        {
            case MessageCommandParameterDataType.String:
                obj = value;
                break;
            case MessageCommandParameterDataType.Int:
            {
                if (int.TryParse((string)value, out int result))
                {
                    obj = result;
                }
                else
                {
                    throw new Exceptions.ConvertionFailedException((string)value,
                        InvalidMessageConvertionType.NotAValidInteger);
                }

                break;
            }
            case MessageCommandParameterDataType.Double:
            {
                if (double.TryParse((string)value, out double result))
                {
                    obj = result;
                }

                break;
            }
            case MessageCommandParameterDataType.User:
            {
                string str = (string)value;

                if (str.StartsWith("<@"))
                {
                    if (str.Length >= 3 && str[2] == '!')
                    {
                        str = str.Remove(0, 3);
                    }
                    else
                    {
                        str = str.Remove(0, 2);
                    }

                    str = str.Remove(str.Length - 1, 1);
                    if (ulong.TryParse(str, out ulong result))
                    {
                        try
                        {
                            obj = await client.GetUserAsync(result);
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.UserDoesNotExist);
                        }
                        catch (DSharpPlus.Exceptions.ServerErrorException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.UserDoesNotExist);
                        }
                    }
                    else
                    {
                        throw new Exceptions.ConvertionFailedException((string)value,
                            InvalidMessageConvertionType.NotAValidUser);
                    }
                }
                else
                {
                    if (ulong.TryParse((string)value, out ulong result))
                    {
                        obj = await client.GetUserAsync(result);
                    }
                    else
                    {
                        throw new Exceptions.ConvertionFailedException((string)value,
                            InvalidMessageConvertionType.NotAValidUser);
                    }
                }

                break;
            }
            case MessageCommandParameterDataType.Member:
            {
                if (_module.Message.Channel.GuildId is null)
                {
                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.IsInDMs);
                }

                string str = (string)value;
                if (str.StartsWith("<@"))
                {
                    if (str.Length >= 3 && str[2] == '!')
                    {
                        str = str.Remove(0, 3);
                    }
                    else
                    {
                        str = str.Remove(0, 2);
                    }

                    str = str.Remove(str.Length - 1, 1);
                    if (ulong.TryParse(str, out ulong result))
                    {
                        try
                        {
                            obj = await _module.Message.Channel.Guild.GetMemberAsync(ulong.Parse(str));
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.MemberDoesNotExist);
                        }
                        catch (DSharpPlus.Exceptions.ServerErrorException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.MemberDoesNotExist);
                        }
                    }
                    else
                    {
                        throw new Exceptions.ConvertionFailedException((string)value,
                            InvalidMessageConvertionType.NotAValidMember);
                    }
                }
                else
                {
                    if (ulong.TryParse((string)value, out ulong result))
                    {
                        try
                        {
                            obj = await _module.Message.Channel.Guild.GetMemberAsync(result);
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.MemberDoesNotExist);
                        }
                        catch (DSharpPlus.Exceptions.ServerErrorException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.MemberDoesNotExist);
                        }
                    }
                    else
                    {
                        throw new Exceptions.ConvertionFailedException((string)value,
                            InvalidMessageConvertionType.NotAValidMember);
                    }
                }


                break;
            }
            case MessageCommandParameterDataType.Channel:
            {
                if (_module.Message.Channel.GuildId is null)
                {
                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.IsInDMs);
                }

                string str = (string)value;

                if (str.StartsWith("<#"))
                {
                    if (str.Length >= 3 && str[2] == '!')
                    {
                        str = str.Remove(0, 3);
                    }
                    else
                    {
                        str = str.Remove(0, 2);
                    }

                    str = str.Remove(str.Length - 1, 1);

                    if (ulong.TryParse(str, out ulong result))
                    {
                        try
                        {
                            obj = _module.Message.Channel.Guild.GetChannel(result);
                        }
                        catch (DSharpPlus.Exceptions.ServerErrorException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.ChannelDoesNotExist);
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.ChannelDoesNotExist);
                        }
                    }
                    else
                    {
                        throw new Exceptions.ConvertionFailedException((string)value,
                            InvalidMessageConvertionType.NotAValidChannel);
                    }
                }

                break;
            }
            case MessageCommandParameterDataType.Bool:
                obj = value;
                break;
            case MessageCommandParameterDataType.Role:
            {
                if (_module.Message.Channel.GuildId is null)
                {
                    throw new Exceptions.ConvertionFailedException((string)value, InvalidMessageConvertionType.IsInDMs);
                }

                string str = (string)value;
                if (str.StartsWith("<@&"))
                {
                    str = str.Remove(0, 3);
                    str = str.Remove(str.Length - 1, 1);

                    if (ulong.TryParse(str, out ulong result))
                    {
                        try
                        {
                            obj = _module.Message.Channel.Guild.GetRole(result);
                        }
                        catch (DSharpPlus.Exceptions.ServerErrorException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.RoleDoesNotExist);
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.RoleDoesNotExist);
                        }
                    }
                    else
                    {
                        throw new Exceptions.ConvertionFailedException((string)value,
                            InvalidMessageConvertionType.NotAValidRole);
                    }
                }
                else
                {
                    if (ulong.TryParse(str, out ulong result))
                    {
                        try
                        {
                            obj = _module.Message.Channel.Guild.GetRole(result);
                        }
                        catch (DSharpPlus.Exceptions.ServerErrorException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.RoleDoesNotExist);
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException)
                        {
                            throw new Exceptions.ConvertionFailedException((string)value,
                                InvalidMessageConvertionType.RoleDoesNotExist);
                        }
                    }
                    else
                    {
                        throw new Exceptions.ConvertionFailedException((string)value,
                            InvalidMessageConvertionType.NotAValidRole);
                    }
                }

                break;
            }
        }

        return obj;
    }
}
