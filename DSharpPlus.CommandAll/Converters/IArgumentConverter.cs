using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Converters
{
    public interface IArgumentConverter { }

    public interface IArgumentConverter<TEventArgs, TOutput> : IArgumentConverter where TEventArgs : AsyncEventArgs
    {
        public Task<Optional<TOutput>> ConvertAsync(ConverterContext context, TEventArgs eventArgs);
    }
}
