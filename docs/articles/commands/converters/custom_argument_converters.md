---
uid: articles.commands.converters.custom_argument_converters
title: Custom Argument Converters
---

# Custom Argument Converters
Creating a custom argument converter isn't difficult, and is accomplished via interfaces. We're going to be creating a custom argument converter for the `Ulid` - a Universally Unique Lexicographically Sortable Identifier - which is a type that's completely compatible with a `Guid` while being much faster and contains more information to work with.

> [!WARNING]
> Previously in CommandsNext, you would implement the `IArgumentConverter<T>` interface. This is no longer the case! Each command processor will expose it's own version of the argument converter interfaces, such as `ITextArgumentConverter<T>` and `ISlashArgumentConverter<T>`.

Argument converters are command processor specific. This means that if you want to use a custom argument converter with both text commands and slash commands, you will need to implement two interfaces: `ITextArgumentConverter<T>` and `ISlashArgumentConverter<T>`. `ISlashArgumentConverter<T>` will contain the `DiscordApplicationCommandOptionType ParameterType` property, which is used on command registration. The `ITextArgumentConverter<T>` interface will contain the `bool RequiresText` property, which is used to determine if the next argument should be consumed as text. Since we want to use this argument converter with both text commands and slash commands, we're going to implement the `ISlashArgumentConverter<Ulid>` and `ITextArgumentConverter<Ulid>` interfaces. Both interfaces implement the `IArgumentConverter<T>` interface, which requires the `public Task<Optional<T>> ConvertAsync` method:

```cs
public class UlidArgumentConverter : ITextArgumentConverter<Ulid>, ISlashArgumentConverter<Ulid>
{
    public string ReadableName => "Ulid";
    public bool RequiresText => true;
    public DiscordApplicationCommandOptionType ParameterType =>
        DiscordApplicationCommandOptionType.String;

    public Task<Optional<Ulid>> ConvertAsync(ConverterContext context)
}
```

Now, a `ConverterContext` is very similar to a `CommandContext` - so much so that they both inherit from the `AbstractContext` type. The main difference is that `ConverterContext` contains more information about the current state of the conversion process. Even though the method receives a `ConverterContext`, the command processor should pass an object that inherits from that abstract class. Now, we need to implement the actual conversion logic. The `Ulid` type has a `TryParse` method that we can use to convert a string to a `Ulid`. We can use this to implement the conversion logic:

```cs
public class UlidArgumentConverter : ITextArgumentConverter<Ulid>, ISlashArgumentConverter<Ulid>
{
    public string ReadableName => "Ulid";
    public bool RequiresText => true;
    public DiscordApplicationCommandOptionType ParameterType =>
        DiscordApplicationCommandOptionType.String;

    private readonly ILogger<UlidArgumentConverter> logger;

    public UlidArgumentConverter(ILogger<UlidArgumentConverter> logger) => logger = logger;

    public Task<Optional<Ulid>> ConvertAsync(ConverterContext context)
    {
        // This should always be a string since `ISlashArgumentConverter<Ulid>.ParameterType` is
        // `DiscordApplicationCommandOptionType.String`, however we type check here as a safety measure
        // and to provide a more informative log message.
        if (context.Argument is not string value)
        {
            logger.LogInformation("Argument is not a string.");
            return Task.FromResult(Optional.FromNoValue<Ulid>());
        }

        logger.LogInformation("Attempting to convert {Value} to Ulid.", value);
        if (Ulid.TryParse(value, out var ulid))
        {
            logger.LogInformation("Successfully converted {Value} to Ulid.", value);
            return Task.FromResult(Optional.FromValue(ulid));
        }

        logger.LogInformation("Failed to convert {Value} to Ulid.", value);
        return Task.FromResult(Optional.FromNoValue<Ulid>());
    }
}
```

Since there is no asynchronous operations occuring within the conversion methods, we use `Task.FromResult`. Another thing to note is that argument converters support constructor dependency injection. Lastly, you'll also notice the use of `Optional<T>` here. When executing an argument converter, there are three possible outcomes:

1. The conversion was successful and the value is returned.
2. The conversion was unsuccessful and the value is not returned.
3. The conversion was unsuccessful due to an unexpected error, causing the converter to throw and exception.

When implementing your own command processor, you must keep the above cases in mind and handle them accordingly. By default, the `BaseCommandProcessor` will handle the above cases for you, but if you choose to implement `ICommandProcessor` directly, you must handle these cases yourself.

Next we must register our argument converter with the command processor:
```cs
TextCommandProcessor textCommandProcessor = new TextCommandProcessor();
textCommandProcessor.AddArgumentConverter<UlidArgumentConverter>();

commandsExtension.AddProcessor(textCommandProcessor);
```

Alternatively, if you have multiple argument converters to register, you can pass them all in at once via an assembly:
```cs
TextCommandProcessor textCommandProcessor = new TextCommandProcessor();
textCommandProcessor.AddArgumentConverters(typeof(Program).Assembly);

commandsExtension.AddProcessor(textCommandProcessor);
```

And that's it! We've now created a custom argument converter for the `Ulid` type. You can now use this argument converter in your commands by simply adding a parameter of type `Ulid` to your command methods. The command processor will automatically use the `UlidArgumentConverter` to convert the string to a `Ulid` for you.

```cs
[Command("ulid")]
public async ValueTask GetUlid(CommandContext commandContext, Ulid ulid) =>
    await commandContext.RespondAsync($"The Ulid is: {ulid}");
```