---
uid: commands_module_lifespans
title: Commands Introduction
---

# Command module lifespans - preventing module reuse

There are situations where you don't want to reuse a module instance, for one reason or another, usually due to 
thread-safety concerns.

CommandsNext allows command modules 2 have to lifespan modes: singleton (default), and transient. Singleton modules, as 
the name implies, are instantiated once per the entire CNext extension's lifetime. Transient modules, on the other 
hand, are instantiated for each command call. This enables you to make use of transient and scoped modules. If you're 
unsure what that means, familiarize yourself with the [dependency injection](xref:commands_dependency_injection "dependency injection") 
guide.

## 1. Default Implementation (Singleton)

This is the default implementation where the ModuleLifespan Attribute does not need to be specified but can for completeness sake

```cs
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace MyFirstBot
{
    [ModuleLifespan(ModuleLifespan.Singleton)]
    public class MyCommand : BaseCommandModule
    {
        [Command("hi")]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!");
        }
    }
}
```

## 2. Transient Implementation

This is the implementation where the ModuleLifespan is set to be Transient
```cs
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace MyFirstBot
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MyCommand : BaseCommandModule
    {
        [Command("hi")]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!");
        }
    }
}
```
