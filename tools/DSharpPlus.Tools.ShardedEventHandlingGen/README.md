# Purpose
Manually maintaining the events that reside within `DiscordClient` is a ~~simple~~ task. Manually maintaining the events that reside within `DiscordShardedClient` is a tedious waste of time. To save time and pain for everyone involved, this tool copies over the events within `DiscordClient` to `DiscordShardedClient`. Additionally, through the power of `inheritdoc`, the documentation remains consistent as well.

# Usage
Go to the solution's root directory. The repository root. The directory with the `.git` folder. The place in which the `DSharpPlus.sln` file resides.

```bash
dotnet run --project tools/DSharpPlus.Tools.ShardedEventHandlingGen/DSharpPlus.Tools.ShardedEventHandlingGen.csproj
```