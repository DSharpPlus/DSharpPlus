using System;

namespace DSharpPlus.CommandAll.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
    public class PermissionsAttribute : Attribute
    {
        public Permissions BotPermissions { get; init; }
        public Permissions UserPermissions { get; init; }

        public PermissionsAttribute(Permissions permissions) => BotPermissions = UserPermissions = permissions;
        public PermissionsAttribute(Permissions botPermissions, Permissions userPermissions)
        {
            BotPermissions = botPermissions;
            UserPermissions = userPermissions;
        }
    }
}
