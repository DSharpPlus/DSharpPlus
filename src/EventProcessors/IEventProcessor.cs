using System.Diagnostics.CodeAnalysis;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Converters.Meta;

namespace DSharpPlus.CommandAll.EventProcessors
{
    public interface IEventProcessor<TInputEvent> where TInputEvent : AsyncEventArgs
    {
        public bool TryConvert(CommandAllExtension extension, TInputEvent eventArgs, [NotNullWhen(true)] out ConverterContext? context);
    }
}
