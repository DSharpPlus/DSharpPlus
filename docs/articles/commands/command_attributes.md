---
uid: articles.commands.command_attributes
title: Command Attributes
---

## Built-In Attributes

CommandsNext has a variety of built-in attributes to enhance your commands and provide some access control.
The majority of these attributes can be applied to your command methods and command groups.

- @DSharpPlus.CommandsNext.Attributes.AliasesAttribute
- @DSharpPlus.CommandsNext.Attributes.CooldownAttribute
- @DSharpPlus.CommandsNext.Attributes.DescriptionAttribute
- @DSharpPlus.CommandsNext.Attributes.CategoryAttribute
- @DSharpPlus.CommandsNext.Attributes.DontInjectAttribute
- @DSharpPlus.CommandsNext.Attributes.HiddenAttribute
- @DSharpPlus.CommandsNext.Attributes.ModuleLifespanAttribute
- @DSharpPlus.CommandsNext.Attributes.PriorityAttribute
- @DSharpPlus.CommandsNext.Attributes.RemainingTextAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireBotPermissionsAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireDirectMessageAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireGuildAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireNsfwAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireOwnerAttribute
- @DSharpPlus.CommandsNext.Attributes.RequirePermissionsAttribute
- @DSharpPlus.CommandsNext.Attributes.RequirePrefixesAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireRolesAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireUserPermissionsAttribute

## Custom Attributes

If the above attributes don't meet your needs, CommandsNext also gives you the option of writing your own!
Simply create a new class which inherits from @DSharpPlus.CommandsNext.Attributes.CheckBaseAttribute and implement the
required method.

Our example below will only allow a command to be ran during a specified year.

```cs
public class RequireYearAttribute : CheckBaseAttribute
{
    public int AllowedYear { get; private set; }

    public RequireYearAttribute(int year)
    {
        AllowedYear = year;
    }

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        return Task.FromResult(AllowedYear == DateTime.Now.Year);
    }
}
```

You'll also need to apply the `AttributeUsage` attribute to your attribute. For our example attribute, we'll set it to
only be usable once on methods.

```cs
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireYearAttribute : CheckBaseAttribute
{
    // ...
}
```

You can provide feedback to the user using the @DSharpPlus.CommandsNext.CommandsNextExtension.CommandErrored event.

```cs
private async Task Main(string[] args)
{
    var discord = new DiscordClient();
    var commands = discord.UseCommandsNext();

    commands.CommandErrored += CmdErroredHandler;
}

private async Task CmdErroredHandler(CommandsNextExtension _, CommandErrorEventArgs e)
{
    var failedChecks = ((ChecksFailedException)e.Exception).FailedChecks;
    foreach (var failedCheck in failedChecks)
    {
        if (failedCheck is RequireYearAttribute)
        {
            var yearAttribute = (RequireYearAttribute)failedCheck;
            await e.Context.RespondAsync($"Only usable during year {yearAttribute.AllowedYear}.");
        }
    }
}
```

Once you've got all of that completed, you'll be able to use it on a command!

```cs
[Command("generic"), RequireYear(2030)]
public async Task GenericCommand(CommandContext ctx, string generic)
{
    await ctx.RespondAsync("Generic response.");
}
```

![Generic Image][0]

<!-- LINKS -->
[0]:  ../../images/commands_command_attributes_01.png
