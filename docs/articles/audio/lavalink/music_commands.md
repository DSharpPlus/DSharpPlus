# Adding Music Commands

This article assumes that you know how to use CommandsNext. If you do not, you should learn [here](https://dsharpplus.github.io/articles/commands/intro.html) before continuing with this guide.

## Prerequisites 

Before we start we will need to make sure CommandsNext is configured. For this we can make a simple configuration and command class: 

```csharp
using DSharpPlus.CommandsNext;

namespace MyFirstMusicBot
{
    public class MyLavalinkCommands : BaseCommandModule
    {

    }
}
```

And be sure to register it in your program file: 

```csharp
CommandsNext = Discord.UseCommandsNext(new CommandsNextConfiguration
{
    StringPrefixes = new string[] { ";;" }
});

CommandsNext.RegisterCommands<MyLavalinkCommands>();
```

## Adding join and leave commands

Your bot, and Lavalink, will need to connect to a voice channel to play music. Let's create the base for these commands: 

```csharp
[Command]
public async Task Join(CommandContext ctx, DiscordChannel channel)
{
            
}

[Command]
public async Task Leave(CommandContext ctx, DiscordChannel channel)
{

}
```

In order to connect to a voice channel, we'll need to do a few things. 

1. Get our node connection. This is where our program's LavalinkNode comes in.
2. Check if the channel is a voice channel, and tell the user if not.
3. Connect the node to the channel. 


And for the leave command: 

1. Get the node connection.
2. Check if the channel is a voice channel, and tell the user if not.
3. Get our existing connection. 
4. Check if the connection exists, and tell the user if not.
5. Disconnect from the channel.

So far, your command class should look something like this: 

```csharp
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MyFirstMusicBot
{
    public class MyLavalinkCommands : BaseCommandModule
    {
        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel channel)
        {
            var node = Program.LavalinkNode;

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(channel);
            await ctx.RespondAsync($"Joined {channel.Name}!");
        }

        [Command]
        public async Task Leave(CommandContext ctx, DiscordChannel channel)
        {
            var node = Program.LavalinkNode;

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            var conn = node.GetConnection(channel.Guild);

            if(conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {channel.Name}!");
        }
    }
}
```

## Adding player commands

Now that we can join a voice channel, we can make our bot play music! Let's now create the base for a play command:

```csharp
[Command]
public async Task Play(CommandContext ctx, [RemainingText] string search)
{
    
}
```
One of Lavalink's best features is its ability to search for tracks from a variety of media sources, such as YouTube, SoundCloud, Twitch, and more. This is what makes bots like Rythm, Fredboat, and Groovy popular. The search is used in a REST request to get the track data, which is then sent through the WebSocket connection to play the track in the voice channel. That is what we will be doing in this command.

Lavalink can also play tracks directly from a media url, in which case the play command can look like this: 

```csharp
[Command]
public async Task Play(CommandContext ctx, Uri url)
{
    
}
```

Like before, we will need to get our node and guild connection and have the appropriate checks. Since it wouldn't make sense to have the channel as a parameter, we will instead get it from the member's voice state: 

```csharp
//Important to check the voice state itself first, 
//as it may throw a NullReferenceException if they don't have a voice state.
if(ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)  
{
    await ctx.RespondAsync("You are not in a voice channel.");
    return;
}

var node = Program.LavalinkNode;
var conn = node.GetConnection(ctx.Member.VoiceState.Guild);

if (conn == null)
{
    await ctx.RespondAsync("Lavalink is not connected.");
    return;
}
```

Next, we will get the track details by calling `node.Rest.GetTracksAsync()`. There are a variety of overloads for this:

1. `GetTracksAsync(LavalinkSearchType.Youtube, search)` will search YouTube for your search string. 
2. `GetTracksAsync(LavalinkSearchType.SoundCloud, search)` will search SoundCloud for your search string. 
3. `GetTracksAsync(Uri)` will use the direct url to obtain the track. This is mainly used for the other media sources. 

For this guide we will be searching YouTube. Let's pass in our search string and store the result in a variable: 

```csharp
//We don't need to specify the search type here
//since it is YouTube by default.
var loadResult = await node.Rest.GetTracksAsync(search);
```

The load result will contain an enum called `LoadResultType`, which will inform us if Lavalink was able to retrieve the track data. We can use this as a check: 

```csharp
//If something went wrong on Lavalink's end                          or it just couldn't find anything.
if(loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
{
    await ctx.RespondAsync($"Track search failed for {search}.");
    return;
}
```

Lavalink will return the track data from your search in a collection called `loadResult.Tracks`, similar to using the search bar in YouTube or SoundCloud directly. The first track is typically the most accurate one, so that is what we will use: 

```csharp
var track = loadResult.Tracks.First();
```

And finally, we can play the track:

```csharp
await conn.PlayAsync(track);

await ctx.RespondAsync($"Now playing {track.Title}!");
```

Your play command should look like this: 
```csharp
[Command]
public async Task Play(CommandContext ctx, [RemainingText] string search)
{
    if(ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
    {
        await ctx.RespondAsync("You are not in a voice channel.");
        return;
    }

    var node = Program.LavalinkNode;
    var conn = node.GetConnection(ctx.Member.VoiceState.Guild);

    if (conn == null)
    {
        await ctx.RespondAsync("Lavalink is not connected.");
        return;
    }

    var loadResult = await node.Rest.GetTracksAsync(search);

    if(loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
    {
        await ctx.RespondAsync($"Track search failed for {search}.");
        return;
    }

    var track = loadResult.Tracks.First();

    await conn.PlayAsync(track);

    await ctx.RespondAsync($"Now playing {track.Title}!");
}
```

Being able to pause the player is also useful. For this we can use most of the base from the play command: 

```csharp
[Command]
public async Task Pause(CommandContext ctx)
{
    if(ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
    {
        await ctx.RespondAsync("You are not in a voice channel.");
        return;
    }

    var node = Program.LavalinkNode;
    var conn = node.GetConnection(ctx.Member.VoiceState.Guild);

    if (conn == null)
    {
        await ctx.RespondAsync("Lavalink is not connected.");
        return;
    }
}
```

For this command we will also want to check the player state to determine if we should send a pause command. We can do so by checking `conn.CurrentState.CurrentTrack`:

if(conn.CurrentState.CurrentTrack == null)
{
    await ctx.RespondAsync("There are no tracks loaded.");
    return;
}

And finally, we can call pause: 

```csharp
await conn.PauseAsync();
```

The finished command should look like so:
```csharp
[Command]
public async Task Pause(CommandContext ctx)
{
    if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
    {
        await ctx.RespondAsync("You are not in a voice channel.");
        return;
    }

    var node = Program.LavalinkNode;
    var conn = node.GetConnection(ctx.Member.VoiceState.Guild);

    if (conn == null)
    {
        await ctx.RespondAsync("Lavalink is not connected.");
        return;
    }

    if(conn.CurrentState.CurrentTrack == null)
    {
         await ctx.RespondAsync("There are no tracks loaded.");
         return;
    }

    await conn.PauseAsync();
}
```

Of course, there are other commands Lavalink has to offer. Check out [the docs](https://dsharpplus.github.io/api/DSharpPlus.Lavalink.LavalinkGuildConnection.html#methods) to view the commands you can use while playing tracks.

There are also open source examples such as Emzi0767's [Companion Cube Bot](https://github.com/Emzi0767/Discord-Companion-Cube-Bot) and [Turret Bot](https://github.com/Emzi0767/Discord-Music-Turret-Bot).
