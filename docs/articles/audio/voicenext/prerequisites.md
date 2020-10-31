---
uid: voicenext_prerequisites
title: VoiceNext Prerequisites
---

## Required Libraries
VoiceNext depends on the [libsodium](https://github.com/jedisct1/libsodium) and [Opus](https://opus-codec.org/) libraries to decrypt and process audio packets.<br/>
Both *must* be available on your development and host machines otherwise VoiceNext will *not* work.


### Windows
When installing VoiceNext though NuGet, an additional package containing the native Windows binaries  will automatically be included with **no additional steps required**.

However, if you are using DSharpPlus from source or without a NuGet package manager, you must manually [download](xref:natives) the binaries and place them at the root of your working directory where your application is located.

### MacOS
Native libraries for Apple's macOS can be installed using the [Homebrew](https://brew.sh) package manager:
```console 
$ brew install opus libsodium
```

### Linux


#### Debian and Derivatives 
Opus package naming is consistent across Debian, Ubuntu, and Linux Mint.
```terminal 
$ sudo apt install libopus0 libopus-dev
```

Package naming for *libsodium* will vary depending on your distro and version:

Distributions|Terminal Command
:---:|:---:
Ubuntu 20.04, Ubuntu 18.04, Debian 10|`sudo apt install libsodium23 libsodium-dev`
Linux Mint, Ubuntu 16.04, Debian 9 |`sudo apt install libsodium18 libsodium-dev`
Debian 8|`sudo apt-get install libsodium13 libsodium-dev`