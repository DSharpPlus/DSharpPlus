namespace DSharpPlus.Commands.ContextChecks.ParameterChecks;


/// <summary>
/// Requires that the executing user is hierarchically placed higher than the value of this parameter.
/// </summary>
public sealed class RequireHigherUserHierarchyAttribute : ParameterCheckAttribute;
