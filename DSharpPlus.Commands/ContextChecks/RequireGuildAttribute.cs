
using System;

namespace DSharpPlus.Commands.ContextChecks;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class RequireGuildAttribute : ContextCheckAttribute;
