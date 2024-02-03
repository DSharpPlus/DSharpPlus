namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.Threading.Tasks;

using DSharpPlus.Commands.Trees;

/// <summary>
/// Represents a base interface for context checks to implement.
/// </summary>
public interface IContextCheck<TAttribute>
    where TAttribute : ContextCheckAttribute
{
    public ValueTask<Exception?> ExecuteCheckAsync(TAttribute attribute, CommandContext context);
}
