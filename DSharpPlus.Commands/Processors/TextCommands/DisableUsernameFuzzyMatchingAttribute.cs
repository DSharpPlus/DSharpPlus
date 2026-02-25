using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.TextCommands;

/// <summary>
/// Applied to parameters of types <see cref="DiscordUser"/> and <see cref="DiscordMember"/>, prevents DSharpPlus from fuzzy-matching
/// against their username and requires an exact, case-insensitive match instead.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class DisableUsernameFuzzyMatchingAttribute : Attribute;
