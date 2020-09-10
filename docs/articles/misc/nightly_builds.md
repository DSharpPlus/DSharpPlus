# I like living on the edge - give me the freshest builds

We offer nightly builds for DSharpPlus. They contain bugfixes and new features before the NuGet releases, however they are 
not guaranteed to be stable, or work at all.

If you want to use them, you need to open your **Package Manager Settings** in Visual Studio, and add the following SlimGet 
feed to the package sources:

`https://nuget.emzi0767.com/api/v3/index.json`

Then open the NuGet interface for your project, check **Prerelease**, and make sure the **package source** is set to the SlimGet 
feed you just added.

Then just select **Latest prerelease** version of DSharpPlus packages, and install them.

You might need to restart Visual Studio for changes to take effect.

If you find any problems in the SlimGet versions of the packages, please follow the instructions in [Reporting issues](/articles/advanced/reporting_issues.html) 
article.

## But I'm running GNU/Linux, Mac OS X, or BSD!

If you're running on a non-Windows OS, you'll have to get your hands dirty. Prepare your text editor and file browser.

Inside `~/.nuget/NuGet` directory, there should be a file called `NuGet.config`. It should look more or less like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

Inside the `packageSources` element, you will need to add the following:

`<add key="DSharpPlus SlimGet" value="https://nuget.emzi0767.com/api/v3/index.json" />`

Once that's done, save the file. If you run `dotnet restore` right now, it should be able to restore the packages without problems.
