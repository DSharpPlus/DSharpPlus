using System;

namespace DSharpPlus.Commands;

/// <summary>
/// Stops this command from being executed in a guild. Use in conjunction with <see cref="AllowDmsAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DenyGuildsAttribute : Attribute;
