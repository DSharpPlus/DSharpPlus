---
uid: articles.commands.custom_error_handler
title: Custom Error Handler
---

# Custom Error Handler
Oh no! Someone tried to execute a command, but for whatever reason, it failed! This article will help you understand why commands might fail and how to handle them.

There are a few reasons why a command might not execute. A few common ones include:
- **Context Checks**: The command might have checks that are not met. For example, the command might require the user to have a certain role, or the command might require the bot to have certain permissions.
- **Argument Parsing**: The command might have arguments that are not valid. For example, the command might require a number, but the user provided a string.
- **Command Execution**: The command might have an unexpected error while executing. For example, the command might try to send a message to a channel, but the bot does not have permission to send messages in that channel, causing an exception to be thrown.

When any part of the command process fails, the command processor will raise the `CommandErrored` event. This event is executed with a `CommandErrorEventArgs` object, which contains information about the command that errored, the command context provided, the command object itself (when possible), and the exception that caused the error. By default, we have a built in error handler which provides a helpful debug embed to the user, but you can override this behavior by setting `CommandsConfiguration.UseDefaultCommandErrorHandler` to `false` and registering your own delegate to the `CommandErrored` event. Here's an example of how you might handle the `CommandErrored` event:

```cs
CommandsExtension commandsExtension = discordClient.UseCommands(new CommandsConfiguration
{
    // Disable the default error handler
    UseDefaultCommandErrorHandler = false,
});

// Add our own error handler
commandsExtension.CommandErrored += async (s, e) =>
{
    StringBuilder stringBuilder = new();
    stringBuilder.Append("An error occurred while executing the command: ");
    stringBuilder.Append(e.Exception.GetType().Name);
    stringBuilder.Append(", ");
    stringBuilder.Append(Formatter.InlineCode(Formatter.Sanitize(e.Exception.Message)));

    await eventArgs.Context.RespondAsync(stringBuilder);
};
```

Our default error handler will handle a large variety of common cases and will provide a helpful debug embed (which includes which exception, the exception message and the stack trace) to the user. However, if you want to provide a more custom error handling experience, you can handle the `CommandErrored` event and provide your own error handling logic.