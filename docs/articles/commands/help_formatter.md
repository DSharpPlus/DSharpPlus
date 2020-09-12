---
uid: commands_help_formatter
title: Help Formatter
---

# Custom Help Formatters

If the default help command isnt exactly what you need and you find yourself needing more, then you have two options available to you.  You can 
either wrote your own help command or you can create a custom help formatter which will enhance the output of the default help command.  If you go with
the custom help formatter, you will need to inherit from @DSharpPlus.CommandsNext.Converters.BaseHelpFormatter as shown below:

```cs
using DSharpPlus.CommandsNext.Converters;

namespace MyFirstBot
{
    public class HelpFormatter : BaseHelpFormatter
    {

    }
}
```

From here, you will have to implement all the abstract methods and fill in their respected details.  If you would like to just tweak small things like the embed color 
or title, you will have to instantiate the DefaultHelpFormatter to be used in your overridden methods.  

```cs
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System.Collections.Generic;

namespace MyFirstBot
{
    public class HelpFormatter : BaseHelpFormatter
    {
        readonly DefaultHelpFormatter _d;

        public HelpFormatter(CommandContext ctx)
            : base(ctx)
        {
            this._d = new DefaultHelpFormatter(ctx);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            return this._d.WithCommand(command);
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            return this._d.WithSubcommands(subcommands);
        }

        public override CommandHelpMessage Build()
        {
            var hmsg = this._d.Build();
            var embed = new DiscordEmbedBuilder(hmsg.Embed)
            {
                Color = new DiscordColor(0xD091B2)
            };
            return new CommandHelpMessage(embed: embed);
        }
    }
}
```

Finally, you will have to register your help formatter.  To do this, where you register your commands, you will call @DSharpPlus.CommandsNext.CommandsNextExtension.SetHelpFormatter* .

```cs
_cnext.SetHelpFormatter<MyHelpFormatter>();
```

You can also do alot more advanced things such as manipulate the actual text that is within the embed that is returned.  Please refer to the [Tests](https://github.com/DSharpPlus/DSharpPlus/blob/3d553ac351ccecbedfbc23f485443d6eb968af72/DSharpPlus.Test/TestBotHelpFormatter.cs) to see
how to accomplish this.