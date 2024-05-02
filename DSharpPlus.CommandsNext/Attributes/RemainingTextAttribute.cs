
using System;

namespace DSharpPlus.CommandsNext.Attributes;
/// <summary>
/// Indicates that the command argument takes the rest of the input without parsing.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class RemainingTextAttribute : Attribute
{ }
