# Alternative WebSocket client implementations

For operating systems and runtimes that do not support the native .NET WebSocket implementation, such as Windows 7 or Mono, 
you need to use an alternative WebSocket client implementation.

## Available implementations

Currently, there are 2 implementations available on NuGet:

* `DSharpPlus.WebSocket.WebSocket4Net`: This implementation is recommended if you're targeting .NET Framework 4.5+, and using 
  Windows 7 or Mono 4.4.2.
* `DSharpPlus.WebSocket.WebSocketSharp`: This implementation is recommended if you're targeting .NET Framework 4.5+, and using 
  Mono version higher than 4.4. This implementation will also work on Windows 7, however there are known issues with it and 
  the library.

## Making your own implementation

If none of these fit your criteria, you can make your own implementation, using the existing ones as a template:

* [Source for WS4Net client implementation](https://github.com/NaamloosDT/DSharpPlus/blob/master/DSharpPlus.WebSocket.WebSocket4Net/WebSocket4NetClient.cs "WebSocket4Net Client")
* [Source for WS# client implementation](https://github.com/NaamloosDT/DSharpPlus/blob/master/DSharpPlus.WebSocket.WebSocketSharp/WebSocketSharpClient.cs "WebSocketSharp Client")

## Using alternative WebSocket client implementations

First, you need to install the desired WebSocket client implementation. If you're installing from NuGet, the procedure is the 
same as for all other DSharpPlus packages.

Then you need to indicate that DSharpPlus should use that specific WebSocket implementation. This is done by calling 
[SetWebSocketClient method](/api/DSharpPlus.DiscordClient.html#DSharpPlus_DiscordClient_SetWebSocketClient__1) with appropriate 
generic argument right after you instantiate your Discord client.

For example, for WS4Net client, you need to call it as:

```cs
client.SetWebSocketClient<WebSocket4NetClient>();
```

Similarly, for WS#:

```cs
client.SetWebSocketClient<WebSocketSharpClient>();
```

For any other implementation, make sure it's a class that inherits from [BaseWebSocketClient class](/api/DSharpPlus.Net.WebSocket.BaseWebSocketClient.html "BaseWebSocketClient") 
and has a public parameter-less constructor.

Lastly, make sure you remember to add `using DSharpPlus.Net.WebSocket;` to your usings.