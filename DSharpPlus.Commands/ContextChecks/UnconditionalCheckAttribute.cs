using System;

namespace DSharpPlus.Commands.ContextChecks;

/// <summary>
/// Represents a type for checks to register against that will always be executed, whether the attribute is present or not.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public sealed class UnconditionalCheckAttribute : ContextCheckAttribute;
