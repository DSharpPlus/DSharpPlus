---
uid: articles.advanced_topics.http_interactions_events
title: HTTP Interactions and Webhook Events
---

Discord offers bots to receive interactions and a [small set of gateway events](https://docs.discord.com/developers/events/webhook-events) over HTTP rather than over the traditional websocket-based gateway. DSharpPlus provides a default implementation of this functionality in the `DSharpPlus.Http.AspNetCore` package, and it is possible to create compatibility layers for your HTTP server of choice.

> [!NOTE]
> Lobby Messages and Game Direct Messages are intended for the Discord social SDK and are therefore not currently implemented in DSharpPlus.

> [!NOTE]
> The following documentation does not provide examples for use with `DiscordClientBuilder`, since it is assumed you will have a service collection for setup with ASP.NET Core. If you do have a `DiscordClientBuilder`, all code is callable via the following:
> ~~~cs
> DiscordClientBuilder.ConfigureServices(services => /* here */);
> ~~~

The first thing you will usually want to do in conjunction with HTTP interactions is to disable the traditional gateway:

~~~cs
services.DisableGateway();
~~~

The `DSharpPlus.Http.AspNetCore` package then provides the following two extensions on `RouteHandlerBuilder`:

~~~cs
routeHandlerBuilder.AddDiscordHttpInteractions(string url = "/interactions");
routeHandlerBuilder.AddDiscordWebhookEvents(string url = "/webhook-events");
~~~

You will then receive interactions and/or webhook events as per usual.

## Implementing HTTP interactions and webhook events for your own HTTP server

DSharpPlus provides all necessary utilities to implement support for your favourite HTTP server:

You need the verification key for the current application (obtainable via `DiscordClient.CurrentApplication.VerifyKey`) and `IInteractionTransportService` or `IWebhookTransportService` respectively.

1. Upon receiving a payload, validate its integrity and decrypt it using `DiscordHeaders.VerifySignature`.
2. Pass the decrypted payload to the respective transport service.
3. If using HTTP interactions (not webhook events!), return the payload received from `IInteractionTransportService.HandleInteractionAsync` and the appropriate status code. This contains the initial response to the interaction.

`DiscordHeaders.VerifySignature` uses native methods imported from `libsodium`. You must either provide it yourself or install `DSharpPlus.Natives.Sodium`. If you are using `DSharpPlus.HttpInteractions.AspNetCore`, libsodium is already implicitly provided for you.
