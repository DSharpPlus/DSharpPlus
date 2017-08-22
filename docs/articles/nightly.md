# I like living on the edge - give me the freshest builds

We offer nightly builds for DSharpPlus. They contain bufixes and new features before the NuGet releases, however they are 
not guaranteed to be stable, or work at all.

If you want to use them, you need to open your **Package Manager Settings** in Visual Studio, and add the following MyGet 
feed to the package sources:

`https://www.myget.org/F/dsharpplus-nightly/api/v3/index.json`

Then open the NuGet interface for your project, check **Prerelease**, and make sure the **package source** is set to the MyGet 
feed you just added.

Then just select **Latest prerelease** version of DSharpPlus packages, and install them.

You might need to restart Visual Studio for changes to take effect.

If you find any problems in the MyGet versions of the packages, please follow the instructions in [Reporting issues](/articles/issues.html) 
article.

# But I'm running Linux!

If you're running Linux, you'll have to do the following to add the MyGet feed to your NuGet sources.

### 1. Locate NuGet.config
This file should be located in your .nuget/NuGet directory, found inside of your user folder. Once you find it, use your preferred text editor or download it to your computer (if you're on a VPS) to edit the file. The file should look similar to this:
![NuGet.config Example](https://i.imgur.com/qvbjJo8.png)

### 2. Edit NuGet.config
Now you'll simply want to add this somewhere between your `<packageSources>`:

`<add key="DSharpPlusNightly" value="https://www.myget.org/F/dsharpplus-nightly/api/v3/index.json" protocolVersion="3" />`

### 3. Finish Up
Save NuGet.config, open your project directory with a terminal, run `dotnet restore`, `dotnet build` and at last `dotnet <PathToDLL>` to start your bot.
