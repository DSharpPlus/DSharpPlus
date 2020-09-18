---
uid: beyond_basics_logging
title: Logging
---

# Logging

## The Default Implementation
DSharpPlus ships with a default logger that'll send log messages to the console. It'll be enabled automatically with no setup required.
This implementation is rather basic and only has two configurable options.

### Minimum Logging Level
This will determine the verbosity of logging. It can be set in your `DiscordConfiguration`.
```cs
new DiscordConfiguration()
{
    MinimumLogLevel = LogLevel.Debug
};
```
The example above will display level log messages that are higher than or equal to `Debug`.

### Timestamp Format
Exactly what it says on the tin. This can be set in your `DiscordConfiguration`.<br/>
The format specified in the example below would result in *`Sep 17 2020 06:28:48`*.
```cs
new DiscordConfiguration()
{
    LogTimestampFormat = "MMM dd yyyy hh:mm:ss"
};
```
For a list of all available format specifiers, check out the MSDN page for 
[custom date and time format strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings#day-d-format-specifier).


## Using a Third Party Implementation
While the default logging implementation will meet the needs of most, some may desire to make use of a more robust implementation which provides more features.
Thankfully, DSharpPlus allows you to use any logging library which has an implementation for the [logging abstractions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging) provided by Microsoft.

[Serilog](https://serilog.net/), one of the more popular logging libraries, will be used to demonstrate.

<br/>
We'll need to install both the `Serilog` and `Serilog.Extensions.Logging` packages from NuGet, along with at least one of the many available 
[sinks](https://github.com/serilog/serilog/wiki/Provided-Sinks). This example will only use the `Serilog.Sinks.Console` sink.

Start off by creating a new `LoggerConfiguration` instance, slap `.WriteTo.Console().CreateLogger()` onto the end of it, then directly assign that to the static `Logger` property on the `Log` class.
```cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
```
You'll be able to tweak things such as the output formatting, minimum log level, and color theme using the many parameters for the `Console` method extension. 
Additionally, if you have other sinks you'd like to use, you'll register each one here using its approprate extension method and configure it accordingly.

Next, create a new variable and assign it a new `LoggerFactory` instance which calls `AddSerilog()`.
```cs
var logFactory = new LoggerFactory().AddSerilog();
```

Then assign that variable to the `LoggerFactory` property of your of `DiscordConfiguration`.
```cs
new DiscordConfiguration()
{
    LoggerFactory = logFactory
}
```

<br/>
Altogether, you'll have something similar to this:
```cs
using Microsoft.Extensions.Logging;
using Serilog;

public async Task MainAsync()
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();

    var logFactory = new LoggerFactory().AddSerilog();
    var discord = new DiscordClient(new DiscordConfiguration()
    {
        LoggerFactory = logFactory
    });
}

```

And that's it! If you now run your bot, you'll see DSharpPlus log messages formatted and displayed by Serilog.

![Console](/images/beyond_basics_logging_01.png)


## Writing a Custom Implementation
If neither the [default logger](#the-default-implementation) nor any of the [third party](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#third-party-logging-providers)
loggers meet your needs, you might consider writing your own implementation. 
This would grant you *a lot* of control over the formatting and output destination.

Some examples of superfluous stuff you could do:

* Color every log message bright yellow
* Send each log message directly to your printer
* Direct your log messages to a speech synthesizer

The only real limit is your ~~programming experience~~ imagination!

 >[!NOTE]
 > In the examples below, we'll be sending log messages to the console to give you an idea of what to do.<br/>
 > The logic for each method will ultimately be up to *you* since it'll be *your* implementation.

### Preparation
First, install the `Microsoft.Extensions.Logging.Abstractions` package from NuGet.

![NuGet Package Manager](/images/beyond_basics_logging_02.png "Latest stable version")

Then, to keep things organized, create a new folder named `Logging`.<br/>
We'll need two classes within that folder: `MyFirstLogger` and `MyFirstLoggerFactory`.

![Solution Explorer](/images/beyond_basics_logging_03.png)

### Basic Logger Implementation
The `MyFirstLogger` class will implement `ILogger`.

![Implement Interface](/images/beyond_basics_logging_04.png)

<br/>
We'll tackle the `Log` method to start things off. 


Your goal is to format the parameters the way you'd like then output to your desired destination.<br/>
Be sure to use the `formatter` function parameter to convert `state` and `exception` to a usable log string.

```cs
public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
{
    if (!IsEnabled(logLevel)) return; // Explained below.

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write($"<{DateTime.Now:MMM dd yyyy hh:mm:ss} - ");
    Console.Write(logLevel switch
    {                
        LogLevel.Information => "INFO > ",
        LogLevel.Warning => "WARN > ",
        LogLevel.Critical => "CRIT > ",
        _ => $"{logLevel.ToString().ToUpper()}> "
    });

    Console.ForegroundColor = ConsoleColor.White;
    var logMsg = formatter(state, exception);
    Console.WriteLine(logMsg);                      
}

```

<br/>
Next, we'll take care of `IsEnabled`. This method should be called by your `Log` method, as shown above.

You'll be returning a `bool` indicating whether or not your logger can output messages for a specific log level. 
This'll usually mean comparing the `logLevel` parameter against a minimum log level (we'll touch on this in the section [below](#passing-data-to-your-implementation)).
To keep things simple here, our example will just return `true` for all log levels.
```cs
public bool IsEnabled(LogLevel logLevel)
{
    return true;
}
```

<br/>
The last method is `BeginScope`. 

DSharpPlus never calls this method and, on top of that, most of you reading this have probably have never
heard of *scopes* in the context of logging; this means we're safe to throw `NotImplementedException`.
```cs
public IDisposable BeginScope<TState>(TState state)
{
    throw new NotImplementedException();
}
```
If you want to read up on scopes, check out [this article](https://dotnetcoretutorials.com/2018/04/12/using-the-ilogger-beginscope-in-asp-net-core/ ".NET Core Tutorials").

<br/>
Altogether, you should have something resembling the following:
```cs
using System;
using Microsoft.Extensions.Logging;

public class MyFirstLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
	    if (!IsEnabled(logLevel)) return;
	
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"<{DateTime.Now:MMM dd yyyy hh:mm:ss} - ");
        Console.Write(logLevel switch
        {                
            LogLevel.Information => "INFO > ",
            LogLevel.Warning => "WARN > ",
            LogLevel.Critical => "CRIT > ",
            _ => $"{logLevel.ToString().ToUpper()}> "
        });

        Console.ForegroundColor = ConsoleColor.White;
        var logMsg = formatter(state, exception);
        Console.WriteLine(logMsg);                      
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}
```


### Basic Logger Factory Implementation
The `MyFirstLoggerFactory` class will implement `ILoggerFactory`.

![Implement Interface](/images/beyond_basics_logging_05.png)

<br/>
We'll start off with the `CreateLogger` method.

You're intended to setup and initialize your implementation of `ILogger` in this method.<br/>
`MyFirstLogger` is a *very* basic implementation, so it won't require much setup at all.
```cs
public ILogger CreateLogger(string categoryName)
{    
    return new MyFirstLogger();
}
```

<br/>
Now, take a look at `AddProvider`.

With this method, you're intended to accept and consume implementations of `ILoggerProvider`.<br/>
However, you're safe to throw `NotImplementedException` since DSharpPlus never calls this method.
```cs
public void AddProvider(ILoggerProvider provider)
{
    throw new NotImplementedException();
}
```

<br/>
We'll then finish things off with `Dispose`.

We're required to implement this method because `ILoggerFactory` implements `IDisposable`.<br/>
As with the method above, DSharpPlus does not call this method and neither does the runtime.

Although you'd be okay to simply leave the method empty, it's good practice to implement a `Dispose` method when you're required, 
*especially* if you're planning to expand upon this implementation and add more features.

To start, append a `private` field variable of type `bool` to the top of your class and name it `_isDisposed`. 
```cs
internal class MyFirstLoggerFactory : ILoggerFactory
{
    private bool _isDisposed;

    // ...
}
```

Write an `if` statement for the `Dispose` method that'll `return` if `_isDisposed` is true.<br/>
You'll want to follow that up with `_isDisposed = true;` right at the bottom of the method.
```cs
public void Dispose()
{
    if (_isDisposed) return;
	_isDisposed = true;
}
```

Then, append an `if` statement to `CreateLogger` that'll throw an exception if `_isDisposed` is true.
```cs
public ILogger CreateLogger(string categoryName)
{   
    if (_isDisposed) throw new InvalidOperationException("Object disposed."); 
    return new MyFirstLogger();
}
```

<br/>
Our finished product should look something like this:
```cs
using System;
using Microsoft.Extensions.Logging;

internal class MyFirstLoggerFactory : ILoggerFactory
{
    private bool _isDisposed;
	
	public ILogger CreateLogger(string _)
    {
        if (_isDisposed) throw new InvalidOperationException("Object disposed.");
        return new MyFirstLogger();
    }

    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        if (_isDisposed) return;
	    _isDisposed = true;
    }
}
```

### Using Your Logger With DSharpPlus
This is the easy part! To have DSharpPlus use your logging implementation, simply assign a new instance of 
your logger factory implementation to the `LoggerFactory` property of your instance of `DiscordConfiguration`.
```cs
new DiscordConfiguration()
{
    LoggerFactory = new MyFirstLoggerFactory()
}
```
Hit `F5` to run your bot and see your brand new logging implementation!

![Console](/images/beyond_basics_logging_06.png)

### Passing Data to Your Implementation 
Now that you have a basic logger up and running, you'll probably want to begin expanding upon this 
implementation to suit your needs; providing data and information to your logger will help you do that.<br/>
To demonstrate, we'll be giving our basic implementation the ability to set a minimum log level.

Lets start by modifying `MyFirstLoggerFactory`.

<br/>
Add a field variable of type `LogLevel` named `_minLevel` to the top of the class.
```cs
internal class MyFirstLoggerFactory : ILoggerFactory
{
    private LogLevel _minLevel;

    // ...
}
```

Then, create a constructor that takes a parameter of type `LogLevel` named `minLevel`.<br/>
Have the constructor assign the parameter to the `_minLevel` field variable.
```cs
public MyFirstLoggerFactory(LogLevel minLevel)
{
    _minLevel = minLevel;
}
```

<br/>
You'll then want to do the exact same to `MyFirstLogger`.
```cs
public class MyFirstLogger : ILogger
{
    private LogLevel _minLevel;

    public MyFirstLogger(LogLevel minLevel)
    {
        _minLevel = minLevel;
    }
	
	// ...
}
```

While you're editing `MyFirstLogger`, you should also modify the `IsEnabled` method.
Have it return the result of a boolean expression which checks if the `logLevel` parameter is greater than or equal to `_minLevel`.
```cs
public bool IsEnabled(LogLevel logLevel)
{
    return logLevel >= _minLevel;
}
```

<br/>
Next, you'll need to head back to `MyFirstLoggerFactory` and pass the `_minLevel` field variable to the constructor of `MyFirstLogger` in the `CreateLogger` method.
```cs
public ILogger CreateLogger(string categoryName)
{   
    if (_isDisposed) throw new InvalidOperationException("Object disposed."); 
    return new MyFirstLogger(_minLevel);
}
```

Lastly, head over to your `DiscordConfiguration` and pass a `LogLevel` enum to the constructor of `MyFirstLoggerFactory`.
We'll be passing `LogLevel.Information` in this example.
```cs
new DiscordConfiguration()
{
    LoggerFactory = new MyFirstLoggerFactory(LogLevel.Information)
}
```

<br/>
And now that you've made all those adjustments, you should now be able to run your bot and see the results!

![Console](/images/beyond_basics_logging_07.png)


# Log Levels
Below is a table of all log levels and the kind of messages you can expect from each.
Name|Position|Description
:---:|:---:|:---
`Critical`|5|Fatal error which may require a restart.
`Error`|4| A failure of an operation or request.
`Warning`|3|Non-fatal errors and abnormalities.
`Information`|2|Session startup and resume messages.
`Debug`|1| Ratelimit buckets and related information.
`Trace`|0| Websocket & REST traffic.

 >[!WARNING]
 > The `Trace` log level is *not* recommended for use in production.
 >
 > It is intended for debugging DSharpPlus and may display tokens and other sensitive data.