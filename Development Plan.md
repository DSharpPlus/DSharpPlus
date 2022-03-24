# Development Plan
The v5 rewrite has long been in planning, with little to no central place to keep track of all of the details. To remedy this, here's where you can find all of the ideas set in stone.

# DSharpPlus.Core
This project will contain raw entities that correctly reflect the current Discord API. Entities can be found over on the [Discord Documentation](https://discord.com/developers/docs/intro). This part of the lib is meant for power users.

DSharpPlus.Core will have no cache, no "convenience" properties such as `DiscordUser.DefaultAvatarUrl` or any other extra features. This package is meant for those who want to get down and dirty with the Discord API, or simply want to be as efficient as possible with their resources.

## Raw Entities
Try to copy the documentation in `<summary>` XML tag, with any exceptions or notes in the `<remarks>` XML tag. If you feel further clarification is required, you're welcome to add your own notes in a `<para>` XML tag.

We have our own implementation of `Snowflake`s and `Optional<T>`s, which should be put to the fullest use whenever possible. Our goal is to copy entities as closely as possible.

We're following v4 practice and having entities be placed in `DSharpPlus.Core/Entities/<subfolder>/` using the `DSharpPlus.Entities` namespace. Provided your IDE/Editor follows the `.editorconfig` rules, there shouldn't be any warnings or notices about this.

## Clients
The clients will be implemented in the following order:
- `DiscordRestClient`
- `DiscordGatewayClient`
- `DiscordClient`
- `DiscordShardedClient`

## Rest Requests
Rest Requests still belong on their proper objects, much like their v4 counterparts. The `DiscordRestClient` will have their own methods which allow making requests to undocumented endpoints. Something like such:
```cs
public async Task DeleteChannelAsync(this DiscordChannel channel, DiscordClient client, string? reason) =>
    client.DoRestRequestAsync(new RestRequest {
    	Payload = new ChannelDeletionRestRequest(channel),
    	Headers = new[] { $"X-Audit-Log-Reason: {reason}" }
    	Url = undefinedChannelEndpointVar
    });
```

## Async Events
Events will need to be asynchronous. The only major "issue" (which could be interpreted as more of a complaint) with Emzi's Async Event lib is that it yells at you if your event takes too long to execute. Further discussion will have to be held on the Discord regarding these.

# DSharpPlus.Abstraction
This is the part of the lib that people will likely use. It holds a cache, it has convenience properties and other positive features. Each class will internally hold a reference to its raw entity, likely shown as below:
```cs
public sealed class DSharpUser {
	internal DiscordUser RawUser { get; init; }
	public DefaultAvatarUrl => $"https://cdn.discordapp.com/embed/avatars/{(this.RawUser.Discriminator % 5).ToString(CultureInfo.InvariantCulture)}.png?size=4096"; // This url should be using the ENDPOINTS static class
}
```

# Extensions
Everyone wants to rewrite the current extensions. We've received several verbal confirmations that there would be multiple people who'd help contribute. As things are right now though, it's too early to start even dreaming how the extensions would look. We acknowledge that there are many existing ideas, both new and old, desperately wanting to be implemented. As stated before, it's just too early. We know for sure that the following extensions *will*  be remade:

| Extension Name | Purpose |
| --- | --- |
| TextCommands | Text-Based commands, such as `!ping` |
| InteractionCommands | Contains everything related to interactions. Slash commands, user and message (not text!) commands, buttons and modals. |
| Interactivity | Contains asynchronous methods that usually involves waiting (I.E, next message on channel, interactable help menus that use buttons and waiting for modal input) |
| VoiceNext | An extension that allows interfacing with voice channels, capable of both receiving and sending packets. |
| MixedCommands | A mix of both text and slash commands, additionally accounting for whatever new forms of user input that Discord brings to the table. |

There are no other extensions currently planned, however, the extensions API will be focused on a bit more. It is still undecided on whether the extensions API will reflect `DSharpPlus.Core`, `DSharpPlus.Abstraction`, or both.
# That's it!
That's how DSharpPlus v5.0.0 is expected to look. The development plans are expected to change at any time but usually not without major chatting beforehand in #lib-discussion. If you have any questions, you're welcome to ask in #lib-discussion, which can be found in the [Discord](https://discord.gg/dsharpplus). 