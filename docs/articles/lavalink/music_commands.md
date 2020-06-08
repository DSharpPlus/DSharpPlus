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

[INSERT PICTURES HERE]

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

Like before, we will need to get our node and guild connection, with the appropriate checks: 

```csharp
var node = Program.LavalinkNode;
var conn = node.GetConnection(channel.Guild);

if(conn == null)
{
    await ctx.RespondAsync("Lavalink is not connected.");
    return;
}
```
