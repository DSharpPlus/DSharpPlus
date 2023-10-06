using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.EventProcessors
{
    public sealed class InteractionCreateProcessor : IEventProcessor<InteractionCreateEventArgs>
    {
        private readonly FrozenDictionary<ulong, Command> _slashCommandMappings;

        public InteractionCreateProcessor(IDictionary<ulong, Command> slashCommandMappings) => _slashCommandMappings = slashCommandMappings.ToFrozenDictionary();

        /// <inheritdoc />
        public bool TryConvert(CommandAllExtension extension, InteractionCreateEventArgs eventArgs, [NotNullWhen(true)] out ConverterContext? context)
        {
            if (eventArgs.Interaction.Type != InteractionType.ApplicationCommand || !_slashCommandMappings.TryGetValue(eventArgs.Interaction.Data.Id, out Command? command))
            {
                context = null;
                return false;
            }

            context = new ConverterContext(extension, eventArgs, command);
            return true;
        }
    }
}
