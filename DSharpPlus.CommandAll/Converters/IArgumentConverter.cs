namespace DSharpPlus.CommandAll.Converters;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

public interface IArgumentConverter { }

public interface IArgumentConverter<TEventArgs, TOutput> : IArgumentConverter where TEventArgs : AsyncEventArgs
{
    public Task<Optional<TOutput>> ConvertAsync(ConverterContext context, TEventArgs eventArgs);
}
