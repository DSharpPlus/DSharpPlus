using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.EventArgs;

namespace DSharpPlus.CommandAll.Processors
{
    public interface ICommandProcessor
    {
        public Task ConfigureAsync(CommandAllExtension extension, ConfigureCommandsEventArgs eventArgs);
    }

    public interface ICommandProcessor<T> : ICommandProcessor where T : AsyncEventArgs
    {
        public IReadOnlyDictionary<Type, ConverterDelegate<T>> Converters { get; }
    }
}
