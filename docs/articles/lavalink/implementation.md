# Configuring Your Client

To begin using DSharpPlus's Lavalink client, you will need to add the `DSharpPlus.Lavalink` nuget package. Once installed, simply add the namespace at the top of your bot file:
```csharp
using DSharpPlus.Lavalink;
```
Next, make sure you have an extension property in your class:
```csharp
public LavalinkExtension Lavalink;
```
After that, we will need to create a configuration for our extension to use. This is where the special values from the server configuration are used.
```csharp
var restConfig = new ConnectionEndpoint 
{
    HostName = "localhost", //From your server configuration.
    Port = 8080 //From your server configuration
};

var socketConfig = new ConnectionEndpoint
{
    HostName = "localhost", //From your server configuration.
    Port = 8080 //From your server configuration
};

var lcfg = new LavalinkConfiguration
{
    Password = "youshallnotpass", //From your server configuration.
    RestEndpoint = restConfig,
    SocketEndpoint = socketConfig
};
```
And finally, initialize the extension with the configuration, and connect the client:
```csharp
Lavalink = discord.UseLavalink(lcfg);
```
