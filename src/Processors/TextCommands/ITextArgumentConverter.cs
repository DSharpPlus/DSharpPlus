using DSharpPlus.CommandAll.Converters;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors.TextCommands
{
    public interface ITextArgumentConverter
    {
        public bool RequiresText { get; init; }
    }

    public interface ITextArgumentConverter<T> : ITextArgumentConverter, IArgumentConverter<MessageCreateEventArgs, T>;
}
