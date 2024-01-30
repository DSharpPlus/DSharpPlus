using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors
{
    public abstract class TextCommandProcessor : ICommandProcessor<MessageCreateEventArgs>
    {
        public IReadOnlyDictionary<Type, ConverterDelegate<MessageCreateEventArgs>> Converters => throw new NotImplementedException();
        public Task ConfigureAsync(CommandAllExtension extension) => throw new NotImplementedException();
    }
}
