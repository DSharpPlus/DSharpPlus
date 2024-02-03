namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.Threading.Tasks;

using DSharpPlus.Commands.Trees;

/// <summary>
/// Represents a base interface for context checks to implement.
/// </summary>
public interface IContextCheck
{
    public ValueTask<Exception?> ExecuteCheckAsync(CommandContext context);
}
