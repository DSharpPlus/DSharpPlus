using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Trees.Predicates;

/// <summary>
/// Checks whether the executing user has the required permissions. This will be enregistered with discord for application commands.
/// </summary>
public class UserPermissionPredicate : ICommandExecutionPredicate
{
    /// <summary>
    /// The set of permissions required for users to execute this command.
    /// </summary>
    public DiscordPermissions RequiredPermissions { get; init; }

    public UserPermissionPredicate(DiscordPermissions requiredPermissions) => this.RequiredPermissions = requiredPermissions;

    // note: this should be special-cased for application commands before merging, it is, as it stands, incorrect.
    /// <inheritdoc/>
    public bool IsFulfilled(AbstractContext context) 
        => context.Member is null || context.Member.PermissionsIn(context.Channel).HasAllPermissions(this.RequiredPermissions);
}
