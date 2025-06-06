---
uid: articles.advanced_topics.compression
title: Gateway Compression
---

Discord bots can use a surprising amount of bandwidth receiving events from Discord. To help reduce the impact of this, Discord supports two approaches to lower gateway load: first, intents, which are discussed in <xref:articles.beyond_basics.intents>, and second, compressing the events that end up sent across the gateway.

More specifically, Discord supports two compression methods: zlib and zstd. By default, DSharpPlus will automatically select the "best" compression method currently available. It is possible to forcibly set the used compression method or to disable it entirely; and it is also possible to use zlib for payload-wise compression rather than connection-wise compression. This is generally not advisable.

## A note on better-ness

The arguments on which compression method is best could fill entire books, and often the answer is very situational, but in the circumstances at play in DSharpPlus there generally is a very clear answer. Zstd is implemented very efficiently and without unnecessary memory allocations and copies, but it is not by default available on all systems in .NET. While DSharpPlus does provide a package to make it available - `DSharpPlus.Natives.Zstd`, the package is quite large and doesn't support 32-bit systems, mobile or RISC-V-based systems (it supports windows, linux and osx running on x86_64 and arm64). On the other hand, the zlib-based implementation is considerably less efficient due to constraints placed by the standard library design, but does not come with any compatibility caveats. Since their effectiveness is very similar, DSharpPlus by default prefers zstd if it is available, falling back to zlib otherwise.

## Changing the compression method

If the default choice DSharpPlus makes is not satisfactory for you, there are methods provided on both `DiscordClientBuilder` and `IServiceCollection` to change the compression method used. These methods are - symmetrically - called `UseZstdCompression`, `UseZlibCompression` and `DisableGatewayCompression`. Note that they must be called after `AddDiscordClient` if you're using `IServiceCollection`-based setup.

Both the default zlib and zstd compression methods are applied "transport"-wide, meaning that they use one compression context for the entire lifetime of a connection. This is generally advisable as it performs better and results in better compression ratios. It is, however, also alternatively possible to apply zlib compression "payload"-wide, meaning that for each payload a new compression context will be created. This feature is provided only for completeness and can be enabled like so: `serviceCollection.Replace<IPayloadDecompressor, ZlibPayloadDecompressor>`. It is available in `DiscordClientBuilder`-based setup through `DiscordClientBuilder.ConfigureServices`.

## Installing Zstd

Most Linux systems will already have zstd installed one way or another, but Windows systems generally do not. To make zstd easily available regardless, install `DSharpPlus.Natives.Zstd` into your project. `dotnet publish -c Release --use-current-runtime` will pick the correct native library file based on the target architecture and place it so that DSharpPlus can detect zstd and enable it by default.

When using `dotnet build`, dotnet makes no assumptions about the target architecture and instead places all native library files in a subdirectory of the build directory. DSharpPlus will still make a best-effort attempt to find zstd in the predicted output from `dotnet build`, but DSharpPlus may not succeed automatically. You may use `UseZstdCompression` to enforce it, but you should weigh the advantages of zstd compression against the potential disadvantage of having to provide your own zstd build on unsupported targets.