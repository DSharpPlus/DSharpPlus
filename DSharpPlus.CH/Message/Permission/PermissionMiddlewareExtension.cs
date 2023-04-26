namespace DSharpPlus.CH.Message.Permission;

public static class PermissionMiddlewareExtension
{
    public static CHConfiguration UsePermission(this CHConfiguration configuration)
    {
        configuration.AddMessageMiddleware<PermissionMiddleware>();
        return configuration;
    }
}
