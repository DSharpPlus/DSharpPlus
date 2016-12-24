![Logo](https://github.com/NaamloosDT/DSharpPlus/blob/master/logo/dsharp+_smaller.png?raw=true)

A C# API for Discord based off [DiscordSharp](https://github.com/suicvne/DiscordSharp), but rewritten to fit the API standards.

## Installation
### Nuget
1. Create a new Console Application
2. In the Projects tab, click "Manage NuGet Packages"
3. Click Browse
4. Search for "DSharpPlus" and install the latest version
5. Done!

### Compiling by yourself
1. Download the Git repository
2. Open the project in Visual Studio
3. Build
4. Create a new Console Application
5. Refrence the built DLLs
6. Done!

## Your initial code
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
