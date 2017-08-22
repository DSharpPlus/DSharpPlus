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

# But im running on linux!

Even if you're running on linux, you will have to follow the following steps as well as those previously mentioned.

### 1. Locate NuGet.config
The file should be located in the .nuget/NuGet folder, located in your user folder. Once you find it, use a commandline text editor or copy the file to your computer to edit it. Inside the file should look something like this if its never been edited:
![NuGet.config Example](https://i.imgur.com/qvbjJo8.png)

### 2. Edit NuGet.config
Now you simply want to add this somewhere between `<packageSources>`:

`<add key="DSharpPlusNightly" value="https://www.myget.org/F/dsharpplus-nightly/api/v3/index.json" protocolVersion="3" />`

### 3. Finish Up
Save the NuGet.config, Goto your project folder in terminal, run `dotnet restore`, then `dotnet build` and lastly `dotnet <PathToDLL>` to start the bot.
