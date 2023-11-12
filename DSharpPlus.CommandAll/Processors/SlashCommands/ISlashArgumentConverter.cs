using DSharpPlus.CommandAll.Converters;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Processors.SlashCommands;

public interface ISlashArgumentConverter : IArgumentConverter
{
    public ApplicationCommandOptionType ParameterType { get; init; }
}

public interface ISlashArgumentConverter<T> : ISlashArgumentConverter, IArgumentConverter<InteractionCreateEventArgs, T>;
