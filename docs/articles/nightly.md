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