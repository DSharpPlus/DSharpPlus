using System.Threading.Tasks;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public interface ISlashArgumentConverter
    {
        public ApplicationCommandOptionType ArgumentType { get; init; }
    }

    public interface ISlashArgumentConverter<T> : ISlashArgumentConverter
    {
        public Task<Optional<T>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs);
    }
}
