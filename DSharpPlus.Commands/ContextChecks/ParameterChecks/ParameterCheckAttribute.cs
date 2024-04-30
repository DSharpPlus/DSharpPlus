namespace DSharpPlus.Commands.ContextChecks.ParameterChecks;

using System;

/// <summary>
/// Represents a base attribute for parameter check metadata attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
public abstract class ParameterCheckAttribute : Attribute;
