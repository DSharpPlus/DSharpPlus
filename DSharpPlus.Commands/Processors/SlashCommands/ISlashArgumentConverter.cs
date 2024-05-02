using DSharpPlus.Commands.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public interface ISlashArgumentConverter : IArgumentConverter
{
    public DiscordApplicationCommandOptionType ParameterType { get; }
}

public interface ISlashArgumentConverter<T> : ISlashArgumentConverter, IArgumentConverter<InteractionConverterContext, InteractionCreateEventArgs, T>;
