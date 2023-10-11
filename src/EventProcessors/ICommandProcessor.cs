using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.CommandAll.EventArgs;

namespace DSharpPlus.CommandAll.Processors
{
    public interface ICommandProcessor
    {
        public Task ConfigureAsync(CommandAllExtension extension, ConfigureCommandsEventArgs eventArgs);
    }

    public interface ICommandProcessor<TInputEvent> : ICommandProcessor where TInputEvent : AsyncEventArgs
    {
        public Task<ConverterContext?> ConvertEventAsync(CommandAllExtension extension, TInputEvent eventArgs);
        public Task<CommandContext?> ParseArgumentsAsync(ConverterContext converterContext, TInputEvent eventArgs);
    }
}
