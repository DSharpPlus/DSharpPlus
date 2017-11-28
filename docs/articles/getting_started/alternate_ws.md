# Alternative WebSocket client implementations

For operating systems and runtimes that do not support the native .NET WebSocket implementation, such as Windows 7 or Mono, 
you need to use an alternative WebSocket client implementation.

## Available implementations

Currently, there are 3 implementations available on NuGet:

* `DSharpPlus.WebSocket.WebSocket4Net`: This implementation is recommended if you're targeting .NET Framework 4.5+, and using 
   Windows 7 or Mono 4.4.2. 
* `DSharpPlus.WebSocket.WebSocket4NetCore`: This updated version of WebSocket.WebSocket4Net is recommended if you're targeting
  [.NET Core](https://github.com/dotnet/core) on Windows 7, and solely that platform.
* `DSharpPlus.WebSocket.WebSocketSharp`: This implementation is recommended if you're targeting .NET Framework 4.5+, and using 
  Mono version higher than 4.4. This implementation will also work on Windows 7, however there are known issues with it and 
  the library.

## Making your own implementation

If none of these fit your criteria, you can make your own implementation, using the existing ones as a template:

* [Source for WS4Net client implementation](https://github.com/NaamloosDT/DSharpPlus/blob/master/DSharpPlus.WebSocket.WebSocket4Net/WebSocket4NetClient.cs "WebSocket4Net Client")
* [Source for WS4NetCore client implementation](https://github.com/NaamloosDT/DSharpPlus/blob/master/DSharpPlus.WebSocket.WebSocket4NetCore/WebSocket4NetCoreClient.cs "WebSocket4NetCore Client")
* [Source for WS# client implementation](https://github.com/NaamloosDT/DSharpPlus/blob/master/DSharpPlus.WebSocket.WebSocketSharp/WebSocketSharpClient.cs "WebSocketSharp Client")

## Using alternative WebSocket client implementations

First, you need to install the desired WebSocket client implementation. If you're installing from NuGet, the procedure is the 
same as for all other DSharpPlus packages.

Then you need to indicate that DSharpPlus should use that specific WebSocket implementation. This is done by setting an 
appropriate factory method in @DSharpPlus.DiscordConfiguration.WebSocketClientFactory property in your @DSharpPlus.DiscordConfiguration instance.

The factory methods are static methods, that return an instance of @DSharpPlus.Net.WebSocket.BaseWebSocketClient, and take a `System.Net.IWebProxy` 
as an argument. For provided implementations, they are called `CreateNew`, and are available on the implementation classes:

* WebSocket4Net: @DSharpPlus.Net.WebSocket.WebSocket4NetClient.CreateNew(IWebProxy)
* WebSocket4NetCore: @DSharpPlus.Net.WebSocket.WebSocket4NetCoreClient.CreateNew(IWebProxy)
* WebSocketSharp: @DSharpPlus.Net.WebSocket.WebSocketSharpClient.CreateNew(IWebProxy)

In order to use a specific implementation, you pass selected factory method to the aformentioned `WebSocketClientFactory` property.

For example, for WS4Net client, you need to set it like this:

```cs
var config = new DiscordConfiguration
{
	Token = "my.token.here",
	TokenType = TokenType.Bot,
	// yadda yadda
	WebSocketClientFactory = WebSocket4NetClient.CreateNew
};
```

For WS4NetCore:

```cs
var config = new DiscordConfiguration
{
	Token = "my.token.here",
	TokenType = TokenType.Bot,
	// yadda yadda
	WebSocketClientFactory = WebSocket4NetCoreClient.CreateNew
};
```

Similarly, for WS#:

```cs
var config = new DiscordConfiguration
{
	Token = "my.token.here",
	TokenType = TokenType.Bot,
	// yadda yadda
	WebSocketClientFactory = WebSocketSharpClient.CreateNew
};
```

For any other implementation, make sure you have a class that inherits from @DSharpPlus.Net.WebSocket.BaseWebSocketClient class, 
implements its abstract members, and has a public constructor which takes a `System.Net.IWebProxy` as an argument. Provide a factory 
method which instantiates this implementation, and you're good to go.

Lastly, don't forget to add `using DSharpPlus.Net.WebSocket;` at the top of your `.cs` file.
