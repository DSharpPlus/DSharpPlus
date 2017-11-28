# I'm broke and all I have is a Pi. How do I host my bot?

Hosting a small bot doesn't require an almighty supercomputer. If performance is not a big concern for your bot, you 
might want to consider buying a Raspberry Pi (or using one you already have). They are $35 ARM-based computers, which 
should sport more than enough power to host a bot that doesn't interact with too many servers or people.

Raspberry Pi comes in 4 versions:

* **Raspberry Pi 1** comes in 4 variants: A, A+, B, and B+. They all feature a single-core 700MHz ARMv6z CPU, and 512MB 
  RAM (with the exception of A, which has only 256MB). The A models have a single USB 2.0 port and no Ethernet, the B model 
  has 2 USB 2.0 ports, and 10/100MBit Ethernet, B+ features 10/100 Ethernet and 4 USB 2.0 ports.
* **Raspberry Pi 2** has a single variant: model B. It comes with a quad-core 900MHz ARMv7-A CPU, 1GB RAM, 10/100 Ethernet 
  port, and 4 USB 2.0 ports.
* **Raspberry Pi 3**, like the 2, only has model B. Features quad-core 1.2GHz ARMv8-A 64-bit CPU (although the default OS is 
  32-bit), 1GB RAM, 10/100 Ethernet, 802.11n Wi-Fi, Bluetooth, and 4 USB 2.0 ports.
* **Raspberry Pi Zero** is smaller than the other options, but at the cost of having a single-core 1GHz ARMv6Z CPU, 512MB 
  RAM, and a single Micro-USB 2.0 port. Later revisions also have 802.11n Wi-Fi and Bluetooth.

There are also several clones of Raspberry Pi, some of which pack more computing power. There are 4 ways to run a bot on your 
Pi. Depending on the board you have, your options might be limited.

## Method 1: Use a prebuilt .NET Core docker image

This method requires an ARMv7- or ARMv8-based board, with 32-bit OS.

This method is probably the most recommended, as it already packages the .NET Core runtime into a Docker container with all 
the necessary utilities required to host your bot. On top of that, it also takes care of properly isolating your bot from 
the OS.

1.  Login to your Pi via SSH or serial connection.
2.  Execute `curl -sSL https://get.docker.com | sudo sh`. This will install Docker and all the required dependencies.
3.  Add your user to `docker` group (`sudo usermod -aG docker $USER`). This is optional, but if you don't do that, you will 
    need to execute all Docker commands with `sudo`. After you do this, you will need to disconnect (or logout) and connect 
    again.
4.  Go to `/tmp` (`cd /tmp`).
5.  Download the [prebuilt docker image](/rpi/armhf-netcore2.0.tar.xz ".NET Core 2.0 ARM image") 
    (`curl -LO https://dsharpplus.emzi0767.com/rpi/armhf-netcore2.0.tar.xz`).
6.  Extract the .xz file (`xz -dvv armhf-netcore2.0.tar.xz`).
7.  Load the image (`cat armhf-netcore2.0.tar | docker load`).
8.  Remove the temporary file (`rm armhf-netcore2.0.tar`).
9.  Start a new container using the image (`docker run -dti --name=mybot armhf/netcore2.0`).
10. Attach to the container (`docker attach mybot`).

You will be dropped into a shell for the default user `dotnet`. The password is `netdot`, and `sudo` access is enabled. It is 
advised you change the password by doing `passwd`. Once you do that, detach from the container (Ctrl+P, Ctrl+Q).

You need to make your bot target .NET Core 2.0 (`netcoreapp2.0`), and publish it. To build and publish, you do the following (using 
dotnet CLI):

1. Clean your previous build (`dotnet clean -c Release`).
2. Restore packages (`dotnet restore`).
3. Build your project in Release configuration for .NET Core 2.0 (`dotnet build -c Release -f netcoreapp2.0`).
4. Publish your project (`dotnet publish -c Release -f netcoreapp2.0`).

