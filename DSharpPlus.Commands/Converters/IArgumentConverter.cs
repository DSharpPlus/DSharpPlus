namespace DSharpPlus.Commands.Converters;

using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;

public interface IArgumentConverter
{
    public string ReadableName { get; }
}

public interface IArgumentConverter<TConverterContext, TEventArgs, TOutput> : IArgumentConverter
    where TConverterContext : ConverterContext
    where TEventArgs : AsyncEventArgs
{
    public Task<Optional<TOutput>> ConvertAsync(TConverterContext context, TEventArgs eventArgs);
}
