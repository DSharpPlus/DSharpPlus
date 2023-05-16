using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageConvertValues
{
    private readonly Dictionary<string, string?> _values;
    private readonly MessageMethodData _data;
    private readonly DiscordMessage _message;
    private readonly DiscordClient _client;

    public MessageConvertValues(Dictionary<string, string?> values, MessageMethodData data,
        DiscordMessage message, DiscordClient client)
        => (_values, _data, _message, _client) = (values, data, message, client);

    public async ValueTask<object?[]> StartConversionAsync()
    {
        List<object?> convertedValues = new();

        foreach (MessageParameterData paramData in _data.Parameters)
        {
            if (_values.TryGetValue(paramData.Name, out string? value))
            {
                if (value is null && paramData.CanBeNull)
                {
                    convertedValues.Add(null);
                }
                else if (value is null && paramData.HasDefaultValue)
                {
                    convertedValues.Add(Type.Missing);
                }
                else if (value is not null)
                {
                    object convertedValue = await ConvertValueAsync(value, paramData);
                    convertedValues.Add(convertedValue);
                }
            }
            else
            {
                if (paramData.CanBeNull)
                {
                    convertedValues.Add(null);
                }
                else
                {
                    // This shouldn't happen.
                }
            }
        }

        return convertedValues.ToArray();
    }

    public async ValueTask<object> ConvertValueAsync(string value, MessageParameterData data)
    {
        object obj;
        switch (data.Type)
        {
            case MessageParameterDataType.Bool:
                obj = bool.Parse(value);
                break;
            case MessageParameterDataType.Channel:
                if (_message.Channel.GuildId is null)
                {
                    throw new Exceptions.ConversionFailedException(value, InvalidMessageConversionType.IsInDMs,
                        data.IsPositionalArgument, data.Name);
                }

                obj = ulong.TryParse(value, out ulong channelResult)
                    ? (object)_message.Channel.Guild.GetChannel(channelResult)
                    : throw new Exceptions.ConversionFailedException(value,
                        InvalidMessageConversionType.NotAValidChannel, data.IsPositionalArgument, data.Name);

                break;
            case MessageParameterDataType.Role:
                if (_message.Channel.GuildId is null)
                {
                    throw new Exceptions.ConversionFailedException(value, InvalidMessageConversionType.IsInDMs,
                        data.IsPositionalArgument, data.Name);
                }

                obj = ulong.TryParse(value, out ulong roleResult)
                    ? (object)_message.Channel.Guild.GetRole(roleResult)
                    : throw new Exceptions.ConversionFailedException(value, InvalidMessageConversionType.NotAValidRole,
                        data.IsPositionalArgument, data.Name);

                break;
            case MessageParameterDataType.User:
                obj = ulong.TryParse(value, out ulong userResult)
                    ? (object)await _client.GetUserAsync(userResult)
                    : throw new Exceptions.ConversionFailedException(value, InvalidMessageConversionType.NotAValidUser,
                        data.IsPositionalArgument, data.Name);

                break;
            case MessageParameterDataType.Int:
                obj = int.TryParse(value, out int intResult)
                    ? (object)intResult
                    : throw new Exceptions.ConversionFailedException(value,
                        InvalidMessageConversionType.NotAValidInteger, data.IsPositionalArgument, data.Name);

                break;
            case MessageParameterDataType.Double:
                obj = double.TryParse(value, out double doubleResult)
                    ? (object)doubleResult
                    : throw new Exceptions.ConversionFailedException(value,
                        InvalidMessageConversionType.NotAValidNumber, data.IsPositionalArgument, data.Name);

                break;
            case MessageParameterDataType.String:
                obj = value;
                break;
            default:
                throw new NotImplementedException();
        }

        return obj;
    }
}
