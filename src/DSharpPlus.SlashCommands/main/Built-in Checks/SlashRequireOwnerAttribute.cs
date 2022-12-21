using System;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.Attributes;

/// <summary>
/// Defines that this slash command is restricted to the owner of the bot.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SlashRequireOwnerAttribute : SlashCheckBaseAttribute
{
    /// <summary>
    /// Defines that this slash command is restricted to the owner of the bot.
    /// </summary>
    public SlashRequireOwnerAttribute() { }

    /// <summary>
    /// Runs checks.
    /// </summary>
    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        var app = ctx.Client.CurrentApplication;
        var me = ctx.Client.CurrentUser;

        return app != null ? Task.FromResult(app.Owners.Any(x => x.Id == ctx.User.Id)) : Task.FromResult(ctx.User.Id == me.Id);
    }
}
