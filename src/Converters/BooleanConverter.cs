using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class BooleanConverter : ISlashArgumentConverter<bool>, ITextArgumentConverter<bool>
    {
        private static readonly FrozenDictionary<string, bool> _values = new Dictionary<string, bool>
        {
            ["true"] = true,
            ["false"] = false,
            ["yes"] = true,
            ["no"] = false,
            ["y"] = true,
            ["n"] = false,
            ["1"] = true,
            ["0"] = false,
            ["on"] = true,
            ["off"] = false,
            ["enable"] = true,
            ["disable"] = false,
            ["enabled"] = true,
            ["disabled"] = false,
            ["t"] = true,
            ["f"] = false
        }.ToFrozenDictionary();

        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Boolean;

        public Task<Optional<bool>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<bool>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => Task.FromResult(Optional.FromValue((bool)context.As<SlashConverterContext>().CurrentOption.Value));
    }
}
