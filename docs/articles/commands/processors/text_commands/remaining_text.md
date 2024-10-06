---
uid: articles.commands.command_processors.text_commands.remaining_text
title: Remaining Text
---

# Remaining Text
The default behavior for the `string` argument is to take the text between spaces, or the text that's quoted. If you want to take all the text after the previously parsed arguments, you should use the `RemainingText` attribute.

```cs
public static class PingCommand
{
    [Command("ban"), RequirePermissions(DiscordPermissions.BanMembers)]
    public static async ValueTask ExecuteAsync(CommandContext context, DiscordUser user, [RemainingText] string reason = "No reason provided.")
    {
        await context.Guild.BanMemberAsync(user, 0, reason);
        await context.RespondAsync($"Banned {user.Username} for {reason}");
    }
}
```

You can use the `RemainingText` attribute on the last argument of a command. This will take all the text after the previously parsed arguments. In the example above, the `reason` argument will take all the text after the `user` argument. If no text is left, then the default value will be used. You can use the command as such: `!ban @user Reason for ban`.