# Adding Music Commands

This article assumes that you know how to use CommandsNext. If you do not, you should learn [here](https://dsharpplus.github.io/articles/commands/intro.html) before continuing with this guide.

## Prerequisites 

Before we start we will need to make sure CommandsNext is configured. For this we can make a simple configuration and command class: 

```csharp
using DSharpPlus.CommandsNext;

namespace MyFirstMusicBot
{
    public class MyLavalinkCommands : BaseCommandModule
    {

    }
}
```
And be sure to register it in your program file: 
```csharp
CommandsNext = Discord.UseCommandsNext(new CommandsNextConfiguration
{
    StringPrefixes = new string[] { ";;" }
});

CommandsNext.RegisterCommands<MyLavalinkCommands>();
