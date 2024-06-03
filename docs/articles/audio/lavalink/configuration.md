---
uid: articles.audio.lavalink.configuration
title: Lavalink Configuration
---
---
uid: articles.audio.lavalink.configuration
title: Lavalink Configuration
---

>[!WARNING]
> `DSharpPlus.Lavalink` has been deprecated, and this article may contain outdated information. Both the extension and this article will be removed
> in the future.

# Setting up DSharpPlus.Lavalink

## Configuring Your Client

To begin using DSharpPlus's Lavalink client, you will need to add the `DSharpPlus.Lavalink` nuget package. Once
installed, simply add these namespaces at the top of your bot file:

```csharp
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
```

After that, we will need to create a configuration for our extension to use. This is where the special values from the
server configuration are used.

```csharp
var endpoint = new ConnectionEndpoint
{
    Hostname = "127.0.0.1", // From your server configuration.
    Port = 2333 // From your server configuration
};

var lavalinkConfig = new LavalinkConfiguration
{
    Password = "youshallnotpass", // From your server configuration.
    RestEndpoint = endpoint,
    SocketEndpoint = endpoint
};
```

Finally, initialize the extension.

```csharp
var lavalink = Discord.UseLavalink();
```

## Connecting with Lavalink

We are now ready to connect to the server. Call the Lavalink extension's connect method and pass the configuration. Make
sure to call this **after** your Discord client connects. This can be called either directly after your client's connect
method or in your client's ready event.

```csharp
LavalinkNode = await Lavalink.ConnectAsync(lavalinkConfig);
```

Your main bot file should now look like this:

```csharp
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;

namespace MyFirstMusicBot
{
    class Program
    {
        public static DiscordClient Discord;

        public static async Task Main(string[] args)
        {
            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "<token_here>",
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug
            });

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1", // From your server configuration.
                Port = 2333 // From your server configuration
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass", // From your server configuration.
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Discord.UseLavalink();

            await Discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig); // Make sure this is after Discord.ConnectAsync().

            await Task.Delay(-1);
        }
    }
}
```

We are now ready to start the bot. If everything is configured properly, you should see a Lavalink connection appear in
your DSharpPlus console:

```
[2020-10-10 17:56:07 -04:00] [403 /LavalinkConn] [Debug] Connection to Lavalink node established
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
