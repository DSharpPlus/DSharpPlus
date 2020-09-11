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

From here, you will be able to hook any event your heart desires to in order to ~~reinvent the wheel~~ accomplish your task.  To do this, you may want to look at the
current [CommandNext handler](https://github.com/DSharpPlus/DSharpPlus/blob/3d553ac351ccecbedfbc23f485443d6eb968af72/DSharpPlus.CommandsNext/CommandsNextExtension.cs#L179)
to get you started.  This will show you how to parse the prefix such as:
```cs
var mpos = -1;
if (this.Config.EnableMentionPrefix)
    mpos = e.Message.GetMentionPrefixLength(this.Client.CurrentUser);

if (this.Config.StringPrefixes?.Any() == true)
    foreach (var pfix in this.Config.StringPrefixes)
        if (mpos == -1 && !string.IsNullOrWhiteSpace(pfix))
            mpos = e.Message.GetStringPrefixLength(pfix, this.Config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

if (mpos == -1 && this.Config.PrefixResolver != null)
    mpos = await this.Config.PrefixResolver(e.Message).ConfigureAwait(false);

if (mpos == -1)
    return;
```
Then you can locate the command and then create the CommandContext such as 

```cs
var cmd = this.FindCommand(cnt, out var args);
var ctx = this.CreateContext(e.Message, pfx, cmd, args);
```
 Once as your handler is created, you can then hook it to any of the events that fits your needs: 

 ```cs 
 this.Client.MessageCreated += this.HandleCommandsAsync;
 ```

 With the above said though, make sure that whatever you do in the event is non-blocking and if it is long lived, you run it in a seperate Thread/Task:

 ```cs 
 _ = Task.Run(async () => await this.ExecuteCommandAsync(ctx));
 ```