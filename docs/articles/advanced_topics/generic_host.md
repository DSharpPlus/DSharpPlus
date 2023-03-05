---
uid: articles.advanced_topics.generic_host
title: Generic Host
---

# Introduction
The .NET Generic Host is a reusable, lightweight, and extensible hosting framework that provides a consistent way to host different types of .NET applications. It is designed to simplify the startup process and provide a common infrastructure for configuring, logging, dependency injection, and other common tasks required by modern applications.

It allows developers to build console applications, background services, and other types of .NET applications that can run as standalone processes, Windows services, or Docker containers, among other deployment options. By using a generic host, developers can focus on implementing the core business logic of their application rather than dealing with the infrastructure and plumbing required to host and manage it.

You can read more about Generic hosts [here](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host).

# Making a Bot using the Generic Host
## Setting up the Builder
To get started, you'll need to write some code that configures and runs your application. Here's a simple example that shows how to use the Generic Host to run a bot service:
```cs
private static async Task Main()
{
    await Host.CreateDefaultBuilder()
        .UseConsoleLifetime()
        .ConfigureServices((hostContext, services) => services.AddHostedService<BotService>())
        .RunConsoleAsync();
}
```
This code does a few things. First, it calls the `Host.CreateDefaultBuilder()` method, which creates a new `IHostBuilder` instance with some default settings. Then, it calls the `UseConsoleLifetime()` method to configure the lifetime of the host. This tells the host to keep running until it receives a [SIGINT or SIGTERM Signal](https://en.wikipedia.org/wiki/Signal_(IPC)#SIGINT) i.e. when the user presses Ctrl+C in the console, or when another program tells it to stop.

Next, it configures the services that the host will use by calling the `ConfigureServices()` method. In this case, it adds a new `BotService` service, which is a class that you'll need to define next.

Finally, it calls the `RunConsoleAsync()` method to start the host and begin running your service. That's it! With just a few lines of code, you can use the .NET Generic Host to run your application as a service.

## Setting up your Service
You'll need to create a class which implements the `IHostedService` interface, which defines two methods: `StartAsync()` and `StopAsync()`. These methods are called by the host when your application starts and stops respectively.

```cs
public sealed class BotService : IHostedService
{
    private readonly ILogger<BotService> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly DiscordClient _discordClient;

    public BotService(ILogger<BotService> logger, IHostApplicationLifetime applicationLifetime)
    {
        this._logger = logger;
        this._applicationLifetime = applicationLifetime;
        this._discordClient = new(new()
        {
            Token = "YourBotTokenHere",
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged
        });
    }

    public async Task StartAsync(CancellationToken token)
    {
        await _discordClient.ConnectAsync();
        // Other startup things here
    }

    public async Task StopAsync(CancellationToken token)
    {
        await _discordClient.DisconnectAsync();
        // More cleanup possibly here
    }
}
```
>[!WARNING]
> Hard-coding your bot token into your source code is not a good idea, especially if you plan to distribute your code publicly. This is because anyone with access to your code can easily extract your bot token, which can be used to perform unauthorized actions on your bot account.
>Instead, it's recommended that you store your bot token in a secure location, such as a configuration file, environment variable, or secret storage service. You can then retrieve the token at runtime and pass it to the initializer. See for example [How to consume configuration with the Generic Host](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0) or [how to use an environment variable](https://learn.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable?view=net-7.0). 

The `StartAsync()` method contains the code that runs when your application starts. In this case, the `DiscordClient` connects to the Discord API.

The `StopAsync()` method contains the code that runs when your application is shut down. In the case of a bot, this might involve closing connections to external APIs, releasing resources, or performing other cleanup tasks.

By implementing these methods, you can ensure that your application starts and stops cleanly, and that any necessary resources are properly managed. This makes it easier to build robust, reliable applications that can be run as services.

With this class, you can easily create and run your own bot services using the .NET Generic Host. Just replace the Token property with your own Discord bot token, and you're ready to go!

## Using Serilog with the Generic Host
Logging is an important part of any application, especially one that runs as a service. Fortunately, the .NET Generic Host makes it easy to integrate with popular logging libraries, like Serilog.

### Dependencies
You will need the [`Serilog.Extensions.Hosting`](https://www.nuget.org/packages/Serilog.Extensions.Hosting) package (along with [`Serilog`](https://www.nuget.org/packages/Serilog) itself with whichever sinks you prefer), which are available from NuGet.

### In your Service
Then, you will need to add Serilog to your DiscordClientConfiguration initializer block to ensure that your bot logs messages using Serilog. To do this, you will need to create a new `LoggerFactory` object and add the Serilog logger provider to it. You can also specify a minimum log level and silence certain DSharpPlus events, such as the "unknown event" log.
```cs
this._discordClient = new(new()
{
    [...]
    LoggerFactory = new LoggerFactory().AddSerilog(),
    MinimumLogLevel = LogLevel.Warning,
    LogUnknownEvents = false
});
```

### In your Host section
When configuring the .NET Generic Host to use Serilog, you will need to add the logger service to your host builder and call the `UseSerilog()` method to configure Serilog as your logging provider.

To do this, you can add the logger service to the `ConfigureServices()` method of your host builder, like this:
```cs
await Host.CreateDefaultBuilder()
    .UseSerilog()
    .UseConsoleLifetime()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging(logging => logging.ClearProviders().AddSerilog());
        services.AddHostedService<BotService>();
    })
    .RunConsoleAsync();
```
In this example, we call the `UseSerilog()` method to configure Serilog as our logging provider, and then add the logger service to the `ConfigureServices()` method using the `AddLogging()` method on the Services collection. We then call the `ClearProviders()` method to remove any default logging providers that may be present, and add the Serilog provider using the `AddSerilog()` method.

Don't forget that before you can use Serilog to log messages in your bot, you will need to initialize Serilog and configure the sinks you want to use. For example, you can initialize Serilog like this:
```cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

When shutting down your bot, it's a good idea to call `Log.CloseAndFlushAsync()` to make sure that any pending log messages are written to the sinks before the process exits. You can add this call to your `Main` method, which could look like this once you're done:
```cs
private static async Task Main()
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("logs/.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    await Host.CreateDefaultBuilder()
        .UseSerilog()
        .UseConsoleLifetime()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddLogging(logging => logging.ClearProviders().AddSerilog());
            services.AddHostedService<BotService>();
        })
        .RunConsoleAsync();

    await Log.CloseAndFlushAsync();
}
```
