namespace DSharpPlus.Commands.ContextChecks.ParameterChecks;
using System;
using System.Threading.Tasks;

/// <summary>
/// Represents an entry in a map of attributes to check types. we can't just do this as a dictionary because one attribute may
/// key multiple different checks.
/// </summary>
public readonly record struct ParameterCheckMapEntry
{
    public required Type AttributeType { get; init; }

    public required Type CheckType { get; init; }

    // we cache this here so that we don't have to deal with it every invocation.
    public required Func<object, ParameterCheckAttribute, ParameterCheckInfo, CommandContext, ValueTask<string?>> ExecuteCheckAsync { get; init; }
}
