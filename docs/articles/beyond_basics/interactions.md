# Interactions

[Interactions](https://discord.com/developers/docs/interactions/receiving-and-responding#interactions) represent a user interacting with your bot.
This can happen in different ways. The most prominent case is Application Commands ("slash commands") but also button presses or context menus.

## Recieving interactions
Discord offers two ways to receive interactions: Through the gateway, and via an inbound HTTP webhook.
Currently DSharpPlus only supports the first one but there are plans to also integrate webhooks.
To recieve an interaction over the gateway you do not have to configure anything and simply register an EventHandler to the `InteractionCreated`event.

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
When responding to an Application Command you can defer your response and extend the window for the interaction to 15 minutes.
This defered response ("XY is thinking..." in the client) can later be edited to show your desired response.

### Message Components
Responding to a Message Component is pretty much the same as a reaction to a application command. 
Biggest difference is that you can use the `UpdateMessage` response type to directly update the message the component is located on.
This response type is also deferable with `DeferrredMessageUpdate`.


### Modals 
Also if you want to respond to any interaction with a modal it has to be the initial response.
When responding to a modal you can not respond with another Modal.

### Autocompletion
When responing to an autocompletion request you have to respond with `DiscordInteractionResponseType.AutoCompleteResult` and zero to 25 results within the 3 second window.


