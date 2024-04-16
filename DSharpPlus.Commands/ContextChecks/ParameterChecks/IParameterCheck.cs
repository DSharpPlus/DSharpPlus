using System.Threading.Tasks;

namespace DSharpPlus.Commands.ContextChecks.ParameterChecks;

/// <summary>
/// Marker interface for parameter checks. Use <seealso cref="IParameterCheck{TAttribute}"/> instead.
/// </summary>
public interface IParameterCheck;

/// <summary>
/// Represents a base interface for parameter checks to implement.
/// </summary>
public interface IParameterCheck<TAttribute> : IParameterCheck
{
    /// <summary>
    /// Executes the check given the attribute and parameter info.
    /// </summary>
    /// <remarks>
    /// It is allowed for a check to access other metadata from the context.
    /// </remarks>
    /// <param name="attribute">The attribute this parameter was decorated with.</param>
    /// <param name="info">The relevant parameters metadata representation and value.</param>
    /// <param name="context">The context the containing command is executed in.</param>
    /// <returns>A string containing the error message, or <c>null</c> if successful.</returns>
    public ValueTask<string?> ExecuteCheckAsync(TAttribute attribute, ParameterCheckInfo info, CommandContext context);
}
