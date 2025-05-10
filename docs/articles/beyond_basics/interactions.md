---
uid: articles.beyond_basics.interactions
title: Interactions
---

[Interactions](https://discord.com/developers/docs/interactions/receiving-and-responding#interactions) represent a user interacting with your bot.
This can happen in different ways. The most prominent case is "slash commands" but also button presses or context menus.

## Recieving interactions
Discord offers two ways to receive interactions: Through the gateway, and via an inbound HTTP webhook.
Currently DSharpPlus only supports the first one but there are plans to also integrate webhooks.
To recieve an interaction over the gateway you do not have to configure anything and simply register an EventHandler to the `InteractionCreated` event.

In addition to that event we have some events that are filtered to provide some convienience:

- Buttons and Select menus -> `ComponentInteractionCreated`
- Modals -> `ModalSubmitted`
- User or Message context menu -> `ContextMenuInteractionCreated`
- Application Commands and autocompletion for those only in `InteractionCreated`

## Handling an interaction
The baseline is that every interaction has to be acknowledged in some fashion in the first 3 seconds after recieving it.
Available response types vary depending on the type of interaction.

> [!Important]
> The initial response has to decide if the repsonse should be ephemeral. You can NOT change this later.

### Application Commands
When responding to an Application Command ("Slash commands" or context menus) you can defer your response and extend the window for the interaction to 15 minutes.
This deferred response ("XY is thinking..." in the client) can later be edited to show your desired response.

### Message Components
Responding to a Message Component is pretty much the same as a reaction to a application command. 
The biggest difference is that you can use the `UpdateMessage` response type to directly update the message the component is located on.
This response type is also deferrable with `DeferrredMessageUpdate`.


### Modals 
If you want to respond to any interaction with a modal it has to be the initial response.
When responding to a modal you can not respond with another Modal.

### Autocompletion
When responding to an Autocomplete request you have to respond with `DiscordInteractionResponseType.AutoCompleteResult` and zero to 25 results within the 3 second window.


### Components V2

> [!Important]
> The following content cannot be edited on a message with the components V2 flag: `content`, `embeds`, `stickers`. Furthermore, messages cannot be "downgraded" from Components V2, only *upgraded*.

Components V2 is a relatively new addition to existing components, with 7 new component types, new APIs, and entirely new ways of visualizing content.
Enabling components V2 is as simple as calling `EnableComponentsV2` on a builder, such as `InteractionResponseBuilder`.

There are some things to take in mind with components V2 however; the biggest one is that once a message is V2, it is *always* V2.
This is a deliberate decision by Discord. Furthermore, messages with the V2 components flag (hereon referred to as Components V2/V2 Messages) only support using components (don't worry, you can still display text!) and setting attachments. However, these attachments *must* be referenced by a component (and for good reason!)

V2 Messages have some unique advantages however:
- Max top-level components doubled! **5 ➜ 10**
- Max total components increased from **25 ➜ 30**

Components V2 (specifically, components introduced by Components V2) do not go in action rows! Freedom alas.
Because of this, **we've also introduced a new API** `BaseDiscordMessageBuilder#AddRawComponents`; this method does *not* create an action row for you, so it is important to mind your usage of it.

> [!Note]
> Components V2 is not limited to interactions. This section may be moved in the future.

What are these new components?

- `Section Component`
  - Several sections (3) of text with an accessory (either a thumbnail or button 👀).
  - May support more components than just text in the future


- `Text Display Component`
  - A simple display of text, up to 4000 characters (summed across all text in the message)
  - Sections also count toward this, and also have the 4000-character limit.


- `Thumbnail Component`
  - A simple thumbnail, usable in sections


- `Media Gallery Component`
  - A collection of arbitrary media items (`DiscordMediaGalleryItem`)
  - Can be a remote url or a local file referenced via `attachment://my_file.png`


- `File Component`
  - A singular, arbitrary file
  - Also supports urls or `attachment://` attachments
  - Does not support native previews for text files
  - Can be spoilered


- `Separator Component`
  - Acts as a vertical spacer between components
  - Has two sizes, which are equivalent to 1 and 2 lines of text respectively.
  - Invisible by default, but can be set to render as a line (`divider = true`)

- `Container Component`
  - Arguably the coolest component-
  - Acts as a "container" for other components; can be colored like an embed
  - Can also be spoilered, blurring the entire container and components within
  - Holds action rows, and all new V2 components except containers
  - Holds up to 10 components
