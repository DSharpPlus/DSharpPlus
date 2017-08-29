using System;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified role.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RequireRoleAttribute : CheckBaseAttribute
    {
        /// <summary>
        /// Gets the name of the role required to execute this command.
        /// </summary>
        public string RoleName { get; }

        /// <summary>
        /// Defines that usage of this command is restricted to members with specified role.
        /// </summary>
        /// <param name="role_name">Name of the role required to execute this command.</param>
        public RequireRoleAttribute(string role_name)
        {
            this.RoleName = role_name;
        }

        public override Task<bool> CanExecute(CommandContext ctx)
        {
            if (ctx.Guild == null || ctx.Member == null)
                return Task.FromResult(false);

            return Task.FromResult(ctx.Member.Roles.Any(xr => xr.Name == this.RoleName));
        }
    }
}
