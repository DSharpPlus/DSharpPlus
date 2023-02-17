---
uid: articles.commands.argument_converters
title: Argument Converter
---

## Custom Argument Converter
Writing your own argument converter will enable you to convert custom types and replace the functionality of existing
converters. Like many things in DSharpPlus, doing this is straightforward and simple.

First, create a new class which implements @DSharpPlus.CommandsNext.Converters.IArgumentConverter`1 and its method 
@DSharpPlus.CommandsNext.Converters.IArgumentConverter`1.ConvertAsync(System.String,DSharpPlus.CommandsNext.CommandContext).
Our example will be a boolean converter, so we'll also pass `bool` as the type parameter for 
@DSharpPlus.CommandsNext.Converters.IArgumentConverter`1.
```cs
public class CustomArgumentConverter : IArgumentConverter<bool>
{
    public Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
    {
        if (bool.TryParse(value, out var boolean))
        {
            return Task.FromResult(Optional.FromValue(boolean));
        }           

        switch (value.ToLower())
        {
            case "yes":
            case "y":
            case "t":
                return Task.FromResult(Optional.FromValue(true));

            case "no":
            case "n":
            case "f":
                return Task.FromResult(Optional.FromValue(false));

            default:
                return Task.FromResult(Optional.FromNoValue<bool>());
        } 
    }	
}
```

Then register the argument converter with CommandContext.
```cs
var discord = new DiscordClient();
var commands = discord.UseCommandsNext();

commands.RegisterConverter(new CustomArgumentConverter());
```

Once the argument converter is written and registered, we'll be able to use it:
```cs
[Command("boolean")]
public async Task BooleanCommand(CommandContext ctx, bool boolean)
{
    await ctx.RespondAsync($"Converted to {boolean}");
}
```

![true][0]

<!-- LINKS -->
[0]:  ../../images/commands_argument_converters_01.png
