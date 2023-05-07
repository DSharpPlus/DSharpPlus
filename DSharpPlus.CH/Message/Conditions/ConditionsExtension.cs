using DSharpPlus.CH.Internals;

namespace DSharpPlus.CH.Message.Conditions;

public static class ConditionsExtension
{
    public static CommandController UsePermission(this CommandController configuration,
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

    public static CommandController UseCooldown(this CommandController configuration)
    {
        configuration.UseMessageCondition<CooldownCondition>();
        return configuration;
    }

    public static CommandController UseRequireGuild(this CommandController configuration)
    {
        configuration.UseMessageCondition<RequireGuildCondition>();
        return configuration;
    }

    public static CommandController UseStandardConditions(this CommandController configuration)
    {
        configuration.UseRequireGuild();
        configuration.UsePermission();
        configuration.UseCooldown();
        return configuration;
    }
}
