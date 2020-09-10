# Custom command handlers

If the current command handler does not do what you need it to do and you feel you need to ~~be op~~ write your own, then you can disable the default handler and 
roll your own.  To do this, within the CommandsNextConfiguration you need to specify the UseDefaultCommandHandler property to false (like below).

```cs
_cnext = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                UseDefaultCommandHandler = false
            });
```

From here, you will be able to hook any event your heart desires to in order to ~~reinvent the wheel~~ accomplish your task.