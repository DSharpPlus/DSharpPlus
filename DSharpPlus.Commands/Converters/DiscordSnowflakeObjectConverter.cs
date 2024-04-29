namespace DSharpPlus.Commands.Converters;

using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public partial class DiscordSnowflakeObjectConverter : ISlashArgumentConverter<SnowflakeObject>, ITextArgumentConverter<SnowflakeObject>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.Mentionable;
    public bool RequiresText { get; init; } = true;

    private readonly DiscordMemberConverter discordMemberSlashArgumentConverter;
    private readonly DiscordRoleConverter discordRoleSlashArgumentConverter;
    private readonly DiscordUserConverter discordUserSlashArgumentConverter;
    private readonly ILogger<DiscordSnowflakeObjectConverter> _logger;

    public DiscordSnowflakeObjectConverter(
        DiscordMemberConverter discordMemberSlashArgumentConverter,
        DiscordRoleConverter discordRoleSlashArgumentConverter,
        DiscordUserConverter discordUserSlashArgumentConverter,
        ILogger<DiscordSnowflakeObjectConverter>? logger = null
    )
    {
        this.discordMemberSlashArgumentConverter = discordMemberSlashArgumentConverter;
        this.discordRoleSlashArgumentConverter = discordRoleSlashArgumentConverter;
        this.discordUserSlashArgumentConverter = discordUserSlashArgumentConverter;
        this._logger = logger ?? NullLogger<DiscordSnowflakeObjectConverter>.Instance;
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

    // Duplicated logic for overload resolving
    public async Task<Optional<SnowflakeObject>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs)
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
