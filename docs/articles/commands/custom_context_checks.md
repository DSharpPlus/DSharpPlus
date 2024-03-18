---
uid: articles.commands.custom_context_checks
title: Custom Context Checks
---

# Custom Context Checks
Context checks are safeguards to a command that will help it to execute successfully. Context checks like `RequireGuild` or `RequirePermissions` will cause the command not to execute if the user executes the command in a DM or if the user/bot does not have the required permissions. Occasionally, you may want to create your own context checks to ensure that a command can only be executed under certain conditions.

A context check contains two important pieces:
- The attribute that will be applied to the command. This contains parameters that will be passed to the executing check.
- The check itself. This is the method that determines if the command can be executed.

# Implementing a Context Check Attribute
To create a context check, you will need to create a new attribute that inherits from `CheckBaseAttribute`. This attribute will be applied to the command method and will contain the parameters that will be passed to the check method.

```cs
public class DirectMessageUsageAttribute : ContextCheckAttribute
{
    public DirectMessageUsage Usage { get; init; }
    public DirectMessageUsageAttribute(DirectMessageUsage usage = DirectMessageUsage.Allow) => Usage = usage;
}
```

This is the attribute that you will apply to the command method.

# Implementing the Check Method
Now we're going to implement the logic which checks if the command is allowed to be executed. The `IContextCheck<T>` interface is used to define the check method. The `T` is the attribute/parameters that's associated with the command. In this case, it's the `DirectMessageUsageAttribute`.

```cs
public class DirectMessageUsageCheck : IContextCheck<DirectMessageUsageAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(DirectMessageUsageAttribute attribute, CommandContext context)
    {
        // When the command is sent via DM and the attribute allows DMs, allow the command to be executed.
        if (context.Channel.IsPrivate && attribute.Usage is not DirectMessageUsage.DenyDMs)
        {
            return ValueTask.FromResult<string?>(null);
        }
        // When the command is sent outside of DM and the attribute allows non-DMs, allow the command to be executed.
        else if (!context.Channel.IsPrivate && attribute.Usage is not DirectMessageUsage.RequireDMs)
        {
            return ValueTask.FromResult<string?>(null);
        }
        // The command was sent via DM but the attribute denies DMs
        // The command was sent outside of DM but the attribute requires DMs.
        else
        {
            string dmStatus = context.Channel.IsPrivate ? "inside a DM" : "outside a DM";
            string requirement = attribute.Usage switch
            {
                DirectMessageUsage.DenyDMs => "denies DM usage",
                DirectMessageUsage.RequireDMs => "requires DM usage",
                _ => throw new NotImplementedException($"A new DirectMessageUsage value was added and not implemented in the {nameof(DirectMessageUsageCheck)}: {attribute.Usage}")
            };

            return ValueTask.FromResult<string?>($"The executed command {requirement} but was executed {dmStatus}.");
        }
    }
}
```

As seen here, we return `null` when the command is allowed to be executed. If the command is not allowed to be executed, we return an error string which can be retrieved from the `CommandsExtension.CommandErrored` event. Then we use the check like such:

```cs
[Command("dm")]
[DirectMessageUsage(DirectMessageUsage.RequireDMs)]
public async ValueTask RequireDMs(CommandContext commandContext) => await commandContext.RespondAsync("This command was executed in a DM!");
```