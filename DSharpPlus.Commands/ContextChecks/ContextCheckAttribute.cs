namespace DSharpPlus.Commands.ContextChecks;

using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public abstract class ContextCheckAttribute : Attribute
{
    public abstract Task<bool> ExecuteCheckAsync(CommandContext context);
}
