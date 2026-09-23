---
uid: articles.basics.interactions.overview
title: An Overview on Interactions
---

[Interactions](https://discord.com/developers/docs/interactions/receiving-and-responding#interactions) represent a user interacting with your bot. This can happen in different ways. The most prominent case is application commands or button presses, but interactions come in many ways.

For receiving and handling interactions smoothly and conveniently, you might also want to check out our [commands framework](xref:articles.commands.interaction)

## Recieving interactions

Discord offers two ways to receive interactions: Through the gateway, and via an inbound HTTP webhook. To recieve an interaction over the gateway you do not have to configure anything and simply register an EventHandler to the `InteractionCreated` event. To receive interactions via HTTP, you must [set up support separately](xref:articles.advanced_topics.http_interactions_events). The same event handler will also handle HTTP events.

In addition to that event we have some events that are filtered to provide some convenience:

- Buttons and Select menus -> `ComponentInteractionCreated`
- Modals -> `ModalSubmitted`
- User or Message context menu -> `ContextMenuInteractionCreated`
- Application Commands and autocompletion for those are unfiltered in `InteractionCreated`

## Handling an interaction

Every interaction has to be acknowledged in some fashion in the first 3 seconds after recieving it. Available response types vary depending on the type of interaction: application commands can send a response or open a modal, components can in addition update the message they were initially attached to, ...

> [!Important]
> The initial response has to decide if the response should be ephemeral. You can NOT change this later.

### Application Commands

When responding to an application command (slash commands or context menus) you can defer your response and extend the window for the interaction to 15 minutes. This deferred response ("XY is thinking..." in the client) can later be edited to show your desired response.

### Message Components

Responding to a Message Component is pretty much the same as a reaction to a application command. The biggest difference is that you can use the `UpdateMessage` response type to directly update the message the component is located on. This response type is also deferrable with `DeferrredMessageUpdate`.

### Modals 

If you want to respond to any interaction with a modal it has to be the initial response and cannot be deferred. When responding to a modal you can not respond with another modal.

### Autocompletion

When responding to an autocomplete request you have to respond with `DiscordInteractionResponseType.AutoCompleteResult` and zero to 25 results within the 3 second window.
