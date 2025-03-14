using System;

namespace DSharpPlus.Commands.ArgumentModifiers;

/// <summary>
/// Causes this parameter to use as much data as possible rather than as little data as possible. For final string parameters,
/// this means matching the entire remaining text.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class GreedyAttribute : Attribute;
