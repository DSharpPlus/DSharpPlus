using System;

namespace DSharpPlus.CommandAll.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
    public class NsfwAttribute : Attribute { }
}
