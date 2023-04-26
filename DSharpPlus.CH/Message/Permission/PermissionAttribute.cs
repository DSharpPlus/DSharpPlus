namespace DSharpPlus.CH.Message.Permission
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MessagePermissionAttribute : Attribute
    {
        public DSharpPlus.Permissions Permissions { get; set; }

        public MessagePermissionAttribute(DSharpPlus.Permissions permissions)
        {
            Permissions = permissions;
        }
    }
}