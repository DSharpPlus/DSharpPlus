---
uid: articles.commands.custom_context_checks
title: Custom Context Checks
---

# Custom Context Checks
Context checks are safeguards to a command that will help it to execute successfully. Context checks like `RequireGuild` or `RequirePermissions` will cause the command not to execute if the user executes the command in a DM or if the user/bot does not have the required permissions. Occasionally, you may want to create your own context checks to ensure that a command can only be executed under certain conditions.

A context check contains two important pieces:
- The attribute that will be applied to the command. This contains parameters that will be passed to the executing check.
- The check itself. This is the method that determines if the command can be executed.

## Implementing a context check attribute
Any context check needs an attribute associated with it. This attribute will be applied to your command methods and needs to inherit from `ContextCheckAttribute`. It should contain the necessary metadata your check needs to determine whether or not to execute the command. For the purposes of this article, we'll create the following attribute:

```cs
public class DirectMessageUsageAttribute : ContextCheckAttribute
{
    public DirectMessageUsage Usage { get; init; }
    public DirectMessageUsageAttribute(DirectMessageUsage usage = DirectMessageUsage.Allow) => Usage = usage;
}
```

## Implementing the context check
Now we're going to implement the logic which checks if the command is allowed to be executed. The `IContextCheck<T>` interface is used to define the check method. The `T` is the attribute that was applied to the command. In this case, it's the `DirectMessageUsageAttribute`, but it can be any check attribute - if desired, there can be multiple checks for attribute.

If the check was successful, the method should return `null`. If it was unsuccessful, the method should return a string that will then be provided
to `CommandsExtension.CommandErrored`. 

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

> [!WARNING]
> Your check may inspect the command context to get more information, but you should be careful making any API calls, especially such that may alter state such as `RespondAsync`. This is an easy source of bugs, and you should be aware of the three-second limit for initial responses to interactions.

Now, for the most important part, we need to register the check:

```cs
commandsExtension.AddCheck<DirectMessageUsageCheck>();
```

Then we use the check like such:

```cs
[Command("dm")]
[DirectMessageUsage(DirectMessageUsage.RequireDMs)]
public async ValueTask RequireDMs(CommandContext commandContext) => await commandContext.RespondAsync("This command was executed in a DM!");
```

## Parameter Checks

DSharpPlus.Commands also supports checks that target specifically one parameter. They are supplied with the present value and the metadata the extension has about the parameter, such as its default value or attributes. To implement a parameter check for your own parameter:
- Create an attribute that inherits from `ParameterCheckAttribute`.
- Have it implement `IParameterCheck<T>`.
- Register your parameter check using `CommandsExtension.AddParameterCheck<T>()`.
- Apply the attribute to your parameter.

> ![NOTE]
> You will be supplied an `object` for the parameter value. It is your responsibility to ensure the type matches what your check expects, and to either ignore or error on incorrect types.

For example, we can make a check that ensures a string is no longer than X characters. First, we create our attribute, as above:

```cs
using DSharpPlus.Commands.ContextChecks.ParameterChecks;

public sealed class MaximumStringLengthAttribute : ParameterCheckAttribute
{
    public int MaximumLength { get; private set; }
    public MaximumStringLengthAttribute(int length) => MaximumLength = length;
}
```

Then, we will be creating our check:

```cs
using DSharpPlus.Commands.ContextChecks.ParameterChecks;

public sealed class MaximumStringLengthCheck : IParameterCheck<MaximumStringLengthAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(MaximumStringLengthAttribute attribute, ParameterInfo info, CommandContext context)
    {
        if (info.Value is not string str)
        {
            return ValueTask.FromResult<string?>("The provided parameter was not a string.");
        }
        else if (str.Length >= attribute.MaximumLength)
        {
            return ValueTask.FromResult<string?>("The string exceeded the length limit.");
        }

        return ValueTask.FromResult<string?>(null);
    }
}
```

We then register it like so:

```cs
commandsExtension.AddParameterCheck<MaximumStringLengthCheck>();
```

And then apply it to our parameter:

```cs
[Command("say")]
public static async ValueTask SayAsync(CommandContext commandContext, [MaximumStringLength(2000)] string text) => await commandContext.RespondAsync(text);
```

## Advanced Features

The classes you use to implement checks participate in dependency injection, and you can request any type you previously supplied to the service provider in a public constructor. Useful applications include, but are not limited to, logging or tracking how often a command executes.

A single check class can also implement multiple checks, like so:

```cs
public class Check : IContextCheck<FirstAttribute>, IContextCheck<SecondAttribute>;
```

or even multiple different kinds of checks, like so:

```cs
public class Check : IContextCheck<FirstAttribute>, IParameterCheck<SecondAttribute>;
```

This means that all other code in that class can be shared between the two check methods, but this should be used with caution - since checks are registered per type, you lose granularity over which checks should be executed; and it means the same construction ceremony will run for both checks.

There is no limit on how many different checks can reference the same attribute, they will all be supplied with that attribute. Checks targeting `UnconditionalCheckAttribute` will always be executed, regardless of whether the attribute is applied or not. Unconditional context checks are not available for parameter checks.