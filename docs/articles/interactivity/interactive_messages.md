---
uid: articles.interactivity.interactive_messages
title: Interactive Messages
---

Interactive messages are messages whose displayed content updates as users interact with them, with one-time setup from your bot. At their core, they are represented by a `Func<InteractiveMessageState, DiscordWebhookBuilder>` that gets invoked every time the user interacts with the message, but DSharpPlus provides abstractions to make this easier to handle. For sending them, extension methods are provided on `DiscordChannel` and `DiscordInteraction` with a set of overloads conditioning who may interact with the message.

## `InteractiveMessageBuilder`

`InteractiveMessageBuilder` is a special message builder for making interactive messages simpler to build. It supports adding special [interactive components](xref:DSharpPlus.Interactivity.InteractiveMessages.Components) that each replace a regular component: 

- `PaginatedTextDisplayComponent` creates a text display component that will receive content from the pages provided to the message,
- `PaginationButtonRowComponent` creates a button row with the configured pagination buttons, et cetera.

These components are then resolved into components Discord understands through the `IInteractiveComponentResolver` service, which simultaneously serves as a customization point: if you wish to modify or reimplement this process, you can do so by decorating or replacing the service implementation with your own.

> [!NOTE]
> The composition of the pagination button row can be modified through `InteractivityOptions.PaginationButtons`.

Specifying this template is mandatory when invoking `SendInteractiveMessageAsync`, however, unless legacy pagination is enabled, `SendPaginatedMessageAsync` provides a simplified API for creating paginated messages specifically. The default template for paginated messages where only content is passed can be customized through `InteractivityOptions.DefaultPaginatedMessageTemplate`.

In addition to our message template, we also have to pass content for the message to display: a collection of `Page`s. If you simply want to split a string into pages, DSharpPlus will do so automatically according to the provided `PageSplitType`, or you can manually generate the pages if each page represents a logical unit more than it represents contiguous text.

#### Page splitting

DSharpPlus by default supports splitting pages according to two modes: by lines or by characters. Splitting by lines operates on the basis of the `\n` character, not visual lines in the Discord client (since those will differ between display devices), and is therefore most useful for text that uses the line-break as an unit of organization, such as e.g. poetry:

~~~cs
// Macbeth, Act II, Scene I
string text = """
    Is this a dagger which I see before me,
    The handle toward my hand? Come, let me clutch thee.
    I have thee not, and yet I see thee still.
    Art thou not, fatal vision, sensible
    To feeling as to sight? or art thou but
    A dagger of the mind, a false creation,
    Proceeding from the heat-oppressed brain?
    I see thee yet, in form as palpable
    As this which now I draw.
    Thou marshall'st me the way that I was going;
    And such an instrument I was to use.
    Mine eyes are made the fools o' the other senses,
    Or else worth all the rest; I see thee still,
    And on thy blade and dudgeon gouts of blood,
    Which was not so before. There's no such thing:
    It is the bloody business which informs
    Thus to mine eyes. Now o'er the one halfworld
    Nature seems dead, and wicked dreams abuse
    The curtain'd sleep; witchcraft celebrates
    Pale Hecate's offerings, and wither'd murder,
    Alarum'd by his sentinel, the wolf,
    Whose howl's his watch, thus with his stealthy pace.
    With Tarquin's ravishing strides, towards his design
    Moves like a ghost. Thou sure and firm-set earth,
    Hear not my steps, which way they walk, for fear
    Thy very stones prate of my whereabout,
    And take the present horror from the time,
    Which now suits with it. Whiles I threat, he lives:
    Words to the heat of deeds too cold breath gives.
    """

// only targetUser can interact with this message
await channel.SendPaginatedMessageAsync(text, targetUser, PageSplitType.LineWise)
~~~

The default splitting mode, and the most suitable for prose text, is character-wise. DSharpPlus will attempt to cut off only after interpunction marks, so that sentences aren't randomly split across pages but at least have some indicator of coming continuation, but if that were to result in an overlong page, DSharpPlus will cut off at the end of a word. 

> [!NOTE]
> The splitting algorithm works by unicode categorization of what terminates a word. This works well enough for most usecases, but if your language tends to use sequences of hundreds of characters without unicode interpunction or whitespaces, you may need to create a text splitting algorithm tailored to your language's orthography and conventions.

#### Legacy features

Interactive messages use components v2 to their fullest potential by default, however, for legacy compatibility pagination still supports v1 messages and reactions instead of buttons. It is not recommended to use these features in new code, and existing bots are encouraged to migrate away as soon as possible. Customizability of these modes may be limited, new features will likely not be added and using reactions for pagination can easily result in exhausting ratelimits. These features may be removed in a future version of DSharpPlus.

The following section assumes you are already familiar with C#, DSharpPlus and Interactivity.

To enable legacy features, they have to be specified when registering interactivity:

~~~cs
services.AddInteractivityExtension(LegacyFeatures.UseV1Messages);

// using reactions implies v1 messages
services.AddInteractivityExtension(LegacyFeatures.UseReactionInteractivity);
~~~

These features will operate on the `SendPaginatedMessageAsync` overloads only.
