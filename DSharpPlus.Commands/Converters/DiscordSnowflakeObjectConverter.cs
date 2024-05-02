
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Commands.Converters;
public partial class DiscordSnowflakeObjectConverter : ISlashArgumentConverter<SnowflakeObject>, ITextArgumentConverter<SnowflakeObject>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.Mentionable;
    public bool RequiresText { get; init; } = true;

    private readonly DiscordMemberConverter discordMemberSlashArgumentConverter;
    private readonly DiscordUserConverter discordUserSlashArgumentConverter;
    private readonly DiscordRoleConverter discordRoleSlashArgumentConverter;

    public DiscordSnowflakeObjectConverter(ILogger<DiscordMemberConverter>? discordMemberSlashArgumentConverter = null, ILogger<DiscordUserConverter>? discordUserSlashArgumentConverter = null)
    {
        this.discordMemberSlashArgumentConverter = new DiscordMemberConverter(discordMemberSlashArgumentConverter ?? NullLogger<DiscordMemberConverter>.Instance);
        this.discordUserSlashArgumentConverter = new DiscordUserConverter(discordUserSlashArgumentConverter ?? NullLogger<DiscordUserConverter>.Instance);
        discordRoleSlashArgumentConverter = new DiscordRoleConverter();
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

        return Optional.FromNoValue<SnowflakeObject>();
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

        return Optional.FromNoValue<SnowflakeObject>();
    }
}
