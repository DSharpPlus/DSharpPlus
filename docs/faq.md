---
uid: faq
title: Frequently Asked Questions
---

# Frequently Asked Questions

#### I have updated from an old version to the latest version and my project will not build!
Please read the latest [migration article][0] to see a list of changes, as new releases may contain breaking changes.

#### Code I copied from an article is not compiling or working as expected. Why?
*Please use the code snippets as a reference; don't blindly copy-paste code!*

The snippets of code in the articles are meant to serve as examples to help you understand how to use a part of the
library. Although most will compile and work at the time of writing, changes to the library over time can make some
snippets obsolete. Many issues can be resolved with Intellisense by searching for similarly named methods and verifying
method parameters.

#### I am targeting .NET Framework, Mono or Unity and have exceptions, crashes, or other problems.
As mentioned in the [preamble][1], the Mono runtime is inherently unstable and has numerous flaws. Because of this we
do not support Mono in any way, nor will we support any other projects which use it, including Unity.

.NET Framework is outdated and we are dropping support for it in major version 5.0; and we do not accept bug reports
or issues from .NET Framework.

Instead, we recommend using the most recent stable version of [.NET][2].

#### I see the latest stable version was released quite a while ago, what should I do?
You should consider targeting nightly versions if possible. They're usually about as stable as the stable version and
purely exist to allow us to iterate faster, implementing broader changes and adapting to Discord changes more effectively.
Many newer Discord features will only be implemented in nightly versions. To use them, specify to your nuget client that
you want to enable prereleases, either via CLI flag or a checkbox in your favourite IDE.

#### Connecting to a voice channel with VoiceNext will either hang or throw an exception.
To troubleshoot, please ensure that:
* You are using the latest version of DSharpPlus.
* You have properly enabled VoiceNext with your instance of @DSharpPlus.DiscordClient.
* You are *not* using VoiceNext in an event handler.
* You have [opus and libsodium][3] available in your target environment.


#### Why am I getting *heartbeat skipped* messages in my console?
Check your internet connection and ensure that the machine your bot is hosted on has a stable internet connection. If
your local network has no issues, the problem could be with either Discord or Cloudflare, in which case, it is out of your
control.

#### Why am I not getting message data?
Verify whether you have the Message Content intent enabled in both the developer dashboard and specified in your
DiscordConfiguration. If your bot is in more than 100 guilds, you will need approval for it from Discord.

#### Why am I getting a 4XX error and how can I fix it?
HTTP Error Code | Cause                       | Resolution
:--------------:|:----------------------------|:---------------------
`400`           | Malformed request.          | Catch the exception and inspect the `Errors` and `JsonMessage` properties - they will tell you what part of your request was malformed. If you need help figuring out what went wrong or suspect a library bug, feel free to contact us.
`401`           | Invalid token.              | Verify your token and make sure no errors were made. The client secret found on the 'general information' tab of your application page *is not* your token.
`403`           | Not enough permissions.     | Verify permissions and ensure your bot account has a role higher than the target user. Administrator permissions *do not* bypass the role hierarchy.
`404`           | Requested object not found. | This usually means the entity does not exist. A 404 response from an interaction (slash command, user/message context command, modal, button) generally means the interaction has expired - if that is the case, either defer the interaction or speed up the code that runs before making your response.
`429`           | Ratelimit hit.              | If you see one-off ratelimit errors, that's fine, you should reattempt or inform the user. If you can consistently reproduce this, you should report this to us with a trace log and as much code as possible. You may need to reduce the amount of requests you make, or you may have found a library issue.

#### I cannot modify a specific user or role. Why is this?
In order to modify a user, the highest role of your bot account must be higher than the target user. Changing the properties of a role requires that your bot account have a role higher than that role.

#### Does the command framework I use support dependency injection?
It does! However, they're all slightly different.

- If you use DSharpPlus.Commands, dependency injection happens through the constructor. One scope is created per command and used for everything contextually related to the command.
- If you use DSharpPlus.SlashCommands, dependency injection also happens through the constructor, but scopes don't always work as you might expect them to. Additionally, context checks do not support dependency injection - you will have to resort to the service locator pattern.
- If you use DSharpPlus.CommandsNext, dependency injection happens through constructors, properties and fields, and scopes don't always work as you might expect them to. You should refrain from using property and field injection and mark your fields private. Any public fields or properties you might need should be annotated as `[DontInject]` to prevent issues. Additionally, context checks and argument converters do not support dependency injection - as with SlashCommands, you will have to resort to the service locator pattern.

