using System.Net.Http.Headers;
using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageConvertValues
{
    private Dictionary<string, string?> _values;
    private MessageCommandMethodData _data;
    private DiscordMessage _message;
    private DiscordClient _client;

    public MessageConvertValues(Dictionary<string, string?> values, MessageCommandMethodData data,
        DiscordMessage message, DiscordClient client)
        => (_values, _data, _message, _client) = (values, data, message, client);

    public async ValueTask<object?[]> StartConversionAsync()
    {
        object?[] convertedValues = new object[_data.Parameters.Count];

        for (int i = 0; i < _data.Parameters.Count; i++)
        {
            MessageCommandParameterData paramData = _data.Parameters[i];

            if (_values.TryGetValue(paramData.Name, out string? value))
            {
                Console.WriteLine(paramData.CanBeNull);
                if (value is null && paramData.CanBeNull)
                {
                    convertedValues[i] = null;
                }
                else
                {
                    object convertedValue = await ConvertValueAsync(value, paramData);
                    convertedValues[i] = convertedValue;
                }
            }
            else
            {
                if (paramData.CanBeNull)
                {
                    convertedValues[i] = null;
                }
                else
                {
                    // This shouldn't happen.
                }
            }
        }

        return convertedValues;
    }

    public async ValueTask<object> ConvertValueAsync(string value, MessageCommandParameterData data)
    {
        object obj;
        switch (data.Type)
        {
            case MessageCommandParameterDataType.Bool:
                obj = bool.Parse(value);
                break;
            case MessageCommandParameterDataType.Channel:
                if (_message.Channel.GuildId is null)
                {
                    throw new Exceptions.ConversionFailedException(value, InvalidMessageConvertionType.IsInDMs,
                        data.IsPositionalArgument, data.Name);
                }

                if (ulong.TryParse(value, out ulong channelResult))
                {
                    obj = _message.Channel.Guild.GetChannel(channelResult);
                }
                else
                {
                    throw new Exceptions.ConversionFailedException(value,
                        InvalidMessageConvertionType.NotAValidChannel, data.IsPositionalArgument, data.Name);
                }

                break;
            case MessageCommandParameterDataType.Role:
                if (_message.Channel.GuildId is null)
                {
                    throw new Exceptions.ConversionFailedException(value, InvalidMessageConvertionType.IsInDMs,
                        data.IsPositionalArgument, data.Name);
                }

                if (ulong.TryParse(value, out ulong roleResult))
                {
                    obj = _message.Channel.Guild.GetRole(roleResult);
                }
                else
                {
                    throw new Exceptions.ConversionFailedException(value, InvalidMessageConvertionType.NotAValidRole,
                        data.IsPositionalArgument, data.Name);
                }

                break;
            case MessageCommandParameterDataType.User:
                if (ulong.TryParse(value, out ulong userResult))
                {
                    obj = await _client.GetUserAsync(userResult);
                }
                else
                {
                    throw new Exceptions.ConversionFailedException(value, InvalidMessageConvertionType.NotAValidUser,
                        data.IsPositionalArgument, data.Name);
                }

                break;
            case MessageCommandParameterDataType.Int:
                if (int.TryParse(value, out int intResult))
                {
                    obj = intResult;
                }
                else
                {
                    throw new Exceptions.ConversionFailedException(value,
                        InvalidMessageConvertionType.NotAValidInteger, data.IsPositionalArgument, data.Name);
                }

                break;
            case MessageCommandParameterDataType.Double:
                if (double.TryParse(value, out double doubleResult))
                {
                    obj = doubleResult;
                }
                else
                {
                    throw new Exceptions.ConversionFailedException(value,
                        InvalidMessageConvertionType.NotAValidNumber, data.IsPositionalArgument, data.Name);
                }

                break;
            case MessageCommandParameterDataType.String:
                obj = value;
                break;
            default:
                throw new NotImplementedException();
        }

        return obj;
    }
}
