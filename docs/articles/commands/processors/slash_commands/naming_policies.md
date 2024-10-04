---
uid: articles.commands.command_processors.slash_commands.naming_policies
title: Interaction Naming Policies
---

# Naming Policies

By default when registering your slash command and parameter names, they will be changed to `snake_case`. This is due to Discord's naming restrictions for interaction-based data. While snake_casing the names is one of the few ways to consistently follow Discord's naming policies, it is not the only way. Some other options include kebab-casing and lowercasing. Maybe there's a third option that you'd like to use, but isn't implemented by default! Let's go over the interface that controls all of this: `IInteractionNamingPolicy`:

```cs
public interface IInteractionNamingPolicy
{
    string GetCommandName(Command command);
    string GetParameterName(CommandParameter parameter, int arrayIndex);
    StringBuilder TransformText(ReadOnlySpan<char> text);
}
```

There are currently three default implementations of this interface:
- `SnakeCaseInteractionNamingPolicy`
- `KebabCaseInteractionNamingPolicy`
- `LowerCaseInteractionNamingPolicy`

You can also create your own implementation of this interface if you want to use a different naming policy. Here's an example of a naming policy that converts any multi-argument parameter names to their ordinal numeric form (1 -> first, 2 -> second, etc.):

```cs
using Humanizer;

public class OrdinalSnakeCaseInteractionNamingPolicy : IInteractionNamingPolicy
{
    private static readonly SnakeCaseInteractionNamingPolicy _snakeCasePolicy = new SnakeCaseInteractionNamingPolicy();

    public string GetCommandName(Command command) => _snakeCasePolicy.GetCommandName(command);

    public string GetParameterName(CommandParameter parameter, int arrayIndex)
    {
        if (string.IsNullOrWhiteSpace(parameter.Name))
        {
            throw new InvalidOperationException("Parameter name cannot be null or empty.");
        }

        StringBuilder stringBuilder = TransformText(parameter.Name);
        if (arrayIndex > -1)
        {
            // Prepend the ordinal number to the parameter name
            // first_parameter_name, second_parameter_name, etc.
            stringBuilder.Insert(0, (arrayIndex + 1).ToOrdinalWords() + "_");
        }

        return stringBuilder.ToString();
    }

    public StringBuilder TransformText(ReadOnlySpan<char> text) => _snakeCasePolicy.TransformText(text);
}
```

> [!NOTE]
> Humanizer is not a dependency of DSharpPlus and is not affiliated with DSharpPlus. You can find more information about Humanizer [here](https://github.com/Humanizr/Humanizer).

Now that you have your custom naming policy, you can use it when setting up the Commands extension:

```cs
serviceCollection.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) => {
    SlashCommandProcessor slashCommandProcessor = new(new SlashCommandConfiguration()
    {
        NamingPolicy = new OrdinalSnakeCaseInteractionNamingPolicy(),
    });

    extension.AddProcessor(slashCommandProcessor);
});
```

And now your commands should be registered with the naming format you've chosen:

[!A screenshot of the `/quote` command, listing several messages with the following parameter names: `first_message_link`, `second_message_link`, `third_message_link`, `fourth_message_link` and `fifth_message_link`.](../../images/commands/processors/slash_commands/naming_policies/ordinal_snake_case.png)