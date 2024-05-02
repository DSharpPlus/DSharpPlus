
using DSharpPlus.Commands.Converters;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Processors.TextCommands;
public interface ITextArgumentConverter : IArgumentConverter
{
    public bool RequiresText { get; init; }
}

public interface ITextArgumentConverter<T> : ITextArgumentConverter, IArgumentConverter<TextConverterContext, MessageCreateEventArgs, T>;
