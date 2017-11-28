# Command module lifespans - preventing module reuse

There are situations where you don't want to reuse a module instance, for one reason or another, usually due to 
thread-safety concerns.

CommandsNext allows command modules 2 have to lifespan modes: singleton (default), and transient. Singleton modules, as 
the name implies, are instantiated once per the entire CNext extension's lifetime. Transient modules, on the other 
hand, are instantiated for each command call. This enables you to make use of transient and scoped modules. If you're 
unsure what that means, familiarize yourself with the [dependency injection](/articles/commands/dependency_injection.html "dependency injection") 
guide.

## TODO