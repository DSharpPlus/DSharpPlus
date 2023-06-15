using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Message.Errors;
using Remora.Results;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

using ConverterLambda = Func<IServiceProvider, DiscordClient, DiscordMessage, ArraySegment<char>, ValueTask<IResult>>;
using ConversionLambda = Func<IResult, object?>;

internal class MessageConvertValues
{
    private readonly IReadOnlyList<(Type, ArraySegment<char>?)> _values;
    private readonly MessageMethodData _data;
    private readonly DiscordMessage _message;
    private readonly DiscordClient _client;
    private readonly IServiceScope _scope;

    public MessageConvertValues(IReadOnlyList<(Type, ArraySegment<char>?)> values, MessageMethodData data,
        DiscordMessage message, DiscordClient client, IServiceScope scope)
    {
        this._values = values;
        this._data = data;
        this._message = message;
        this._client = client;
        this._scope = scope;
    }

    public async ValueTask<Result<object?[]>> StartConversionAsync()
    {
        List<object?> objects = new(_values.Count);

        for (int i = 0; i < _values.Count; i++)
        {
            (Type type, ArraySegment<char>? segment) = _values[i];
            MessageParameterData param = _data.Parameters[i];

            try
            {
                if (segment is ArraySegment<char> s)
                {
                    (ConverterLambda converter, ConversionLambda conversion) =
                        CommandController.ConverterList.GetValueOrDefault(type)!;

                    ValueTask<IResult> taskResult = converter(_scope.ServiceProvider, _client, _message, s);
                    IResult result = await taskResult;
                    if (result.IsSuccess)
                    {
                        objects.Add(conversion(result));
                    }
                    else
                    {
                        return Result<object?[]>.FromError(new FailedConversionError("Failed to convert value", param.Name, new(s), result.Error));
                    }
                }
                else
                {
                    objects.Add(null);
                }
            }
            catch (Exception e)
            {
                return Result<object?[]>.FromError(new ExceptionError(e));
            }
        }

        return objects.ToArray();
    }
}
