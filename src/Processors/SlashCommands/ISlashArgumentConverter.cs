using DSharpPlus.CommandAll.Converters;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public interface ISlashArgumentConverter
    {
        public ApplicationCommandOptionType ArgumentType { get; init; }
    }

    public interface ISlashArgumentConverter<T> : ISlashArgumentConverter, IArgumentConverter<InteractionCreateEventArgs, T>;
}
