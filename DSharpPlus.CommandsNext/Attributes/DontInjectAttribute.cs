using System;

namespace DSharpPlus.CommandsNext.Attributes;

/// <summary>
/// Prevents this field or property from having its value injected by dependency injection.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class DontInjectAttribute : Attribute
{ }
