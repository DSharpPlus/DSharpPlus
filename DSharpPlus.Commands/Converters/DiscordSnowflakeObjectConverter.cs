namespace DSharpPlus.Commands.Converters;

using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;

public partial class DiscordSnowflakeObjectConverter : ISlashArgumentConverter<SnowflakeObject>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.Mentionable;
    public bool RequiresText { get; init; } = true;
    private readonly ILogger<DiscordSnowflakeObjectConverter> _logger;
    private readonly ISlashArgumentConverter<DiscordMember> discordMemberSlashArgumentConverter;
    private readonly ISlashArgumentConverter<DiscordRole> discordRoleSlashArgumentConverter;
    private readonly ISlashArgumentConverter<DiscordUser> discordUserSlashArgumentConverter;

    public DiscordSnowflakeObjectConverter(ISlashArgumentConverter<DiscordMember> discordMemberSlashArgumentConverter, ISlashArgumentConverter<DiscordRole> discordRoleSlashArgumentConverter, ISlashArgumentConverter<DiscordUser> discordUserSlashArgumentConverter, ILogger<DiscordSnowflakeObjectConverter>? logger = null)
    {
        this.discordMemberSlashArgumentConverter = discordMemberSlashArgumentConverter;
        this.discordRoleSlashArgumentConverter = discordRoleSlashArgumentConverter;
        this.discordUserSlashArgumentConverter = discordUserSlashArgumentConverter;
        this._logger = this._logger = logger ?? NullLogger<DiscordSnowflakeObjectConverter>.Instance;
    }

    public async Task<Optional<SnowflakeObject>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        //Checks through existing converters
        if (await discordRoleSlashArgumentConverter.ConvertAsync(context, eventArgs) is Optional<DiscordRole> role && role.HasValue)
        {
            return Optional.FromValue<SnowflakeObject>(role.Value);
        }
        else if (await discordMemberSlashArgumentConverter.ConvertAsync(context, eventArgs) is Optional<DiscordMember> member && member.HasValue)
        {
            return Optional.FromValue<SnowflakeObject>(member.Value);
        }
        else if (await discordUserSlashArgumentConverter.ConvertAsync(context, eventArgs) is Optional<DiscordUser> user && user.HasValue)
        {
            return Optional.FromValue<SnowflakeObject>(user.Value);
        }
        else
        {
            this._logger.LogError("Failed to resolve SnowflakeObject type."); 
            return Optional.FromNoValue<SnowflakeObject>();
        }
    }
}
