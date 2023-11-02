using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Checks
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public abstract class ContextCheckAttribute : Attribute
    {
        public abstract Task<bool> ExecuteCheckAsync(CommandContext context);
    }
}