Your build artifacts will be placed in `bin/Release/netcoreapp2.0/publish`. Package them and transfer them to your Pi. I recommend 
packing as a `.tar` archive using a program like 7-Zip. Once on the Pi, you will need to copy your bot data to your Docker 
container:

1. Navigate to where you uploaded the archive.
2. Copy the archive to the container (`docker cp mybot.tar mybot:/home/dotnet/tmp/mybot.tar`). You need to replace `mybot.tar` with 
   the actual archive name.
3. Reattach to the container (`docker attach mybot`).
4. Inside, navigate to `~/tmp` (`cd ~/tmp`).
5. Extract and delete the archive (`tar xf mybot.tar && rm mybot.tar`).
6. Create a directory for your bot inside `~/apps` (`mkdir ~/apps/mybot`).
7. Copy the bot files to the directory and delete the temporary files (`cp -rf * ~/apps/mybot && rm -rf *`).
8. Navigate to the bot's directory (`cd ~/apps/mybot`).
9. Start your bot `dotnet MyBot.dll`.

In the above, replace `mybot` with the name of your bot (without spaces), and `MyBot.dll` with your bot's entry DLL name. You 
can now detach, your bot is running. Should you ever need to update your bot, just reattach to the container, stop the bot using 
Ctrl+C, then repeat the above steps.

## Method 2: Install .NET Core 2.0 runtime manually

This method has the same requirements as the first method (ARMv7 or ARMv8 CPU, with 32-bit OS).

This method will install a shared .NET Core 2.0 runtime on your device. This is particularly useful if you intend to run more 
than one .NET Core application on the device. To install the runtime, do the following:

1. Login to your Pi via SSH or serial connection.
2. Install necessary prerequisites (`sudo apt-get install curl libunwind8 gettext`).
3. Go to `/tmp` (`cd /tmp`).
4. Download [.NET Core 2.0 ARM runtime](https://dotnetcli.blob.core.windows.net/dotnet/Runtime/release/2.0.0/dotnet-runtime-latest-linux-arm.tar.gz ".NET Core 2.0 ARM runtime") 
   (`curl -LO https://dotnetcli.blob.core.windows.net/dotnet/Runtime/release/2.0.0/dotnet-runtime-latest-linux-arm.tar.gz`).
5. Create a directory called `dotnet` in `/opt` (`sudo mkdir /opt/dotnet`).
6. Extract the runtime to the directory (`sudo tar xzf dotnet-runtime-latest-linux-arm.tar.gz -C /opt/dotnet`).
7. Create a softlink for the `dotnet` binary in `/usr/local/bin` (`sudo ln -s /opt/dotnet/dotnet /usr/local/bin/dotnet`).
8. Clean up (`rm dotnet-runtime-latest-linux-arm.tar.gz`).
9. Verify that the installation was successful (`dotnet --info`).

If you were successful, you should see something to this effect:

```
pi@raspberry:/tmp $ dotnet --info

Microsoft .NET Core Shared Framework Host

  Version  : 2.0.0
  Build    : e8b8861ac7faf042c87a5c2f9f2d04c98b69f28d
```

You need to make your bot target .NET Core 2.0 (`netcoreapp2.0`), and publish it. To build and publish, you do the following (using 
dotnet CLI):

1. Clean your previous build (`dotnet clean -c Release`).
2. Restore packages (`dotnet restore`).
3. Build your project in Release configuration for .NET Core 2.0 (`dotnet build -c Release -f netcoreapp2.0`).
4. Publish your project (`dotnet publish -c Release -f netcoreapp2.0`).

Your build artifacts will be placed in `bin/Release/netcoreapp2.0/publish`. Package them and transfer them to your Pi. From there, 
unpack, and run by doing `dotnet Project.Name.dll`. For example, if your project is named MyBot, then the command will be 
`dotnet MyBot.dll`.

### What if the version is not `2.0.0`?

If the reported .NET Core runtime version is different from `2.0.0`, but for example `2.0.1`, you will need to tweak your csproj file 
and build environment a bit.

1. Add the following feed to your NuGet sources: `https://dotnet.myget.org/F/dotnet-core/api/v3/index.json`. This is the .NET Core 
   MyGet feed.
2. Open your .csproj file, and inside the root (`<Project>`) element, create the following element:

   ```xml
   <PropertyGroup>
     <RuntimeFrameworkVersion>2.0.1</RuntimeFrameworkVersion>
   </PropertyGroup>
   ```
   
   Of course, replace `2.0.1` with the actual version reported by `dotnet --info`.

## Method 3: Package your bot as self-contained app

This method has the same requirements as the first method (ARMv7 or ARMv8 CPU, with 32-bit OS).

This method will cause your application to package a copy of the runtime with your application. This approach is good for situations 
where you don't want to (or can't) install the runtime on the target or don't intend to run multiple .NET Core applications. It's not 
suitable for multiple application scenarios, however, as this grows applications size by a large margin.

