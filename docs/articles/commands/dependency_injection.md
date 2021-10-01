---
uid: articles.commands.dependency_injection
title: Dependency Injection
---

## Dependency Injection
As you begin to write more complex commands, you'll find that you need a way to get data in and out of them. Although
you *could* use `static` fields to accomplish this, the preferred solution would be *dependency injection*.

This would involve placing all required object instances and types (referred to as *services*) in a container, then
providing that container to CommandsNext. Each time a command module is instantiated, CommandsNext will then attempt to
populate constructor parameters, `public` properties, and `public` fields exposed by the module with instances of
objects from the service container.

We'll go through a simple example of this process to help you understand better.

### Create a Service Provider
To begin, we'll need to create a service provider; this will act as the container for the services you need for your
commands. Create a new variable just before you register CommandsNext with your @DSharpPlus.DiscordClient and assign it
a new instance of `ServiceCollection`.
```cs
var discord = new DiscordClient();	
var services = new ServiceCollection();	// Right here!
var commands = discord.UseCommandsNext();
```

We'll use `.AddSingleton` to add type `Random` to the collection, then chain that call with the
`.BuildServiceProvider()` extension method. The resulting type will be `ServiceProvider`.
```cs
var services = new ServiceCollection()
    .AddSingleton<Random>()
	.BuildServiceProvider();
```

Then we'll need to provide CommandsNext with our services.
```cs
var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
{
    Services = services
});
```

### Using Your Services
Now that we have our services set up, we're able to use them in commands. We'll be tweaking our
[random number command][0] to demonstrate.

Add a new property to the command module named *Rng*. Make sure it has a `public` setter.
```cs
public class MyFirstModule : BaseCommandModule
{
    public Random Rng { private get; set; } // Implied public setter.

    // ...
}
```

Modify the *random* command to use our property.
```cs
[Command("random")]
public async Task RandomCommand(CommandContext ctx, int min, int max)
{
    await ctx.RespondAsync($"Your number is: {Rng.Next(min, max)}");
}
```

Then we can give it a try!

![Command Execution][1]

CommandsNext has automatically injected our singleton `Random` instance into the `Rng` property when our command module
was instantiated. Now, for any command that needs `Random`, we can simply declare one as a property, field, or in the
module constructor and CommandsNext will take care of the rest. Ain't that neat?

## Lifespans
### Modules
By default, all command modules have a singleton lifespan; this means each command module is instantiated once for the
lifetime of the CommandsNext instance. However, if the reuse of a module instance is undesired, you also have the option
to change the lifespan of a module to *transient* using the @DSharpPlus.CommandsNext.Attributes.ModuleLifespanAttribute.
```cs
[ModuleLifespan(ModuleLifespan.Transient)]
public class MyFirstModule : BaseCommandModule
{
    // ...
}
```

Transient command modules are instantiated each time one of its containing commands is executed.

### Services
In addition to the `.AddSingleton()` extension method, you're also able to use the `.AddScoped()` and `.AddTransient()`
extension methods to add services to the collection. The extension method chosen will affect when and how often the
service is instantiated. Scoped and transient services should only be used in transient command modules, as singleton
modules will always have their services injected once.

Lifespan  | Instantiated
:--------:|:-------------
Singleton | One time when added to the collection.
Scoped    | Once for each command module.
Transient | Each time its requested.

<!-- LINKS -->
[0]:  xref:articles.commands.intro#argument-converters
[1]:  /images/commands_dependency_injection_01.png
