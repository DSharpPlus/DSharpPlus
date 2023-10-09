using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.CommandAll.EventArgs;

namespace DSharpPlus.CommandAll.EventProcessors
{
    public interface IEventProcessor<TInputEvent> where TInputEvent : AsyncEventArgs
    {
        public Task ConfigureAsync(CommandAllExtension extension, ConfigureCommandsEventArgs eventArgs);
        public Task<ConverterContext?> ConvertEventAsync(CommandAllExtension extension, TInputEvent eventArgs);
        public Task<CommandContext?> ParseArgumentsAsync(ConverterContext converterContext, TInputEvent eventArgs);
    }
}
