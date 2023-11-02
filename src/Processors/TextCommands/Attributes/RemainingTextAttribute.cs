using System;

namespace DSharpPlus.CommandAll.Processors.TextCommands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
    public sealed class RemainingTextAttribute : Attribute;
}
