---
uid: commands_custom_handler
title: Custom Handlers
---

# Custom command handlers

If the built in command handler doesn't satisfy your needs and you feel the need to ~~be op~~ write your own, then you can disable the default handler and 
roll your own.  To do this, within the CommandsNextConfiguration you need to specify the UseDefaultCommandHandler property to false (like below).

```cs
_cnext = _client.UseCommandsNext(new CommandsNextConfiguration()
{
    UseDefaultCommandHandler = false
});
```

From here, you will be able to hook any event your heart desires to in order to ~~reinvent the wheel~~ accomplish your task.