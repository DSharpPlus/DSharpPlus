using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public partial class DiscordSnowflakeObjectConverter : ISlashArgumentConverter<SnowflakeObject>, ITextArgumentConverter<SnowflakeObject>
{
    private static readonly DiscordMemberConverter discordMemberSlashArgumentConverter = new();
    private static readonly DiscordUserConverter discordUserSlashArgumentConverter = new();
    private static readonly DiscordRoleConverter discordRoleSlashArgumentConverter = new();

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Mentionable;
    public string ReadableName => "Discord User, Discord Member, or Discord Role";
    public bool RequiresText => true;

    public async Task<Optional<SnowflakeObject>> ConvertAsync(ConverterContext context)
    {
        // Checks through existing converters
        // Check if it's a role first since that converter doesn't make any Rest API calls.
        if (await discordRoleSlashArgumentConverter.ConvertAsync(context) is Optional<DiscordRole> role && role.HasValue)
        {
            return Optional.FromValue<SnowflakeObject>(role.Value);
        }
        // Check if it's a member since it's more likely the command invoker wants to mention a member instead of a random person.
        else if (await discordMemberSlashArgumentConverter.ConvertAsync(context) is Optional<DiscordMember> member && member.HasValue)
        {
            return Optional.FromValue<SnowflakeObject>(member.Value);
        }
        // Finally fallback to checking if it's a user.
        else if (await discordUserSlashArgumentConverter.ConvertAsync(context) is Optional<DiscordUser> user && user.HasValue)
        {
            return Optional.FromValue<SnowflakeObject>(user.Value);
        }

        // What the fuck.
        return Optional.FromNoValue<SnowflakeObject>();
    }
}
