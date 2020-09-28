---
uid: commands_command_attributes
title: Command Attributes
---

## Built-In Attributes
CommandsNext has a variety of built-in attributes to enhance your commands and provide some access control.
The majority of these attributes can be applied to your command methods and command groups.

- @DSharpPlus.CommandsNext.Attributes.AliasesAttribute
- @DSharpPlus.CommandsNext.Attributes.CooldownAttribute
- @DSharpPlus.CommandsNext.Attributes.DescriptionAttribute
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
Simply create a new class which inherits from `CheckBaseAttribute` and implement the required method.

Our example below will only allow a command to be ran during a specified year.
```cs
public class RequireYearAttribute : CheckBaseAttribute
{
    public int AllowedYear { get; private set; }

    public RequireYearAttribute(int year)
    {
        AllowedYear = year;
    }

    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (AllowedYear != DateTime.Now.Year)
        {
            await ctx.RespondAsync($"Only usable during year {AllowedYear}.");
            return false; // if non-async method: Task.FromResult(false)
        }
		
        return true; // if non-async method: Task.FromResult(true)
    }
}
```

<br/>
You'll also need to apply the `AttributeUsage` attribute to your attribute.<br/>
For our example attribute, we'll set it to only be usable once on methods.
```cs
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireYearAttribute : CheckBaseAttribute
{
    // ...
}
```

<br/>
Once you've got all of that completed, you'll be able to use it on a command!
```cs
[Command("generic"), RequireYear(2030)]
public async Task GenericCommand(CommandContext ctx, string generic)
{
    await ctx.RespondAsync("Generic response.");
}
```

![Generic Image](/images/commands_command_attributes_01.png)
