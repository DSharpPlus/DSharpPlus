using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public interface IArgumentConverter
{
    public string ReadableName { get; }
}

public interface IArgumentConverter<TOutput> : IArgumentConverter
{
    public Task<Optional<TOutput>> ConvertAsync(ConverterContext context);
}
