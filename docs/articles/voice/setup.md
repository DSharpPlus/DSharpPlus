# Setting up VoiceNext

Before you can use VoiceNext, you need to appropriately set it up. Due to how voice works, you need to install certain native 
libraries before you can enjoy Discord's voice.

## 1. Identify your OS

The most important part is identifying the Operating System and CPU, OS, and .NET implementation architecture for your 
environment.

### Windows

Find **This PC**, right-click it, and go to **Properties**. In there, look for **System type**, as shown in the picture below:

![Identify - Windows](/images/05_01_identify_win32.jpg "Identifying Windows system")

### GNU/Linux

The first thing you will want to do is identifying your GNU/Linux distribution. You can do that by executing `lsb_release -a`. 
That should provide you with following information:

![Identify - GNU/Linux](/images/05_02_identify_gnulinux_lsb.png "Identifying LSB-compatible GNU/Linux system")

If that fails, try `ls -d1 /etc/* | grep '\-release$' | head -n 1 | xargs cat`. Output should look more or less like this

![Identify - GNU/Linux](/images/05_03_identify_gnulinux_osrelease.png "Identifying non-LSB-compatible GNU/Linux system")

### macOS

You use Mac, not much to identify here.

### BSD

To identify your BSD (or other UNIX system), you need to execute `uname -a`. It gives output similar to this one:

![Identify - BSD](/images/05_04_identify_bsd.png "Identifying BSD system")

## 2. Deploy your bot

Nothing much here, just deploy your bot as usual. If using .NETFX, copy the output files. When using .NET Core, publish the 
project, then copy the result to the target.

## 3. Install necessary libraries

For voice to work, you will need libraries for Opus and Sodium. Thankfully, this is not hard to obtain.

### Windows

Depending on whether your operating system is 32- or 64-bit, you will need x86 or x64 natives respectively. I have prebuilt 
opus and sodium natives for Windows available for download [here](/natives/index.html).

If you're in doubt about which libraries to use, follow this flowchart:

![Natives flowchart](/images/05_05_natives_flowchart.png "Natives - flowchart")

To install those, just add them to your project. Then change their properties so that they are always copied on build.

Do note that when debugging, you will usually need 32-bit natives, regardless of OS architecture.

### GNU/Linux

You will need to install `libopus` and `libsodium` from your distro repositories. Depending on your distribution, the package 
names and installation method may vary.

#### Debian, Devuan, Raspbian, Ubuntu, and derivatives

Depending on your distro and version, you might need to install different packages.

* **Debian/Devuan/Raspbian Oldstable**: `$ sudo apt-get install libopus0 libsodium13 libopus-dev libsodium-dev`
* **Debian/Devuan/Raspbian Stable**: `$ sudo apt-get install libopus0 libsodium18 libopus-dev libsodium-dev`
* **Ubuntu 14.04 LTS**: `$ sudo add-apt-repository ppa:chris-lea/libsodium && sudo apt-get update && sudo apt-get install libopus0 libsodium libopus-dev libsodium-dev`
* **Ubuntu 16.04 LTS, 16.10, 17.04**: `$ sudo apt-get install libopus0 libsodium18 libopus-dev libsodium-dev`

#### Fedora

**NOTE**: I do not own a Fedora box, therefore the below might require tweaking:

`$ sudo dnf install opus libsodium opus-devel libsodium-devel`

#### Arch

**NOTE**: I do not own an Arch box, therefore the below might require tweaking:

`$ sudo pacman -S opus libsodium`

#### Gentoo

**NOTE**: I do not own a Gentoo box, therefore the below might require tweaking:

`$ sudo emerge -atv opus libsodium`

### macOS

**NOTE**: I do not own a Mac, therefore the below might require tweaking:

`$ brew install opus libsodium`

### FreeBSD

On FreeBSD installing the libraries is as simple as executing the following:

`# pkg install opus libsodium`

## 4. Optional: install FFmpeg

### Windows

Depending on whether your operating system is 32- or 64-bit, you will need x86 or x64 FFmpeg build respectively. I have 
slimmed down FFmpeg distribution for Windows available for download [here](/natives/index.html).

### GNU/Linux

You will need to install `ffmpeg` from your distro repositories. Depending on your distribution, the package names and 
installation method may vary.

#### Debian, Devuan, Raspbian, Ubuntu, and derivatives

Depending on your distro and version, you might need to install different packages.

**NOTE**: The installation procedure for Debian Oldstable is experimental. I will not take any responsibility for any damage 
caused to your system.

* **Debian/Devuan/Raspbian Oldstable**: `$ echo 'deb http://ftp.debian.org/debian jessie-backports main' | sudo tee -a /etc/apt/sources.list && sudo apt-get update && sudo apt-get install ffmpeg`
* **Debian/Devuan/Raspbian Stable**: `$ sudo apt-get install ffmpeg`
* **Ubuntu 14.04 LTS**: `$ sudo add-apt-repository ppa:mc3man/trusty-media && sudo apt-get update && sudo apt-get install ffmpeg`
* **Ubuntu 16.04 LTS, 16.10, 17.04**: `$ sudo apt-get install ffmpeg`

#### Fedora

**NOTE**: I do not own a Fedora box, therefore the below might require tweaking:

`$ sudo dnf install ffmpeg`

#### Arch

**NOTE**: I do not own an Arch box, therefore the below might require tweaking:

`$ sudo pacman -S ffmpeg`

#### Gentoo

**NOTE**: I do not own a Gentoo box, therefore the below might require tweaking:

`$ sudo emerge -atv ffmpeg`

### macOS

**NOTE**: I do not own a Mac, therefore the below might require tweaking:

`$ brew install ffmpeg --with-fdk-aac --with-sdl2 --with-freetype --with-frei0r --with-libass --with-libvorbis --with-libvpx --with-opencore-amr --with-openjpeg --with-opus --with-rtmpdump --with-speex --with-theora --with-tools`

### FreeBSD

On FreeBSD installing FFmpeg is as simple as executing the following:

`# pkg install ffmpeg`
