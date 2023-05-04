namespace DSharpPlus.CH.Message.Permission;

public static class PermissionMiddlewareExtension
{
    public static CHConfiguration UsePermission(this CHConfiguration configuration)
    {
        configuration.AddMessageCondition<PermissionCondition>();
        return configuration;
    }
}
