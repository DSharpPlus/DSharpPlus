using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class StringConverter : IArgumentConverter<string>
    {
        public Task<Optional<string>> ConvertAsync(string value, CommandContext ctx)
            => Task.FromResult(Optional<string>.FromValue(value));
    }

    public class UriConverter : IArgumentConverter<Uri>
    {
        public Task<Optional<Uri>> ConvertAsync(string value, CommandContext ctx)
        {
            try
            {
                if (value.StartsWith("<") && value.EndsWith(">"))
                    value = value.Substring(1, value.Length - 2);

                return Task.FromResult(Optional<Uri>.FromValue(new Uri(value)));
            }
            catch
            {
                return Task.FromResult(Optional<Uri>.FromNoValue());
            }
        }
    }
}
