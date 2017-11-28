# Augmenting commands - custom argument converters

Suppose you want to augment an existing argument converter, or introduce a new one. The Argument Converter system in 
CommandsNext allows you to modify or register converters for arguments passed to commands. An argument converter for 
type `T` is a class which implements @DSharpPlus.CommandsNext.Converters.IArgumentConverter`1 and has an 
implementation for @DSharpPlus.CommandsNext.Converters.IArgumentConverter`1.TryConvert(System.String,DSharpPlus.CommandsNext.CommandContext,`0@) method.

Here's we'll be creating an augmented boolean converter.

## 1. Creating a converter

Create a new class, call it `EnhancedBoolConverter`, and make it implement @DSharpPlus.CommandsNext.Converters.IArgumentConverter`1
with generic argument set to bool (`IArgumentConverter<bool>`).

In the `TryConvert` method, you will want to add code which checks if the `value` is equal to `"yes"` or `"no"`, and return 
appropriate value. Otherwise it should fallback to default bool parser. It should look more or less like this:

```cs
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;

namespace MyFirstBot
{
    public class EnhancedBoolConverter : IArgumentConverter<bool>
    {
        public bool TryConvert(string value, CommandContext ctx, out bool result)
        {
            switch (value.ToLowerInvariant())
            {
                case "yes":
                    result = true;
                    return true;

                case "no":
                    result = false;
                    return true;
            }

            return bool.TryParse(value, out result);
        }
    }
}
```

## 2. Registering the converter

Once your converter is created, you need to register it with CommandsNext. You can do that by invoking 
@DSharpPlus.CommandsNext.CommandsNextExtension.RegisterConverter``1(IArgumentConverter{``0}) with the converter instance:

```cs
commands.RegisterConverter(new EnhancedBoolConverter());
```

You need to call the method before you register your commands.

## 3. Making use of the converter

Create a new command which takes a bool argument to test the converter:

```cs
[Command("yesno")]
public async Task YesNo(CommandContext ctx, bool arg)
{
	await ctx.RespondAsync($"Your pick: {arg ? "Yes" : "No"}");
}
```

You can now invoke it as `;;yesno yes` or `;;yesno no`.

## 4. Further notes

This particular example replaces an existing converter with a new one. You can also register converters for other types 
(including custom ones). All you need is a converter instance for it.

You can also give types a user-friendly name (that is used for help) by invoking the @DSharpPlus.CommandsNext.CommandsNextExtension.RegisterUserFriendlyTypeName``1(System.String) 
method, e.g.:

```cs
commands.RegisterUserFriendlyTypeName<MyType>("my custom data");
```