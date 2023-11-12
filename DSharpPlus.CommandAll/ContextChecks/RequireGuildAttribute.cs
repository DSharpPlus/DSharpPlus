using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.ContextChecks
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
    public class RequireGuildAttribute : ContextCheckAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext context) => Task.FromResult(context.Guild is not null);
    }
}
