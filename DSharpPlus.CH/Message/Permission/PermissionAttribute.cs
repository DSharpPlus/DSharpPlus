namespace DSharpPlus.CH.Message.Permission;

[AttributeUsage(AttributeTargets.Method)]
public class MessagePermissionAttribute : Attribute
{
    public Permissions Permissions { get; set; }

    public MessagePermissionAttribute(Permissions permissions) => Permissions = permissions;
}
