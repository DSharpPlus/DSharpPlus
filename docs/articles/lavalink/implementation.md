# Setting up DSharpPlus.Lavalink

The remaining articles assume that you know how to use CommandsNext. If you have not, you should learn [here](https://dsharpplus.github.io/articles/commands/intro.html) before continuing with Lavalink.

## Configuring Your Client

To begin using DSharpPlus's Lavalink client, you will need to add the `DSharpPlus.Lavalink` nuget package. Once installed, simply add these namespaces at the top of your bot file:
```csharp
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
```
Next, make sure you have an extension property in your class:
```csharp
public static LavalinkExtension Lavalink;
```

After that, we will need to create a configuration for our extension to use. This is where the special values from the server configuration are used.
```csharp
var restConfig = new ConnectionEndpoint 
{
    Hostname = "localhost", //From your server configuration.
    Port = 8080 //From your server configuration
};

var socketConfig = new ConnectionEndpoint
{
    Hostname = "localhost", //From your server configuration.
    Port = 8080 //From your server configuration
};

var lcfg = new LavalinkConfiguration
{
    Password = "youshallnotpass", //From your server configuration.
    RestEndpoint = restConfig,
    SocketEndpoint = socketConfig
};
```
Finally, initialize the extension.
```csharp
Lavalink = discord.UseLavalink(lcfg);
```

## Connecting with Lavalink

We are now ready to connect to the server. Call the Lavalink extension's connect method and pass the configuration. Make sure to call this **after** your Discord client connects:

```csharp
await Lavalink.ConnectAsync(lcfg);
```

Your main bot file should now look like this: 

```csharp
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;

namespace MyFirstMusicBot
{
    class Program
    {
        public static DiscordClient Discord;
        public static LavalinkExtension Lavalink;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "<token_here>",
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            var restConfig = new ConnectionEndpoint
            {
                Hostname = "localhost", //From your server configuration.
                Port = 8080 //From your server configuration
            };

            var socketConfig = new ConnectionEndpoint
            {
                Hostname = "localhost", //From your server configuration.
                Port = 8080 //From your server configuration
            };

            var lcfg = new LavalinkConfiguration
            {
                Password = "youshallnotpass", //From your server configuration.
                RestEndpoint = restConfig,
                SocketEndpoint = socketConfig
            };

            Lavalink = Discord.UseLavalink();

            await Discord.ConnectAsync();
            await Lavalink.ConnectAsync(lcfg); //Make sure this is after Discord.ConnectAsync().
            await Task.Delay(-1);
        }
    }
}
```
If everything is configured properly, you should see a Lavalink connection appear in your DSharpPlus console:

```
[WebSocket] [Debug] Received OP 10 (HELLO) - Trying to either resume or identify.
[WebSocket] [Debug] Starting Heartbeat.
[WebSocket] [Debug] Sending Heartbeat.
[WebSocket] [Debug] Received WebSocket Heartbeat Ack. Ping: 58ms
[Lavalink] [Info] Connection established
```

And a client connection appear in your Lavalink console: 

```
INFO 5180 --- [  XNIO-1 task-1] io.undertow.servlet                      : Initializing Spring DispatcherServlet 'dispatcherServlet'
INFO 5180 --- [  XNIO-1 task-1] o.s.web.servlet.DispatcherServlet        : Initializing Servlet 'dispatcherServlet'
INFO 5180 --- [  XNIO-1 task-1] o.s.web.servlet.DispatcherServlet        : Completed initialization in 8 ms
INFO 5180 --- [  XNIO-1 task-1] l.server.io.HandshakeInterceptorImpl     : Incoming connection from /0:0:0:0:0:0:0:1:58238
INFO 5180 --- [  XNIO-1 task-1] lavalink.server.io.SocketServer          : Connection successfully established from /0:0:0:0:0:0:0:1:58238
```

We are now ready to set up some music commands!
