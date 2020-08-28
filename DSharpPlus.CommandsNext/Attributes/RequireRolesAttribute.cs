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
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RequireRolesAttribute : CheckBaseAttribute
    {
        /// <summary>
        /// Gets the name of the role required to execute this command.
        /// </summary>
        public IReadOnlyList<string> RoleNames { get; }

        /// <summary>
        /// Gets the role checking mode. Refer to <see cref="RoleCheckMode"/> for more information.
        /// </summary>
        public RoleCheckMode CheckMode { get; }

        /// <summary>
        /// Defines that usage of this command is restricted to members with specified role. Note that it's much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
        /// </summary>
        /// <param name="checkMode">Role checking mode.</param>
        /// <param name="roleNames">Names of the role to be verified by this check.</param>
        public RequireRolesAttribute(RoleCheckMode checkMode, params string[] roleNames)
        {
            this.CheckMode = checkMode;
            this.RoleNames = new ReadOnlyCollection<string>(roleNames);
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
                return Task.FromResult(false);

            var rns = ctx.Member.Roles.Select(xr => xr.Name);
            var rnc = rns.Count();
            var ins = rns.Intersect(this.RoleNames, ctx.CommandsNext.GetStringComparer());
            var inc = ins.Count();

            switch (this.CheckMode)
            {
                case RoleCheckMode.All:
                    return Task.FromResult(this.RoleNames.Count == inc);
                    
                case RoleCheckMode.SpecifiedOnly:
                    return Task.FromResult(rnc == inc);

                case RoleCheckMode.None:
                    return Task.FromResult(inc == 0);
                    
                case RoleCheckMode.Any:
                default:
                    return Task.FromResult(inc > 0);
            }
        }
    }

    /// <summary>
    /// Specifies how does <see cref="RequireRolesAttribute"/> check for roles.
    /// </summary>
    public enum RoleCheckMode
    {
        /// <summary>
        /// Member is required to have any of the specified roles.
        /// </summary>
        Any,

        /// <summary>
        /// Member is required to have all of the specified roles.
        /// </summary>
        All,

        /// <summary>
        /// Member is required to have exactly the same roles as specified; no extra roles may be present.
        /// </summary>
        SpecifiedOnly,

        /// <summary>
        /// Member is required to have none of the specified roles.
        /// </summary>
        None
    }
}
