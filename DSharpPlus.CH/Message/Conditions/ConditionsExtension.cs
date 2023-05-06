namespace DSharpPlus.CH.Message.Conditions;

public static class ConditionsExtension
{
    public static CHConfiguration UsePermission(this CHConfiguration configuration,
        PermissionConditionConfiguration? permissionConfiguration = null)
    {
        PermissionCondition.Configuration = permissionConfiguration ?? new()
        {
            MessageFunc = (ctx) =>
            {
                Entities.DiscordMessageBuilder msgBuilder = new();
                msgBuilder.WithReply(ctx.Message.Id);
                msgBuilder.WithContent("You do not have enough permissions to use this command.");
                return msgBuilder;
            },
        };
        configuration.UseMessageCondition<PermissionCondition>();
        return configuration;
    }

    public static CHConfiguration UseCooldown(this CHConfiguration configuration)
    {
        configuration.UseMessageCondition<CooldownCondition>();
        return configuration;
    }

    public static CHConfiguration UseRequireGuild(this CHConfiguration configuration)
    {
        configuration.UseMessageCondition<RequireGuildCondition>();
        return configuration;
    }

    public static CHConfiguration UseStandardConditions(this CHConfiguration configuration)
    {
        configuration.UseRequireGuild();
        configuration.UsePermission();
        configuration.UseCooldown();
        return configuration;
    }
}
