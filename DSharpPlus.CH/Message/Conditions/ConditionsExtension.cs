namespace DSharpPlus.CH.Message.Conditions;

public static class ConditionsExtension
{
    public static CHConfiguration UsePermission(this CHConfiguration configuration)
    {
        configuration.AddMessageCondition<PermissionCondition>();
        return configuration;
    }
}
