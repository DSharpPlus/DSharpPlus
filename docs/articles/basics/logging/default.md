---
uid: articles.basics.logging.default
title: The Default Logger
---

DSharpPlus ships with a default logging implementation. It is enabled automatically when using `DiscordClientBuilder` for setup, and will serve the needs of your first bot:

![Info Level Logging][0]

This is a basic implementation that only sends log messages to the console. When writing something more serious, you should consider using a proper logging setup that fits your usecase. More information on that is available in the [documentation on third-party loggers](./third_party.md). Furthermore, the default implementation is *only* enabled on `DiscordClientBuilder`, not 

#### Minimum Logging Level

You're able to adjust the verbosity of log messages via `DiscordClientBuilder.SetLogLevel()`. Note that this only works for the default setup, and if you provide your own logger you are expected to configure it accordingly.

```cs
builder.SetLogLevel(LogLevel.Debug)
```

The example above will display level log messages that are higher than or equal to `Debug`.

![Debug Level Logging][1]

#### Log Levels

Below is a table of all log levels and the kind of messages you can expect from each.

Name          | Position | Description
:------------:|:--------:|:------------
`Critical`    | 5        | Fatal error which may require a restart.
`Error`       | 4        | A failure of an operation or request.
`Warning`     | 3        | Non-fatal errors and abnormalities.
`Information` | 2        | General information about library operation.
`Debug`       | 1        | Lifecycle and status information.
`Trace`       | 0        | Low-level websocket & REST traffic.

>[!WARNING]
> The `Trace` log level is *not* recommended for use in production.
>
> It is intended for debugging DSharpPlus and will display all data DSharpPlus sends and receives.

<!-- LINKS -->
[0]: ../../../images/beyond_basics_logging_default_01.png
[1]: ../../../images/beyond_basics_logging_default_02.png
[2]: ../../../images/beyond_basics_logging_default_03.png
[3]: https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings#day-d-format-specifier
