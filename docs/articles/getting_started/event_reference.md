# Understanding and using events in DSharpPlus

The events in DSharpPlus use the Task Asynchronous Pattern to execute. In essence, this means that all event handler methods 
must return a `Task`.

## TAP versus `async void`

DSharpPlus is a largely asynchronous library, and, as such, most interactions with Discord API happen asynchronously.

.NET offers `async void` methods for asynchronous event handlers, however this method has numerous major flaws, most notably 
the fact that these methods offer no control over their execution, which means that once one of these methods is fired, you 
don't know whether the execution has finished, or not. Another major flaw is that should an unhandled exception occur in such 
a method, the entire CLR will crash, as you cannot catch that exception. Such exceptions are also hard to debug, as they often 
appear as something completely unrelated during runtime.

To this end, DSharpPlus offers TAP-based events. They come in 2 flavours: parametrised, and parameter-less.

## Parameter-less event handlers

Parameter-less events take no arguments, and have to return a `Task`. The handlers themselves can be `async`, and they can 
use `await` inside. You can use them in 4 different ways:

You can create asynchronous anonymous methods to attach as event handlers, for example:

```cs
discord.Event += async () =>
{
	await SomethingAsync();
}

discord.Event += () =>
{
	// non-async code here
	return Task.CompletedTask; // or Task.Delay(0); if targeting .NET 4.5.x
}

discord.Event += MyEventHandlerMethod;
// later 
async Task MyEventHandlerMethod()
{
    await SomethingAsync();
}

discord.Event += MyEventHandlerMethod;
// later
Task MyEventHandlerMethod()
{
    // non-async code here
	return Task.CompletedTask; // or Task.Delay(0); if targeting .NET 4.5.x
}
```

## Parameterized event handlers

This is largely similar to parameter-less, except these event handlers take appropriate `EventArgs`, that are derived from 
@DSharpPlus.EventArgs.DiscordEventArgs.

```cs
discord.MessageCreated += async e =>
{
	await e.Message.RespondAsync("Hi");
}

discord.MessageCreated += () =>
{
	// non-async code here
	return Task.CompletedTask; // or Task.Delay(0); if targeting .NET 4.5.x
}

discord.MessageCreated += MyEventHandlerMethod;
// later 
async Task MyEventHandlerMethod(MessageCreatedEventArgs e)
{
    await e.Message.RespondAsync("Hi");
}

discord.MessageCreated += MyEventHandlerMethod;
// later
Task MyEventHandlerMethod()
{
    // non-async code here
	return Task.CompletedTask; // or Task.Delay(0); if targeting .NET 4.5.x
}
```

## Preventing further event handlers from running

Parametrized asynchronous events take instances that derive from @DSharpPlus.AsyncEventArgs. This means they 
have a @DSharpPlus.AsyncEventArgs.Handled property, which, if set to `true` prevents further event handlers 
from executing.