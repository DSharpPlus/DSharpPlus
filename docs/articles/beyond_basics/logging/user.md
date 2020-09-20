---
uid: beyond_basics_logging_user
title: Build-a-Logger
---

# Writing a Custom Implementation
If neither the [default logger](xref:beyond_basics_logging_default) nor any of the [third party](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#third-party-logging-providers)
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

![NuGet Package Manager](/images/beyond_basics_logging_user_01.png "Latest stable version")

Then, to keep things organized, create a new folder named `Logging`.<br/>
We'll need two classes within that folder: `MyFirstLogger` and `MyFirstLoggerFactory`.

![Solution Explorer](/images/beyond_basics_logging_user_02.png)

### Basic Logger Implementation
The `MyFirstLogger` class will implement `ILogger`.

![Implement Interface](/images/beyond_basics_logging_user_03.png)

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

![Implement Interface](/images/beyond_basics_logging_user_04.png)

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

![Console](/images/beyond_basics_logging_user_05.png)

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

![Console](/images/beyond_basics_logging_user_06.png)