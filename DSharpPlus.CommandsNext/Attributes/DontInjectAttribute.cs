namespace DSharpPlus.CommandsNext.Attributes;
using System;

/// <summary>
/// Prevents this field or property from having its value injected by dependency injection.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class DontInjectAttribute : Attribute
{ }
