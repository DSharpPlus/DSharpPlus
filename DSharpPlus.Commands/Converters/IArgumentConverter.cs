using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public interface IArgumentConverter { }

public interface IArgumentConverter<TConverterContext, TEventArgs, TOutput> : IArgumentConverter
    where TConverterContext : ConverterContext
    where TEventArgs : AsyncEventArgs
{
    public Task<Optional<TOutput>> ConvertAsync(TConverterContext context, TEventArgs eventArgs);
}
