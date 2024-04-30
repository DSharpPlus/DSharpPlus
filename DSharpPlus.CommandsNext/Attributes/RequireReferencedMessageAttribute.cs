namespace DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

/// <summary>
/// Defines that a command is only usable when sent in reply. Command will appear in help regardless of this attribute.
/// </summary>
public sealed class RequireReferencedMessageAttribute : CheckBaseAttribute
{
    /// <summary>
    /// Defines that a command is only usable when sent in reply. Command will appear in help regardless of this attribute.
    /// </summary>
    public RequireReferencedMessageAttribute()
    { }

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        => Task.FromResult(help || ctx.Message.ReferencedMessage != null);
}
