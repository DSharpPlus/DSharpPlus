namespace DSharpPlus.CommandAll.Processors.SlashCommands;

using DSharpPlus.CommandAll.Converters;
using DSharpPlus.EventArgs;

public interface ISlashArgumentConverter : IArgumentConverter
{
    public ApplicationCommandOptionType ParameterType { get; init; }
}

public interface ISlashArgumentConverter<T> : ISlashArgumentConverter, IArgumentConverter<InteractionCreateEventArgs, T>;
