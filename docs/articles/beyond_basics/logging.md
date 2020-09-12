---
uid: beyond_basics_logging
title: Logging
---

# Logging
DSharpPlus utilizes .Net Cores Logging facilities by default.  With this you can hook any logger that has been updated to this.  You can also create your own LoggingFactory
to enhance the information that is outputted into a format of your choosing. 

## Hooking up Third Party Loggers
First we are going to have to Add a couple of nuget packages that will handle the logging for us.  Under this example we will be using Serilog (However we will not cover how to configure 
these within the appsettings.json).  The packages we will install are:
1. Serilog
2. Serilog.Settings.Configuration
3. Serilog.Extensions.Logging
4. Serilog.Sinks.Console
5. Serilog.Sinks.File

Now you will create your own LoggingFactory and setup Serilog as shown below:

```cs
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(hostContext.Configuration.GetSection("Logging"))
    .WriteTo.Console()
    .WriteTo.File()
    .CreateLogger();

var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);
```

This will setup serilog to output all the log messages to any sinks that may be configured (in our case the console and the files).  Now within the `DiscordConfiguration`, pass to it
`loggingFactory` as shown below

```cs
var config = new DiscordConfiguration()
{
    MinimumLogLevel = LogLevel.Debug,
    Token = botID.ToString(),
    TokenType = TokenType.Bot,
    LoggerFactory = loggerFactory
};

_client = new DiscordClient(config);
```

Now your console and rolling log file should have all the entries outputted to it from DSharpPlus.  