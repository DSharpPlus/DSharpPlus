using System.Threading.Tasks;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors.TextCommands
{
    public interface ITextArgumentConverter
    {
        public bool RequiresText { get; init; }
    }

    public interface ITextArgumentConverter<T> : ITextArgumentConverter
    {
        public Task<Optional<T>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs);
    }
}
