using System;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Marks this command or group as hidden.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HiddenAttribute : Attribute
    { }
}
