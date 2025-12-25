# Building DSharpPlus

DSharpPlus, for the most part, is just like any other `dotnet` library. You will need the latest .NET SDK installed, either explicitly or bundled by the latest version of your IDE, and you will need to simply run `dotnet build`, `dotnet publish` or `dotnet pack` depending on whether you aim to create a dev-time build, prod builds or local package files.

Optionally, DSharpPlus can use zstd for its gateway compression and sodium for receiving [interactions](https://discord.com/developers/docs/interactions/overview#preparing-for-interactions) or [other events](https://discord.com/developers/docs/events/webhook-events). Both of those libraries are provided for platforms `{win, osx, linux, linux-musl}-{x64, arm64}` via the NuGet packages `DSharpPlus.Natives.Zstd` and `DSharpPlus.Natives.Sodium`. On other operating systems or other CPU architectures, you have to provide the libraries yourself.

---

## DSharpPlus.Voice

DSharpPlus' voice implementation is a bit more finicky. On platforms `{win, osx, linux, linux-musl}-{x64, arm64}`, the necessary native libraries are provided for you by DSharpPlus. On other operating systems or other CPU architectures, including but not limited to 32-bit platforms, mobile operating systems or RISC-V, you have to build the libraries yourself:

- [Sodium](https://github.com/jedisct1/libsodium) is unmodified and may be provided by whatever mechanism your platform provides. You may receive a warning on startup that AES-256 GCM is not supported on certain platforms or CPUs, but DSharpPlus.Voice will function regardless - albeit with limitations.
- [SpeexDSP](https://github.com/xiph/speexdsp) is unmodified and may be provided by whatever mechanism your platform provides. 
- [Opus](https://github.com/xiph/opus) is modified and must be self-built from https://github.com/dsharpplus/opus. Please follow the opus build instructions for your platform and place the shared library file (.dll, .so, .dylib) in the same directory as your bot.
- [Koana](https://github.com/dsharpplus/libkoana) is a heavily stripped-down and slightly modified fork of [DPP's E2EE implementation](https://github.com/brainboxdotcc/dpp) and must be self-built.  
   - First, you must provide OpenSSL **3.x** on your system, either building it or installing the **development** package. Merely installing OpenSSL libraries is insufficient. Before continuing, make sure you have the header files and **static** OpenSSL libraries on your system.
   - Second, build koana itself, using your platform's mechanism to create a release build using cmake. Koana will attempt to detect OpenSSL in common locations, but you may have to provide it using the following arguments to cmake: `-DOPENSSL_ROOT_DIR=path`, `-DOPENSSL_CRYPTO_LIBRARY=path/to/libcrypto`, `-DOPENSSL_SSL_LIBRARY=path/to/libssl`, `-DOPENSSL_INCLUDE_DIR=path/to/headers`. Note that you must provide all four of them.
   - Third, place the shared library file (.dll, .so, .dylib) in the same directory as your bot.
   - Fourth, if you built OpenSSL yourself, you may have to provide libcrypto to your bot as well, in the same way.

We may not accept bug reports only reproducible on unsupported platforms, and we may not be able to help with building for unsupported platforms. General experience with using cmake is highly recommended.

>[!NOTE]
> Our native builds for x86-64 target `x86-64-v2`, which is any CPU that supports SSE4.2, LAHF-SAHF, POPCNT and CMPXCHG16B at the very least, or any CPU built after 2008. If you are using an older CPU that does not support these features, you will also need to self-build the natives according to the above instructions. Additionally, you must remove the native libraries provided by DSharpPlus, since they will be incompatible with your CPU.
