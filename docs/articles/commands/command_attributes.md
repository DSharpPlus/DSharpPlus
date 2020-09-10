# Base Attributes

CommandNext has a number of Attributes to help lockdown/enhance your command.  Below are a number of the ones that currently exists:

- @DSharpPlus.CommandsNext.Attributes.AliasesAttribute
- @DSharpPlus.CommandsNext.Attributes.CooldownAttribute
- @DSharpPlus.CommandsNext.Attributes.DescriptionAttribute
- @DSharpPlus.CommandsNext.Attributes.DontInjectAttribute
- @DSharpPlus.CommandsNext.Attributes.HiddenAttribute
- @DSharpPlus.CommandsNext.Attributes.ModuleLifespanAttribute
- @DSharpPlus.CommandsNext.Attributes.PriorityAttribute
- @DSharpPlus.CommandsNext.Attributes.RemainingTextAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireBotPermissionsAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireDirectMessageAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireGuildAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireNsfwAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireOwnerAttribute
- @DSharpPlus.CommandsNext.Attributes.RequirePermissionsAttribute
- @DSharpPlus.CommandsNext.Attributes.RequirePrefixesAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireRolesAttribute
- @DSharpPlus.CommandsNext.Attributes.RequireUserPermissionsAttribute

# Custom Attributes

There are some cases where you will need create your own Attribute to enhance your commands.  To do this, your new attribute will need to inherit from CheckBaseAttribute
and then you can fill in the details.

```cs
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace MyFirstBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class InternalPermissionAttribute : CheckBaseAttribute
    {
        readonly string Permission;
        public InternalPermissionAttribute(string permission)
        {
            this.Permission = permission;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            ////perform check in here 
        }
    }
}
```

Once as the details are filled out, you can then add the attribute to your command.

```cs
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyFirstBot.Attributes;
using System.Threading.Tasks;

namespace MyFirstBot
{
    public class MyCommand : BaseCommandModule
    {
        [Command("hi"), InternalPermission("noob")]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync($"👋 Hi noob, {ctx.User.Mention}!");
        }
    }
}
```