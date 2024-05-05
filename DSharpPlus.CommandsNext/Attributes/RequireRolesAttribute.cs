using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is restricted to members with specified role. Note that it's much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RequireRolesAttribute : CheckBaseAttribute
{
    /// <summary>
    /// Gets the names of roles required to execute this command.
    /// </summary>
    public IReadOnlyList<string> RoleNames { get; }

    /// <summary>
    /// Gets the IDs of roles required to execute this command.
    /// </summary>
    public IReadOnlyList<ulong> RoleIds { get; }

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
        : this(checkMode, roleNames, [])
    { }

    /// <summary>
    /// Defines that usage of this command is restricted to members with the specified role.
    /// Note that it is much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
    /// </summary>
    /// <param name="checkMode">Role checking mode.</param>
    /// <param name="roleIds">IDs of the roles to be verified by this check.</param>
    public RequireRolesAttribute(RoleCheckMode checkMode, params ulong[] roleIds)
        : this(checkMode, [], roleIds)
    { }

    /// <summary>
    /// Defines that usage of this command is restricted to members with the specified role.
    /// Note that it is much preferred to restrict access using <see cref="RequirePermissionsAttribute"/>.
    /// </summary>
    /// <param name="checkMode">Role checking mode.</param>
    /// <param name="roleNames">Names of the role to be verified by this check.</param>
    /// <param name="roleIds">IDs of the roles to be verified by this check.</param>
    public RequireRolesAttribute(RoleCheckMode checkMode, string[] roleNames, ulong[] roleIds)
    {
        this.CheckMode = checkMode;
        this.RoleIds = new ReadOnlyCollection<ulong>(roleIds);
        this.RoleNames = new ReadOnlyCollection<string>(roleNames);
    }

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Guild == null || ctx.Member == null)
        {
            return Task.FromResult(false);
        }

        if ((this.CheckMode.HasFlag(RoleCheckMode.MatchNames) && !this.CheckMode.HasFlag(RoleCheckMode.MatchIds)) || this.RoleIds.Count == 0)
        {
            return Task.FromResult(MatchRoles(
                this.RoleNames, ctx.Member.Roles.Select(xm => xm.Name), ctx.CommandsNext.GetStringComparer()));
        }
        else if ((!this.CheckMode.HasFlag(RoleCheckMode.MatchNames) && this.CheckMode.HasFlag(RoleCheckMode.MatchIds)) || this.RoleNames.Count == 0)
        {
            return Task.FromResult(MatchRoles(this.RoleIds, ctx.Member.RoleIds));
        }
        else // match both names and IDs
        {
            bool nameMatch = MatchRoles(this.RoleNames, ctx.Member.Roles.Select(xm => xm.Name), ctx.CommandsNext.GetStringComparer()),
                idMatch = MatchRoles(this.RoleIds, ctx.Member.RoleIds);

            return Task.FromResult(this.CheckMode switch
            {
                RoleCheckMode.Any => nameMatch || idMatch,
                _ => nameMatch && idMatch
            });
        }
    }

    private bool MatchRoles<T>(IReadOnlyList<T> present, IEnumerable<T> passed, IEqualityComparer<T>? comparer = null)
    {
        IEnumerable<T> intersect = passed.Intersect(present, comparer ?? EqualityComparer<T>.Default);

        return this.CheckMode switch
        {
            RoleCheckMode.All => present.Count == intersect.Count(),
            RoleCheckMode.SpecifiedOnly => passed.Count() == intersect.Count(),
            RoleCheckMode.None => !intersect.Any(),
            _ => intersect.Any()
        };
    }
}

/// <summary>
/// Specifies how <see cref="RequireRolesAttribute"/> checks for roles.
/// </summary>
[Flags]
public enum RoleCheckMode
{
    /// <summary>
    /// Member is required to have none of the specified roles.
    /// </summary>
    None = 0,

    /// <summary>
    /// Member is required to have all of the specified roles.
    /// </summary>
    All = 1,

    /// <summary>
    /// Member is required to have any of the specified roles.
    /// </summary>
    Any = 2,

    /// <summary>
    /// Member is required to have exactly the same roles as specified; no extra roles may be present.
    /// </summary>
    SpecifiedOnly = 4,

    /// <summary>
    /// Instructs the check to evaluate for matching role names.
    /// </summary>
    MatchNames = 8,

    /// <summary>
    /// Instructs the check to evaluate for matching role IDs.
    /// </summary>
    MatchIds = 16
}