Furthermore, you should note that SlashCommands and CommandsNext will be deprecated soon and removed in a future release.

#### Can I use a user token?
Automating a user account is against Discord's Terms of Service and is not supported by DSharpPlus.

#### How can I set a custom status?
You can use either of the following methods (prefer the first one if possible, since it does not require a special API call).

- The overload for @DSharpPlus.DiscordClient.ConnectAsync(DSharpPlus.Entities.DiscordActivity,System.Nullable{DSharpPlus.Entities.UserStatus},System.Nullable{System.DateTimeOffset}) which accepts a @DSharpPlus.Entities.DiscordActivity.
- The overload for @DSharpPlus.DiscordClient.UpdateStatusAsync(DSharpPlus.Entities.DiscordActivity,System.Nullable{DSharpPlus.Entities.UserStatus},System.Nullable{System.DateTimeOffset}) which accepts a @DSharpPlus.Entities.DiscordActivity.
- The overload for @DSharpPlus.DiscordClient.UpdateStatusAsync(DSharpPlus.Entities.DiscordActivity,System.Nullable{DSharpPlus.Entities.UserStatus},System.Nullable{System.DateTimeOffset}) OR @DSharpPlus.DiscordShardedClient.UpdateStatusAsync(DSharpPlus.Entities.DiscordActivity,System.Nullable{DSharpPlus.Entities.UserStatus},System.Nullable{System.DateTimeOffset}) (for the sharded client) at any point after the connection has been established.

#### Am I able to retrieve an entity by name?
Yes, if you have the parent object. For example, if you are searching for a role in a guild, use LINQ on the `Roles` property and filter by
`DiscordRole.Name`. If you do not have the parent object or the property is not populated, you will either have to request it or find an
alternative way of solving your problem.

#### Why are you using Newtonsoft.Json when System.Text.Json is available?
Newtonsoft.Json is grandfathered in from the times before System.Text.Json was available, and migrating our codebase is a monumental task.
We are taking steps in that direction, it just takes a long time.

#### Why the hell are my events not firing?
This is because since version 8 of the Discord API, @DSharpPlus.DiscordIntents are required to be enabled on
@DSharpPlus.DiscordConfiguration and the Discord Application Portal. We have an [article][4] that covers all that has to
be done to set this up.

#### Speaking of events, where is my ready event?
You should avoid using the ready event in most cases; it does not indicate that your bot is ready to operate. For this reason, we have changed
the name in nightly builds to `SessionCreated`, which you can hook if you must, but generally you should prefer `GuildDownloadCompleted`, which
is fired when the bot is ready to operate.

#### And where are my errors? / My command just silently fails without an error!
The library catches exceptions and dispatches them to an event. DSharpPlus.Commands contains a default error handler that will inform you of any
errors, but we don't yet do this in all places (we're planning to). See the following useful table for what events to hook:

| Library | Error Location | Event
|:--------|:---------------|:-----
| DSharpPlus | any event handler | `DiscordClient.ClientErrored`
| DSharpPlus.CommandsNext | any command | `CommandsNextExtension.CommandErrored`
| DSharpPlus.SlashCommands | any command | `SlashCommandsExtension.SlashCommandErrored`
| DSharpPlus.SlashCommands | any autocomplete handler | `SlashCommandsExtension.AutocompleteErrored`
| DSharpPlus.Commands | anywhere | `CommandsExtension.CommandErrored`

This is also where you can retrieve the results of any pre-execution checks you may have registered.

#### Why does everything explode when I try to serialize entities or push them to a database?
Our entities are tightly bound to each other and their associated `DiscordClient` and cannot be serialized or deserialized without significant
involvement of library internals. If you need to store them, you should create your own serialization models that contain the data you need.

#### Why is everything sealed? Why can't I extend anything?
Because of how these library internals mentioned above work, inheriting from our entities rarely if ever does anything useful for you. If you
added another field, it couldn't be used, if you changed some method, you would risk the library breaking. There are some exceptions where an
abstract base type exists, and potentially some more where it may not - feel free to let us know - but in general, you should prefer extension
methods and custom helper methods.

#### Where are my pictures of spiderman?
![GOD DAMN IT PETER][5]

<!-- LINKS -->
[0]: xref:articles.migration.3x_to_4x
[1]: xref:articles.preamble
[2]: https://dotnet.microsoft.com/download
[3]: xref:articles.audio.voicenext.prerequisites
[4]: xref:articles.beyond_basics.intents
[5]: ./images/faq_spiderman.png
