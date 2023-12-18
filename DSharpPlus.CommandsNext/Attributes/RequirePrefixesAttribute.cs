using System;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is only allowed with specific prefixes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RequirePrefixesAttribute : CheckBaseAttribute
{
    /// <summary>
    /// Gets the array of prefixes with which execution of this command is allowed.
    /// </summary>
    public string[] Prefixes { get; }

    /// <summary>
    /// <para>Gets or sets default help behaviour for this check. When this is enabled, invoking help without matching prefix will show the commands.</para>
    /// <para>Defaults to false.</para>
    /// </summary>
    public bool ShowInHelp { get; set; } = false;

    /// <summary>
    /// Defines that usage of this command is only allowed with specific prefixes.
    /// </summary>
    /// <param name="prefixes">Prefixes with which the execution of this command is allowed.</param>
    public RequirePrefixesAttribute(params string[] prefixes)
    {
        if (prefixes?.Any() != true)
        {
            throw new ArgumentNullException(nameof(prefixes), "The allowed prefix collection cannot be null or empty.");
        }

        this.Prefixes = prefixes;
    }

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        => Task.FromResult((help && this.ShowInHelp) || this.Prefixes.Contains(ctx.Prefix, ctx.CommandsNext.GetStringComparer()));
}
