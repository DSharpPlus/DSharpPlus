using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class StringConverter : IArgumentConverter<string>
    {
        public Task<Optional<string>> ConvertAsync(string value, CommandContext ctx)
            => Task.FromResult(Optional<string>.FromValue(value));
    }
}
