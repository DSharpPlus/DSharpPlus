namespace DSharpPlus.Commands.Processors.SlashCommands;

using DSharpPlus.Commands.Converters;
using DSharpPlus.EventArgs;

public interface ISlashArgumentConverter : IArgumentConverter
{
    public ApplicationCommandOptionType ParameterType { get; init; }
}

public interface ISlashArgumentConverter<T> : ISlashArgumentConverter, IArgumentConverter<InteractionCreateEventArgs, T>;
