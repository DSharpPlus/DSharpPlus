# Dependency injection - passing data around

In a situation where you need to pass objects in and out of your command modules, you need a way to access that data. 
Dependency injection provides a convenient and safe (not to mention the only correct) way of passing data to your 
modules. 

You use dependency injection by first creating a service provider, then supplying it to your @DSharpPlus.CommandsNext.CommandsNextConfiguration 
instance via @DSharpPlus.CommandsNext.CommandsNextConfiguration.Services property. The objects you placed in the 
service provider will then be injected into your modules when they are instantiated. During injection, CommandsNext 
first injects objects via constructor (i.e. it will try to match any constructor parameter to service types in the 
provider. If it fails, it will throw. Up next, any public writable properties are populated with services from the 
provider. If a suitable service is not found, the property is not injected. The process is then repeated for public 
writable fields. To prevent specific properties or fields from being injected, you can put the @DSharpPlus.CommandsNext.Attributes.DontInjectAttribute 
over them.

This is useful in a scenario when you have any kind of data that you need to be persistent or accessible from command 
modules, such as settings classes, entity framework database contexts, and so on.

## 1. Creating a service provider

If you go back to the basic CommandsNext example, you will remember the `random` command. Let's amend it, and make 
use of shared `Random` instance (note that reusing `Random` instances is generally not a good idea; here it's done for 
the sake of the example).

Before you enable your CommandsNext module, you will need to create a new `ServiceCollection` instance, then add a 
singleton `Random` instance to it, and finally, build a service provider out of it. You can do it like so:

```cs
var deps = new ServiceCollection()
	.AddSingleton(new Random())
	.BuildServiceProvider();
```

Don't forget to add `using Microsoft.Extensions.DependencyInjection;` to your usings.

You then need to pass the resulting provider to your CommandsNext configuration. Amend it like so:

```cs
commands = discord.UseCommandsNext(new CommandsNextConfiguration
{
	StringPrefix = ";;",
	Services = deps
});
```

## 2. Amending the command module

Go to your command module, and give it a read-only property called Rng, of type Random:

```cs
public Random Rng { get; }
```

Now create a constructor for the module, which takes an instance of Random as an argument:

```cs
public MyCommands(Random rng)
{
	Rng = rng;
}
```

And finally edit the `Random` command to look like this:

```cs
[Command("random")]
public async Task Random(CommandContext ctx, int min, int max)
{
	await ctx.RespondAsync($"🎲 Your random number is: {Rng.Next(min, max)}");
}
```

When you invoke `;;random 1 10` now, it will use the shared `Random` instance.

## 3. Further notes

While the service collection can hold singletons, it can hold transient and scoped instances as well. The difference is 
that singleton instances are instantantiated once (when added to the collection), scoped are instantiated once per 
module instantiation, and transients are instantiated every time they are requested.

Combined with transient module lifespans, injecting an entity framework database context as a transient or scoped 
service makes working with databases easier, as an example.

Note that if a module has singleton lifespan, all services will be injected to it once. Only transient modules take 
advantage of scoped and transient services.