You need to make your bot target .NET Core 2.0 (`netcoreapp2.0`), and publish it for ARM Linux runtime. To build and publish, you do the 
following (using dotnet CLI):

1. Clean your previous build (`dotnet clean -c Release`).
2. Restore packages (`dotnet restore`).
3. Build your project in Release configuration for .NET Core 2.0 (`dotnet build -c Release -f netcoreapp2.0`).
4. Publish your project (`dotnet publish -c Release -f netcoreapp2.0 -r linux-arm`).

Your build artifacts will be placed in `bin/Release/netcoreapp2.0/linux-arm/publish`. Package them and transfer them to your Pi. Before 
you can run it, you will need to make a binary called `Your.Project` executable. For example, if your project is named MyBot, you will need 
to `chmod +x MyBot`.

After all is done, you can run your bot by doing `./Your.Project` from the directory it's in. Of course, replace `Your.Project` with the 
actual binary name.

## Method 4: Run your bot using Mono

This method can be utilized for all board flavours (ARMv6-, ARMv7, and ARMv8-based), no matter the OS bitness (32- and 64-bit *should* work).

Since Mono neither is .NET Core, nor implements its APIs, this method will only work if you target .NETFX (.NET Framework 4.5, 4.6, or 4.7). 
Mono runtime has several caveats. It's notorious for being buggy, so this might not always work.

Using your package manager, install Mono runtime (Debian/Raspbian: `sudo apt-get install mono-complete`). Once that is done, you will need to 
follow the [Mono instructions](/articles/getting_started/mono.html "Mono instructions and notes") to complete the project setup.

Once all is done, build your project, and transfer the artifacts to the Pi. Assuming your artifacts are in `~/mybot` and the executable is 
called `MyBot.exe`, you can run your bot by navigating to the directory (`cd ~/mybot`) and executing the executable with Mono (`mono MyBot.exe`).

## Final remarks

Do note that in order to keep the bot running after you disconnect, you will need a terminal multiplexer, such as `tmux` or `screen`. On top 
of that, you will need a way of ensuring the application is restarted after it crashes. Do note that if you run inside a docker container, the 
multiplexer is not necessary, albeit recommended.

The latter can be done via a simple bash script, or using a system process manager integrated into your distribution or init system. The latter 
is a proper approach, however it's not trivial.

Simplest bash script that can autorestart your bot (while giving you a chance to shut it down completely) will look similarly to this:

```bash
#!/bin/bash

echo Using `which dotnet`
while true
do
	dotnet "$1"
	echo "Application crashed, restarting in 5 seconds..."
	sleep 5
done
```

Save as `autorestart.sh`, make executable via `chmod +x autorestart.sh`, then run like `./autorestart.sh Your.Project.dll`, where `Your.Project.dll` 
is your bot's entry DLL. If you published for specific platform, just replace `dotnet "$1"` with `"./$1"`. For mono, just do `mono "$1"`.

And finally, it's recommended you run your bot in something like Docker. This can improve security, and you would be able to restrict the resources 
the bot is using to operate.

And ***never*** run your bot as `root`.
