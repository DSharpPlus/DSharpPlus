# Building DSharpPlus

DSharpPlus, for the most part, is just like any other `dotnet` library. You will need the latest .NET SDK installed, either explicitly or bundled by the latest version of your IDE, and you will need to simply run `dotnet build`, `dotnet publish` or `dotnet pack` depending on whether you aim to create a dev-time build, prod builds or local package files.

DSharpPlus' voice implementation contains rust code binding to [OpenMLS](https://github.com/openmls/openmls). DSharpPlus only intends to support 64-bit targets (since we need to contend with exporting and interop in DSharpPlus) that are also supported *and tested* by OpenMLS. At this time, this means DSharpPlus.Voice only supports win-x64, linux-x64 and osx-x64. Contact us if you find this list to be outdated.

Since this code is relevant to operation security, DSharpPlus strongly discourages running DSharpPlus.Voice on untested and/or unsupported platforms. If this is absolutely necessary, you may build DSharpPlus.Voice like so:

1. Make sure you have the latest Rust toolchain and the latest .NET SDK installed.
2. Build libaerith by running `cargo build -r` in the `./lib/libaerith` folder, relative to this file.
3. Create a folder for your [Runtime Identifier](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog) in the following path: `./natives/libaerith/$rid/native`, relative to this file, where `$rid` must be replaced by the runtime identifier you wish to build DSharpPlus for.
4. Copy the dynamic library file from `./lib/libaerith/target/release` into your new folder. On Windows, the file will be called `aerith.dll`, on Linux, `libaerith.so`, on MacOS, `libaerith.dylib`.
5. Build DSharpPlus as normal, explicitly specifying your runtime identifier using `-r|--runtime`. You will see a warning in your CLI about an unsupported target.