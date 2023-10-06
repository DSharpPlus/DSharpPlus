using System.Diagnostics.CodeAnalysis;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.EventProcessors
{
    public sealed class MessageCreateProcessor : IEventProcessor<MessageCreateEventArgs>
    {
        /// <inheritdoc />
        public bool TryConvert(CommandAllExtension extension, MessageCreateEventArgs eventArgs, [NotNullWhen(true)] out ConverterContext? context)
        {
            context = null;
            return false;
        }
    }
}
