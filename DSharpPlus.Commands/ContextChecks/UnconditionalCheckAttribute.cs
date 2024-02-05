namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.ComponentModel;

/// <summary>
/// Represents a type for checks to register against that will always be executed, whether the attribute is present or not.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public sealed class UnconditionalCheckAttribute : ContextCheckAttribute;
