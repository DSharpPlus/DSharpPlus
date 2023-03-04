---
uid: articles.advanced_topics.generic_host
title: Generic Host
---

# Introduction
A .NET Generic Host is a reusable, lightweight, and extensible hosting framework that provides a consistent way to
host different types of .NET applications. It is designed to simplify the startup process and provide a common
infrastructure for configuring, logging, dependency injection, and other common tasks required by modern applications.

It allows developers to build console applications, background services, and other types of .NET applications that can
run as standalone processes, Windows services, or Docker containers, among other deployment options. By using a generic host,
developers can focus on implementing the core business logic of their application rather than dealing with the infrastructure
and plumbing required to host and manage it.

You can read more about Generic hosts [here](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host).

# Making a Bot using the Generic Host
## Setting up the Builder
To get started, you'll need to write some code that configures and runs your application. Here's a simple example that shows
how to use the Generic Host to run a bot service:
```cs
private static async Task Main() {
  await Host.CreateDefaultBuilder()
    .UseConsoleLifetime()
    .ConfigureServices((HostContext, Services) => {
      Services.AddHostedService<YourBotHost>();
    })
    .RunConsoleAsync();
}
```
This code does a few things. First, it calls the `Host.CreateDefaultBuilder()` method, which creates a new `IHostBuilder` instance
with some default settings. Then, it calls the `UseConsoleLifetime()` method to configure the lifetime of the host. This tells
the host to keep running until it receives a [SIGINT Signal](https://en.wikipedia.org/wiki/Signal_(IPC)#SIGINT) i.e. when the user
presses Ctrl+C in the console, or when another program tells it to stop.

Next, it configures the services that the host will use by calling the ConfigureServices() method. In this case, it adds a new `YourBotHost`
service, which is a class that you'll need to define next.

Finally, it calls the `RunConsoleAsync()` method to start the host and begin running your service. That's it! With just a few lines of code,
you can use the .NET Generic Host to run your application as a service.

## Setting up your Service
You'll need to create a class which implements the `IHostedService` interface, which defines two methods: `StartAsync()` and `StopAsync()`.
These methods are called by the host when your application starts and stops, respectively.

```cs
internal class YourBotHost : IHostedService {
  private readonly ILogger<YourBotHost> Logger;
  private readonly IHostApplicationLifetime AppLifetime;
  private readonly DiscordClient Discord;

  public YourBotHost(ILogger<YourBotHost> Logger, IHostApplicationLifetime AppLifetime) {
    this.Logger = Logger;
    this.AppLifetime = AppLifetime;
    this.Discord = new(new() {
      Token = "YourBotTokenHere",
      TokenType = TokenType.Bot,
      Intents = DiscordIntents.AllUnprivileged
    });
  }

  public async Task StartAsync(CancellationToken Token) {
    await Discord.ConnectAsync();
    return;
  }

  public async Task StopAsync(CancellationToken Token) {
    await Discord.DisconnectAsync();
    return;
  }
}
```
The `StartAsync()` method contains the code that runs when your application starts. In this case, the `DiscordClient` connects to the Discord API.

The `StopAsync()` method contains the code that runs when your application is shut down. In the case of a bot, this might involve closing
connections to external APIs, releasing resources, or performing other cleanup tasks.

By implementing these methods, you can ensure that your application starts and stops cleanly, and that any necessary resources are properly managed.
This makes it easier to build robust, reliable applications that can be run as services.

With this class, you can easily create and run your own bot services using the .NET Generic Host. Just replace the Token property with your
own Discord bot token, and you're ready to go!

## Using Serilog with the Generic Host
Logging is an important part of any application, especially one that runs as a service. Fortunately, the .NET Generic Host makes it easy to integrate
with popular logging libraries, like Serilog.

### Dependencies
You will need the `Serilog.Extensions.Hosting` package (and obviously Serilog itself, with whichever sinks you'd like to use), which is available from NuGet.

### In your Service
Then, you will need to add Serilog to your DiscordBot initializer block to ensure that your bot logs messages using Serilog. To do this, you will need to
create a new `LoggerFactory` object and add the Serilog logger provider to it. You can also specify a minimum log level and tell D#+ to shut up about unknown
events, if you'd like.
```cs
this.Discord = new(new() {
  [...]
  LoggerFactory = new LoggerFactory().AddSerilog(),
  MinimumLogLevel = LogLevel.Warning,
  LogUnknownEvents = false
});
```

### In your Host section
When configuring the .NET Generic Host to use Serilog, you will need to add the logger service to your host builder and call the `UseSerilog()` method to
configure Serilog as your logging provider.

To do this, you can add the logger service to the `ConfigureServices()` method of your host builder, like this:
```cs
await Host.CreateDefaultBuilder()
  .UseSerilog()
  .UseConsoleLifetime()
  .ConfigureServices((HostContext, Services) => {
    Services.AddLogging(Logging => Logging.ClearProviders().AddSerilog());
    Services.AddHostedService<BotHost>();
  })
  .RunConsoleAsync();
```
In this example, we call the `UseSerilog()` method to configure Serilog as our logging provider, and then add the logger service to the `ConfigureServices()`
method using the `AddLogging()` method on the Services collection. We then call the `ClearProviders()` method to remove any default logging providers that
may be present, and add the Serilog provider using the `AddSerilog()` method.

Don't forget that before you can use Serilog to log messages in your bot, you will need to initialize Serilog and configure the sinks you want to use.
For example, you can initialize Serilog like this:
```cs
Log.Logger = new LoggerConfiguration()
  .WriteTo.Console()
  .WriteTo.File($"Logs{Path.DirectorySeparatorChar}.log", rollingInterval: RollingInterval.Day)
  .CreateLogger();
```

When shutting down your bot, it's a good idea to call `Log.CloseAndFlushAsync()` to make sure that any pending log messages are written to the sinks
before the process exits. You can add this call to your `Main` method, which could look like this once you're done:
```cs
private static async Task Main() {
  Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"Logs{Path.DirectorySeparatorChar}.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

  await Host.CreateDefaultBuilder()
    .UseSerilog()
    .UseConsoleLifetime()
    .ConfigureServices((HostContext, Services) => {
      Services.AddLogging(Logging => Logging.ClearProviders().AddSerilog());
      Services.AddHostedService<BotHost>();
    })
    .RunConsoleAsync();

  await Log.CloseAndFlushAsync();
}
```
