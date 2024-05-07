---
uid: articles.migration.slashcommands_to_commands
title: DSharpPlus.SlashCommands to DSharpPlus.Commands
---

## Migrating from DSharpPlus.SlashCommands to DSharpPlus.Commands

This section will focus on migrating existing code - there is a rough sketch of what to expect in new code at the end.

Before migrating to the shiny new command farmework, you should make sure to update to the latest available build of the library - migrating both at once will be considerably more challenging. Then, we'll need to do some setup.

Remove the SlashCommands reference and install the package. Then, set up a [service collection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) containing all services your commands need, as well as a logger. Then, call `BuildServiceProvider()` to obtain a service provider.

> [!IMPORTANT]
> If you want the commands extension to log anything, you must register a logger - it cannot currently use the default logger. Additionally, when designing your services, you should keep in mind that all commands are currently transient.

Now that we're ready to change our code, go to all of your command classes and remove the reference to `ApplicationCommandModule` - we won't be needing that any more.

If you previously used the functionality provided by that class, migrate it into a `CommandsExtension.CommandInvoked` event handler.

> [!NOTE]
> DSharpPlus.Commands does not currently support pre-execution events. If you were using them to control execution of the command, use an [unconditional context check](../commands/custom_context_checks#advanced-features) - more on checks later.

Next, change `InteractionContext` to `CommandContext` (or `SlashCommandContext`, if desired), change `SlashCommandAttribute` to just `CommandAttribute` and move the descriptions to their own `System.ComponentModel.DescriptionAttribute`s. The new extension will synthesize parameter names from the C# parameter names, but you can override the generated names using `ParameterAttribute`. Most other attributes follow the same naming and have been moved between namespaces or merged, such as `SlashMinValueAttribute` and `SlashMaxValueAttribute` -> `SlashMinMaxValueAttribute`.

Any localization you have will need to be factored into a localization provider and applied to the command using `InteractionLocalizerAttribute`. 

If you previously specified default required permissions, use `RequirePermissionsAttribute`, which also provides a way to specify the permissions your bot needs for the command to successfully execute. 

Now, let's talk about checks. The library provides a fair few built-in checks, both on [parameters](../commands/custom_context_checks#parameter-checks) and on commands, similar to what you're used to. However, implementing your own checks works slightly different now. Checks are now comprised of two types - the check implementation and the attribute applied to the command. For more in-depth applications, you should refer to [the dedicated article](../commands/custom_context_checks), but on surface level, paste your implementation into a check implementing `IContextCheck<TAttribute>`, change it to return error messages instead of exceptions if possible and register the check with the extension using the `AddCheck` methods while keeping the attribute applied to the command.

As for new features, DSharpPlus.Commands allows argument converters on slash commands, as well as broadening the range of available types: you can now use all integer types, additional Discord entities and more. Have a peek around `DSharpPlus.Commands.Converters`, or [implement your own converter](../commands/custom_argument_converters).

As a last step, we'll change how our commands are registered. To simply register commands, use the `AddCommands` methods on yoru `CommandsExtension`. If you wish to register commands to one guild for debugging/testing purposes, use `DebugGuildId` in `CommandsConfiguration`, and if you wish to register specific command to specific guilds, specify the IDs of those guilds in your `AddCommands` calls. It is generally recommended to not register guild-specific commands with the same name as global commands.

## Changed Names and Concepts

#### Attributes

| DSharpPlus.SlashCommands | DSharpPlus.Commands |
| ------------------------ | ------------------- |
| `MinimumLengthAttribute` and `MaximumLengthAttribute` | `SlashMinMaxLengthAttribute` |
| `MinimumAttribute` and `MaximumAttribute` | `SlashMinMaxValueAttribute` |
| `NameLocalizationAttribute` and `DescriptionLocalizationAttribute` | `InteractionLocalizerAttribute` |
| `ChoiceNameAttribute` | `ChoiceDisplayNameAttribute` |
| `DSharpPlus.SlashCommands.DescriptionAttribute` | `System.ComponentModel.DescriptionAttribute` |
| `OptionAttribute` | `ParameterAttribute` |
| `SlashCommandAttribute` and `SlashCommandGroupAttribute` | `CommandAttribute` |

#### Checks

Checks are now split into two parts, with a changed error model - refer to [the dedicated article](../commands/custom_context_checks).

#### Pre-Execution and Post-Execution Events

Pre-execution events are not currently supported, but if you used them to control execution, you can use [an unconditional check](../commands/custom_context_checks#advanced-features) instead. `CommandsExtension.CommandInvoked` serves as post-execution event. `ApplicationCommandModule` no longer exists.

#### Localization

Instead of having an attribute for each locale for the name and description of a command or parameter, you now use `InteractionLocalizerAttribute` and implement a localizer. 

#### Argument Converters

DSharpPlus.Commands supports argument converters - refer to [the dedicated article](../commands/custom_argument_converters).
