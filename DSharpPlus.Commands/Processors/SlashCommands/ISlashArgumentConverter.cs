using DSharpPlus.Commands.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public interface ISlashArgumentConverter : IArgumentConverter
{
    public DiscordApplicationCommandOptionType ParameterType { get; }
}

public interface ISlashArgumentConverter<T> : ISlashArgumentConverter, IArgumentConverter<T>;
