
using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters;
public class StringConverter : IArgumentConverter<string>
{
    Task<Optional<string>> IArgumentConverter<string>.ConvertAsync(string value, CommandContext ctx)
        => Task.FromResult(Optional.FromValue(value));
}

public class UriConverter : IArgumentConverter<Uri>
{
    Task<Optional<Uri>> IArgumentConverter<Uri>.ConvertAsync(string value, CommandContext ctx)
    {
        try
        {
            if (value.StartsWith('<') && value.EndsWith('>'))
            {
                value = value[1..^1];
            }

            return Task.FromResult(Optional.FromValue(new Uri(value)));
        }
        catch
        {
            return Task.FromResult(Optional.FromNoValue<Uri>());
        }
    }
}
