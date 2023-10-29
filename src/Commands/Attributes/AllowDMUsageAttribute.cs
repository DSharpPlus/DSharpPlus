using System;

namespace DSharpPlus.CommandAll.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AllowDMUsageAttribute : Attribute { }
}
