---
uid: articles.advanced_topics.compression
title: Gateway Compression
---

Discord bots can use a surprising amount of bandwidth receiving events from Discord. To help reduce the impact of this, Discord supports two approaches to lower gateway load: first, intents, which are discussed in <xref:articles.beyond_basics.intents>, and second, compressing the events that end up sent across the gateway.

More specifically, Discord supports two compression methods: zlib and zstd. By default, DSharpPlus will automatically select the "best" compression method currently available. It is possible to forcibly set the used compression method or to disable it entirely; and it is also possible to use zlib for payload-wise compression rather than connection-wise compression. This is generally not advisable.

## Changing the compression method

If the default choice DSharpPlus makes is not satisfactory for you, there are methods provided on both `DiscordClientBuilder` and `IServiceCollection` to change the compression method used. These methods are - symmetrically - called `UseZstdCompression`, `UseZlibCompression` and `DisableGatewayCompression`. Note that they must be called after `AddDiscordClient` if you're using `IServiceCollection`-based setup.

Both the default zlib and zstd compression methods are applied "transport"-wide, meaning that they use one compression context for the entire lifetime of a connection. This is generally advisable as it performs better and results in better compression ratios. It is, however, also alternatively possible to apply zlib compression "payload"-wide, meaning that for each payload a new compression context will be created. This feature is provided only for completeness and can be enabled like so: `serviceCollection.Replace<IPayloadDecompressor, ZlibPayloadDecompressor>`. It is available in `DiscordClientBuilder`-based setup through `DiscordClientBuilder.ConfigureServices`.