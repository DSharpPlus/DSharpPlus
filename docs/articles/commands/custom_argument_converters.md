---
uid: articles.commands.custom_argument_converters
title: Custom Argument Converters
---

# Custom Argument Converters
Creating a custom argument converter isn't difficult, and is accomplished via interfaces. We're going to be creating a custom argument converter for the `Ulid` - Universally Unique Lexicographically Sortable Identifier - which is a type that's completely compatible with a `Guid` while being much faster and giving more information for you to work with.

Argument converters are command processor specific. This means that if you want to use a custom argument converter with both text commands and slash commands, you will need to implement similar conversion logic twice. While momentarily inconvienant, ultimately this is advantageous because it allows you to make full use of all processor-specific information. Since we want to use this argument converter with both text commands and slash commands, we're going to implement the `ISlashArgumentConverter<Ulid>` and `ITextArgumentConverter<Ulid>` interfaces:


```cs
public class UlidArgumentConverter : ITextArgumentConverter<Ulid>, ISlashArgumentConverter<Ulid>
{
    public Task<Optional<Ulid>> ConvertAsync(
        TextConverterContext context,
        MessageCreateEventArgs eventArgs
    )
    {
        throw new NotImplementedException();
    }

    public Task<Optional<Ulid>> ConvertAsync(
        InteractionConverterContext context,
        InteractionCreateEventArgs eventArgs
    )
    {
        throw new NotImplementedException();
    }
}
```

Now, a `ConverterContext` is very similar to a `CommandContext` - so much so that they both inherit from the `AbstractContext` type. The main difference is that `ConverterContext` contains more information about the current state of the conversion process. Each processor should implement it's own argument converter interfaces, and the `ConverterContext` types will be different for each processor. If you wish to consolidate your argument converters, look into transforming the data you need into a common conversion method, like such:

```cs
public class UlidArgumentConverter : ITextArgumentConverter<Ulid>, ISlashArgumentConverter<Ulid>
{
    public Task<Optional<Ulid>> ConvertAsync(
        TextConverterContext context,
        MessageCreateEventArgs eventArgs
    ) => ConvertAsync(context.Argument);

    public Task<Optional<Ulid>> ConvertAsync(
        InteractionConverterContext context,
        InteractionCreateEventArgs eventArgs
    ) => ConvertAsync(context.Argument.RawValue);

    public Task<Optional<Ulid>> ConvertAsync(string value) => throw new NotImplementedException();
}
```

Now, we need to implement the actual conversion logic. The `Ulid` type has a `TryParse` method that we can use to convert a string to a `Ulid`. We can use this to implement the conversion logic:

```cs
public class UlidArgumentConverter : ITextArgumentConverter<Ulid>, ISlashArgumentConverter<Ulid>
{
    private readonly ILogger<UlidArgumentConverter> logger;

    public UlidArgumentConverter(ILogger<UlidArgumentConverter> logger) => logger = logger;

    public Task<Optional<Ulid>> ConvertAsync(
        TextConverterContext context,
        MessageCreateEventArgs eventArgs
    ) => ConvertAsync(context.Argument);

    public Task<Optional<Ulid>> ConvertAsync(
        InteractionConverterContext context,
        InteractionCreateEventArgs eventArgs
    ) => ConvertAsync(context.Argument.RawValue);

    public Task<Optional<Ulid>> ConvertAsync(string value)
    {
        logger.LogDebug("Attempting to convert {Value} to Ulid.", value);
        if (Ulid.TryParse(value, out var ulid))
        {
            logger.LogDebug("Successfully converted {Value} to Ulid.", value);
            return Task.FromResult(Optional.FromValue(ulid));
        }

        logger.LogDebug("Failed to convert {Value} to Ulid.", value);
        return Task.FromResult(Optional.FromNoValue<Ulid>());
    }
}
```

Since there is no asynchronous operations occuring within the conversion methods, we use `Task.FromResult`. Another thing to note is that argument converters support constructor dependency injection. Lastly, you'll also notice the use of `Optional<T>` here. When executing an argument converter, there are three possible outcomes:

1. The conversion was successful and the value is returned.
2. The conversion was unsuccessful and the value is not returned.
3. The conversion was unsuccessful due to an unexpected error, causing the converter to throw and exception.

Next we must register our argument converter with the command processor:
```cs
TextCommandProcessor textCommandProcessor = new TextCommandProcessor();
textCommandProcessor.AddArgumentConverter(new UlidArgumentConverter());

await commandsExtension.AddProcessorsAsync(textCommandProcessor);
```

Alternatively, if you have multiple argument converters to register, you can pass them all in at once via an assembly:
```cs
TextCommandProcessor textCommandProcessor = new TextCommandProcessor();
textCommandProcessor.AddArgumentConverters(typeof(Program).Assembly);

await commandsExtension.AddProcessorsAsync(textCommandProcessor);
```

And that's it! We've now created a custom argument converter for the `Ulid` type. You can now use this argument converter in your commands by simply adding a parameter of type `Ulid` to your command methods. The command processor will automatically use the `UlidArgumentConverter` to convert the string to a `Ulid` for you.

```cs
[Command("ulid")]
public async ValueTask GetUlid(CommandContext commandContext, Ulid ulid) =>
    await commandContext.RespondAsync($"The Ulid is: {ulid}");
```