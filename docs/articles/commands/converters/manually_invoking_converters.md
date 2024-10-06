---
uid: articles.commands.converters.manually_invoking_converters
title: Manually Invoking Argument Converters
---

# Manually Invoking Argument Converters
Ocassionally, you may need to manually invoke an argument converter. This can be done by calling the `ConvertAsync` method on a `IArgumentConverter<T>` object. This method is asynchronous and will return a `Optional<T>` object. If the conversion was successful, the `Optional<T>` object will contain the converted value. If the conversion was unsuccessful, the `Optional<T>` object will be empty. If there was a truly unexpected error, an exception will be thrown. In order to invoke an argument converter, you must first obtain an object that implements `ConverterContext`. This object contains all the necessary information for the converter to work. Below is an example of how to manually invoke an argument converter:

```csharp
// Obtain the argument converter
IArgumentConverter<T> converter = context.Extension.GetProcessor<TextCommandProcessor>().Converters[typeof(T)];

// Create a ConverterContext object
TextConverterContext converterContext = new()
{
    Channel = Channel,
    Command = CommandsExtension.Commands["day_of_week"],
    Extension = CommandsExtension,
    Message = context.Message, // This can affect the outcome of the conversion depending on the converter!
    RawArguments = "Monday",
    ServiceScope = ServiceProvider.CreateScope(),
    Splicer = DefaultTextArgumentSplicer.Splice,
    User = User
};

// Go to the next parameter of the command
converterContext.NextParameter();

// Go to the next argument of the message
converterContext.NextArgument();

// Invoke the converter
Optional<T> result = await converter.ConvertAsync(converterContext);
```

Alternatively, if you aren't able to use generics, you can use the non-generic argument converter delegates that `BaseCommandProcessor` exposes, which all first party processors inherit from. Below is an example of how to manually invoke an argument converter without using generics:

```csharp
// Select our type
Type type = typeof(ulong);

// Obtain the argument converter
IArgumentConverter converter = context.Extension.GetProcessor<TextCommandProcessor>().ConverterDelegates[type];

// Create a ConverterContext object
TextConverterContext converterContext = new()
{
    Channel = Channel,
    Command = CommandsExtension.Commands["day_of_week"],
    Extension = CommandsExtension,
    Message = context.Message, // This can affect the outcome of the conversion depending on the converter!
    RawArguments = "Monday",
    ServiceScope = ServiceProvider.CreateScope(),
    Splicer = DefaultTextArgumentSplicer.Splice,
    User = User
};

IOptional result = await converter.ConvertAsync(converterContext);
```