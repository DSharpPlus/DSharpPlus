Your initial code
==================
There are 2 ways you can do this, first is the basic way:
```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Voice;

namespace DSharpPlusBot
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordClient _client = new DiscordClient();

            _client.Connect(File.ReadAllText("token.txt"), TokenType.Bot);

            Console.ReadLine();
        }
    }
}
```
This method contains no configuration for your bot, and is not the version that we will be using for the rest of the tutorials. We will instead be using this version:
```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Voice;

namespace DSharpPlusBot
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordClient _client = new DiscordClient(new DiscordConfig()
            {
                Token = File.ReadAllText("token.txt"),
                AutoReconnect = true
            });

            _client.Connect();
            Console.ReadLine();
        }
    }
}
```
## Where to from here
From here you can go to the next chapter, "Your First Message". Use the code from above and continue on.