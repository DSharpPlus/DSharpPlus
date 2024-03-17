namespace DSharpPlus.Commands.Processors.TextCommands;

using DSharpPlus.Commands.Converters;
using DSharpPlus.EventArgs;

public interface ITextArgumentConverter : IArgumentConverter
{
    public bool RequiresText { get; init; }
}

public interface ITextArgumentConverter<T> : ITextArgumentConverter, IArgumentConverter<TextConverterContext, MessageCreateEventArgs, T>;
