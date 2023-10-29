using System.Threading.Tasks;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors.TextCommands
{
    public interface ITextArgumentConverter<T>
    {
        public Task<Optional<T>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs);
    }
}
