#pragma warning disable IDE0046

using System.Linq;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees.Predicates;

/// <summary>
/// Checks whether the site of this command matches what is required. This will be enregistered with Discord for application commands.
/// </summary>
public class InLocationPredicate : ICommandExecutionPredicate
{
    public InLocationPredicate(bool isAllowedInGuilds, bool isAllowedInBotDms, bool isAllowedInOtherDms)
    {
        this.IsAllowedInGuilds = isAllowedInGuilds;
        this.IsAllowedInBotDms = isAllowedInBotDms;
        this.IsAllowedInOtherDms = isAllowedInOtherDms;
    }

    /// <summary>
    /// Specifies whether this predicate is fulfilled within guilds.
    /// </summary>
    public bool IsAllowedInGuilds { get; init; }

    /// <summary>
    /// Specifies whether this predicate is fulfilled within DMs between an user and the bot.
    /// </summary>
    public bool IsAllowedInBotDms { get; init; }

    /// <summary>
    /// Specifies whether this predicate is fulfilled within DMs between users without the bot.
    /// </summary>
    public bool IsAllowedInOtherDms { get; init; }

    /// <inheritdoc/>
    public bool IsFulfilled(AbstractContext context)
    {
        if (this.IsAllowedInGuilds && context.Guild is not null)
        {
            return true;
        }

        if (this.IsAllowedInBotDms && context.Channel.IsPrivate && ((DiscordDmChannel)context.Channel).Recipients.Any(x => x.Id == context.Client.CurrentUser.Id))
        {
            return true;
        }

        if (this.IsAllowedInOtherDms && context.Channel.IsPrivate && !((DiscordDmChannel)context.Channel).Recipients.Any(x => x.Id == context.Client.CurrentUser.Id))
        {
            return true;
        }

        return false;
    }
}
