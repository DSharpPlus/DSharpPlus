namespace DSharpPlus.Commands.ContextChecks;
using System.Threading.Tasks;

/// <summary>
/// Marker interface for context checks, use <seealso cref="IContextCheck{TAttribute}"/> instead.
/// </summary>
public interface IContextCheck;

/// <summary>
/// Represents a base interface for context checks to implement.
/// </summary>
public interface IContextCheck<TAttribute> : IContextCheck
    where TAttribute : ContextCheckAttribute
{
    /// <summary>
    /// Executes the check given the attribute.
    /// </summary>
    /// <remarks>
    /// It is allowed for a check to access other metadata from the context.
    /// </remarks>
    /// <param name="attribute">The attribute this command was decorated with.</param>
    /// <param name="context">The context this command is executed in.</param>
    /// <returns>A string containing the error message, or <c>null</c> if successful.</returns>
    public ValueTask<string?> ExecuteCheckAsync(TAttribute attribute, CommandContext context);
}
