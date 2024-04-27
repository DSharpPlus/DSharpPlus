namespace DSharpPlus.Commands.Converters;

using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class DiscordSnowflakeObjectConverter : ISlashArgumentConverter<SnowflakeObject>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.Mentionable;
    public bool RequiresText { get; init; } = true;
    private readonly ILogger<DiscordSnowflakeObjectConverter> _logger;

    public DiscordSnowflakeObjectConverter(ILogger<DiscordSnowflakeObjectConverter>? logger = null) => this._logger = logger ?? NullLogger<DiscordSnowflakeObjectConverter>.Instance;

    public Task<Optional<SnowflakeObject>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        DiscordInteractionDataOption option = eventArgs.Interaction.Data.Options.Single(x =>
                    x.Name.Equals(context.Parameter.Name, StringComparison.InvariantCultureIgnoreCase));

        //Checks through resolved
        if (eventArgs.Interaction.Data.Resolved.Roles != null && eventArgs.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out DiscordRole? role))
        {
            return Task.FromResult(Optional.FromValue<SnowflakeObject>(role));            
        }
        else if (eventArgs.Interaction.Data.Resolved.Members != null && eventArgs.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out DiscordMember? member))
        {
            return Task.FromResult(Optional.FromValue<SnowflakeObject>(member));
        }
        else if (eventArgs.Interaction.Data.Resolved.Users != null && eventArgs.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out DiscordUser? user))
        {
            return Task.FromResult(Optional.FromValue<SnowflakeObject>(user));
        }
        else
        {
            this._logger.LogError("Failed to resolve SnowflakeObject type."); 
            return Task.FromResult(Optional.FromNoValue<SnowflakeObject>());
        }
    }
}
