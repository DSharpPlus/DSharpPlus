# I updated the library and now I'm drowning in red underline!
Much like 2.0 to 3.0, 3.0 to 4.0 introduces some breaking changes. As with 
previous major version bump, you will need to update your code.

## Basics
Default client implementation saw a huge number of fixes, as well as 
functional updates.

### WebSocket implementation changes
WebSocket implementations are now instantiated via factory delegates. This is 
now also a configuration setting, rather than method. For more information, 
see [Alternate WebSocket client implementations](/articles/getting_started/aternate_ws.html) 
page.

### Client changes
The client now supports proxies for both WebSocket and HTTP traffic. To proxy 
your traffic, create a new instance of `System.Net.WebProxy` and assign it to 
@DSharpPlus.DiscordConfiguration.Proxy property.

### Module to Extension rename
Extension classes were renamed. Instead of being called `SomethingModule` they 
are now `SomethingExtension`. This means that the following changed:

- `CommandsNextModule` -> `CommandsNextExtension`
- `InteractivityModule` -> `InteractivityExtension`
- `VoiceNextClient` -> `VoiceNextExtension`
- `BaseModule` -> `BaseExtension`

### Entity mutation changes
Entity updating methods now take an action which mutates the state of the 
object, instead of taking large lists of arguments. This means that instead of 
updating e.g. a role like this:

```cs
await role.UpdateAsync(name: "Modified Role", color: new DiscordColor(0xFF00FF));
```

you will update it like this:

```cs
await role.UpdateAsync(x =>
{
	x.Name = "Modified Role";
	x.Color = new DiscordColor(0xFF00FF);
});
```

### Other minor changes
- **User DM handling** - Users can no longer be DM'd directly. Instead, you 
  will need to find a member object for the user you want to DM, then use the 
  appropriate methods on the member object.
- **Channel permission override enhancements** - You can now query the member 
  or role objects for each permission override set on channels. Furthermore, 
  the overwrite building is now more intuitive.
- **Indefinite reconnecting** - the client can now be configured to attempt 
  reconnecting indefinitely.
- **Channel.Users** - you can now query users in voice and text channels by 
  using @DSharpPlus.Entities.DiscordChannel.Users property.
- **SendFileAsync argument reordering** - arguments for these methods were 
  reordered to prevent overload confusion.
- **New Discord features** - support for animated emoji and slow mode.

## CommandsNext
There were several major changes made to CommandsNext extension. While basics 
remain the same, some finer details are different.

### Multiprefix support
Prefixes are now configured via @DSharpPlus.CommandsNext.CommandsNextConfiguration.StringPrefixes 
instead of old `StringPrefix` property. Prefixes passed in this array will all 
function at the same time. At the same time, @DSharpPlus.CommandsNext.CommandContext 
class has been augmented with @DSharpPlus.CommandsNext.CommandContext.Prefix 
property, which allows for checking which prefix was used to trigger the 
command. Furthermore, the new @DSharpPlus.CommandsNext.Attributes.RequirePrefixesAttribute 
can be used as a check to require a specific prefix to be used with a command.

### Command hiding inheritance
Much like checks, the @DSharpPlus.CommandsNext.Attributes.HiddenAttribute is 
now inherited in modules which are not command groups.

### Support for `Nullable<T>` and `System.Uri` conversion
The default argument converters have been augmented to allow for conversion of 
nullable value types. No further configuration is required.

Furthermore, native support for `System.Uri` type now exists as well.

### Dependency Injection changes
CommandsNext now uses Microsoft's Dependency Injection abstractions, which 
greatly enhances flexibility, as well as allows 3rd party service containers 
to be used. For more information, see [Dependency injection](/articles/commands/dependency_injection.html) 
page.

### Command overloads and group commands
Command overloads are now implemented. This means you can create a command 
which takes multiple various argument type configurations. This is done by 
creating several commands and giving them all the same name.

Overloads need to have unique argument configurations, which means that it is 
possible to create commands which use the same argument types in different 
order (e.g. `int, string` and `string, int`), however you cannot create two 
overloads which have the same argument types and order.

Checks are pooled between all overloads, which means that specifying the same 
check on every overload will make it run several times; if you apply a check 
to a single overload, it will apply to all of them.

Group command is also done by marking a command with @DSharpPlus.CommandsNext.Attributes.GroupCommandAttribute 
instead of regular `CommandAttribute`. They can also be overloaded.

### Common module base
All command modules are now required to inherit from @DSharpPlus.CommandsNext.BaseCommandModule. 
This also enables the modules to use @DSharpPlus.CommandsNext.BaseCommandModule.BeforeExecutionAsync(DSharpPlus.CommandsNext.CommandContext) 
and @DSharpPlus.CommandsNext.BaseCommandModule.AfterExecutionAsync(DSharpPlus.CommandsNext.CommandContext).

### Module lifespans
It is now possible to create transient command modules. As opposed to regular 
singleton modules, which are instantiated upon registration, these modules are 
instantiated before every command call, and are disposed shortly after.

Combined with dependency injection changes, this enables the usage of 
transient and scoped modules.

For more information, see [Module lifespans](/articles/commands/module_lifespans.html) 
page.

### Help formatter changes
Help formatter is now lower level, because it now receives a command object 
and a group object. Furthermore, they are now also subject to dependency 
injection, receiving services and command context via DI.

Default help module is also transient, allowing it to take advantage of more 
advanced DI usages.

### Custom command handlers
You can now disabe the built-in command handler, and create your own. For more 
information, see [Custom Command Handlers](/articles/commands/custom_handler.html).

### Minor changes
- **Case-insensitivity changes** - case insensitivity now applies to command 
  name matching, prefix matching, and argument conversions.
- **DM help** - Default help can now be routed to DMs.
- **Custom attributes on commands** - CommandsNext now exposes all custom 
  attributes declared on commands, groups, and modules.
- **Implicit naming** - Commands can be named from their method or class name,
  by not giving it a name in the Command or Group attribute.
- **Argument converters are now asynchronous** - this allows using async code 
  in converters.

## Interactivity
TODO

## VoiceNext
TODO

## Lavalink
The library now comes with a Lavalink client, which supports both Lavalink 2.x 
and 3.x.

Lavalink is the preferred method for playing music from sources such as 
YouTube or SoundCloud. It's a standalone lightweight Java application, which 
handles downloading, transcoding, and transmitting audio to Discord.

For more information, see [Lavalink](/articles/lavalink/intro.html) page.
