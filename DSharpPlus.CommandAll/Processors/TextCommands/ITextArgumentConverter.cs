namespace DSharpPlus.CommandAll.Processors.TextCommands;

using DSharpPlus.CommandAll.Converters;
using DSharpPlus.EventArgs;

public interface ITextArgumentConverter : IArgumentConverter
{
    public bool RequiresText { get; init; }
}

public interface ITextArgumentConverter<T> : ITextArgumentConverter, IArgumentConverter<MessageCreateEventArgs, T>;
