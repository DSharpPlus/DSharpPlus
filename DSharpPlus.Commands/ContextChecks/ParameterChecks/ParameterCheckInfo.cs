namespace DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.Trees;

/// <summary>
/// Presents information about a parameter check.
/// </summary>
/// <param name="Parameter">The parameter as represented in the command tree.</param>
/// <param name="Value">The processed value of the parameter.</param>
public sealed record ParameterCheckInfo(CommandParameter Parameter, object? Value);
