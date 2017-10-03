using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified role. Note that it's much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RequireRolesAttributeAttribute : CheckBaseAttribute
    {
        /// <summary>
        /// Gets the name of the role required to execute this command.
        /// </summary>
        public IReadOnlyList<string> RoleNames { get; }

        /// <summary>
        /// Defines that usage of this command is restricted to members with specified role. Note that it's much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
        /// </summary>
        /// <param name="role_names">Names of the role required to execute this command.</param>
        public RequireRolesAttributeAttribute(params string[] role_names)
        {
            this.RoleNames = new ReadOnlyCollection<string>(role_names);
        }

        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
                return Task.FromResult(false);

            var rns = ctx.Member.Roles.Select(xr => xr.Name);
            return Task.FromResult(rns.Intersect(this.RoleNames).Any());
        }
    }
}
