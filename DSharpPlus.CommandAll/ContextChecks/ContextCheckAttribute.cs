namespace DSharpPlus.CommandAll.ContextChecks;
using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public abstract class ContextCheckAttribute : Attribute
{
    public abstract Task<bool> ExecuteCheckAsync(CommandContext context);
}
