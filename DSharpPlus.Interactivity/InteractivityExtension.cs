using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

namespace DSharpPlus.Interactivity;

/// <summary>
/// Extension class for DSharpPlus.Interactivity
/// </summary>
public class InteractivityExtension : IDisposable
{
    internal readonly ConcurrentDictionary<Type, AsyncEvent> eventDistributor = [];
    internal IClientErrorHandler errorHandler;

#pragma warning disable IDE1006 // Naming Styles
    internal InteractivityConfiguration Config { get; }
    public DiscordClient Client { get; private set; }

    private EventWaiter<MessageCreatedEventArgs> MessageCreatedWaiter;

    private EventWaiter<MessageReactionAddedEventArgs> MessageReactionAddWaiter;

    private EventWaiter<TypingStartedEventArgs> TypingStartWaiter;

    private EventWaiter<ComponentInteractionCreatedEventArgs> ComponentInteractionWaiter;

    internal ComponentEventWaiter ComponentEventWaiter;

    internal ModalEventWaiter ModalEventWaiter;

    internal ReactionCollector ReactionCollector;

    internal Poller Poller;

    internal Paginator Paginator;
    internal ComponentPaginator compPaginator;

#pragma warning restore IDE1006 // Naming Styles

    internal InteractivityExtension(InteractivityConfiguration cfg) => this.Config = new InteractivityConfiguration(cfg);

    public void Setup(DiscordClient client)
    {
        this.Client = client;
        this.MessageCreatedWaiter = new EventWaiter<MessageCreatedEventArgs>(this);
        this.MessageReactionAddWaiter = new EventWaiter<MessageReactionAddedEventArgs>(this);
        this.ComponentInteractionWaiter = new EventWaiter<ComponentInteractionCreatedEventArgs>(this);
        this.TypingStartWaiter = new EventWaiter<TypingStartedEventArgs>(this);
        this.Poller = new Poller(this.Client);
        this.ReactionCollector = new ReactionCollector(this);
        this.Paginator = new Paginator(this.Client);
        this.compPaginator = new(this.Client, this.Config);
        this.ComponentEventWaiter = new(this.Client, this.Config);
        this.ModalEventWaiter = new(this.Client);
        this.errorHandler = new DefaultClientErrorHandler(this.Client.Logger);
    }

    /// <summary>
    /// Makes a poll and returns poll results.
    /// </summary>
    /// <param name="m">Message to create poll on.</param>
    /// <param name="emojis">Emojis to use for this poll.</param>
    /// <param name="behaviour">What to do when the poll ends.</param>
    /// <param name="timeout">override timeout period.</param>
    /// <returns></returns>
    public async Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(DiscordMessage m, IEnumerable<DiscordEmoji> emojis, PollBehaviour? behaviour = default, TimeSpan? timeout = null)
    {
        if (!Utilities.HasReactionIntents(this.Client.Intents))
        {
            throw new InvalidOperationException("No reaction intents are enabled.");
        }

        if (!emojis.Any())
        {
            throw new ArgumentException("You need to provide at least one emoji for a poll!");
        }

        foreach (DiscordEmoji em in emojis)
        {
            await m.CreateReactionAsync(em);
        }

        ReadOnlyCollection<PollEmoji> res = await this.Poller.DoPollAsync(new PollRequest(m, timeout ?? this.Config.Timeout, emojis));

        PollBehaviour pollbehaviour = behaviour ?? this.Config.PollBehaviour;
        DiscordMember thismember = await m.Channel.Guild.GetMemberAsync(this.Client.CurrentUser.Id);

        if (pollbehaviour == PollBehaviour.DeleteEmojis && m.Channel.PermissionsFor(thismember).HasPermission(DiscordPermissions.ManageMessages))
        {
            await m.DeleteAllReactionsAsync();
        }

        return new ReadOnlyCollection<PollEmoji>(res.ToList());
    }

    /// <summary>
    /// Waits for a modal with the specified id to be submitted.
    /// </summary>
    /// <param name="modal_id">The id of the modal to wait for. Should be unique to avoid issues.</param>
    /// <param name="timeoutOverride">Override the timeout period in <see cref="InteractivityConfiguration"/>.</param>
    /// <returns>A <see cref="InteractivityResult{ModalSubmitEventArgs}"/> with a modal if the interactivity did not time out.</returns>
    public Task<InteractivityResult<ModalSubmittedEventArgs>> WaitForModalAsync(string modal_id, TimeSpan? timeoutOverride = null)
        => WaitForModalAsync(modal_id, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for a modal with the specified id to be submitted.
    /// </summary>
    /// <param name="modal_id">The id of the modal to wait for. Should be unique to avoid issues.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <returns>A <see cref="InteractivityResult{ModalSubmitEventArgs}"/> with a modal if the interactivity did not time out.</returns>
    public async Task<InteractivityResult<ModalSubmittedEventArgs>> WaitForModalAsync(string modal_id, CancellationToken token)
    {
        if (string.IsNullOrEmpty(modal_id) || modal_id.Length > 100)
        {
            throw new ArgumentException("Custom ID must be between 1 and 100 characters.");
        }

        ModalMatchRequest matchRequest = new(modal_id,
                c => c.Interaction.Data.CustomId == modal_id, cancellation: token);
        ModalSubmittedEventArgs? result = await this.ModalEventWaiter.WaitForMatchAsync(matchRequest);

        return new(result is null, result);
    }

    /// <summary>
    /// Waits for a modal with the specificed custom id to be submitted by the given user.
    /// </summary>
    /// <param name="modal_id">The id of the modal to wait for. Should be unique to avoid issues.</param>
    /// <param name="user">The user to wait for the modal from.</param>
    /// <param name="timeoutOverride">Override the timeout period in <see cref="InteractivityConfiguration"/>.</param>
    /// <returns>A <see cref="InteractivityResult{ModalSubmitEventArgs}"/> with a modal if the interactivity did not time out.</returns>
    public Task<InteractivityResult<ModalSubmittedEventArgs>> WaitForModalAsync(string modal_id, DiscordUser user, TimeSpan? timeoutOverride = null)
        => WaitForModalAsync(modal_id, user, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for a modal with the specificed custom id to be submitted by the given user.
    /// </summary>
    /// <param name="modal_id">The id of the modal to wait for. Should be unique to avoid issues.</param>
    /// <param name="user">The user to wait for the modal from.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <returns>A <see cref="InteractivityResult{ModalSubmitEventArgs}"/> with a modal if the interactivity did not time out.</returns>
    public async Task<InteractivityResult<ModalSubmittedEventArgs>> WaitForModalAsync(string modal_id, DiscordUser user, CancellationToken token)
    {
        if (string.IsNullOrEmpty(modal_id) || modal_id.Length > 100)
        {
            throw new ArgumentException("Custom ID must be between 1 and 100 characters.");
        }

        ModalMatchRequest matchRequest = new(modal_id,
                c => c.Interaction.Data.CustomId == modal_id &&
                c.Interaction.User.Id == user.Id, cancellation: token);
        ModalSubmittedEventArgs? result = await this.ModalEventWaiter.WaitForMatchAsync(matchRequest);

        return new(result is null, result);
    }

    /// <summary>
    /// Waits for any button in the specified collection to be pressed.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="buttons">A collection of buttons to listen for.</param>
    /// <param name="timeoutOverride">Override the timeout period in <see cref="InteractivityConfiguration"/>.</param>
    /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
    /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
    public Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, IEnumerable<DiscordButtonComponent> buttons, TimeSpan? timeoutOverride = null)
        => WaitForButtonAsync(message, buttons, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for any button in the specified collection to be pressed.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="buttons">A collection of buttons to listen for.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
    /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
    public async Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, IEnumerable<DiscordButtonComponent> buttons, CancellationToken token)
    {
        if (message.Author != this.Client.CurrentUser)
        {
            throw new InvalidOperationException("Interaction events are only sent to the application that created them.");
        }

        if (!buttons.Any())
        {
            throw new ArgumentException("You must specify at least one button to listen for.");
        }

        if (message.Components.Count == 0)
        {
            throw new ArgumentException("Provided message does not contain any components.");
        }

        if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is DiscordComponentType.Button))
        {
            throw new ArgumentException("Provided message does not contain any button components.");
        }

        ComponentInteractionCreatedEventArgs? res = await this.ComponentEventWaiter
            .WaitForMatchAsync(new(message,
                c =>
                    c.Interaction.Data.ComponentType == DiscordComponentType.Button &&
                    buttons.Any(b => b.CustomId == c.Id), token));

        return new(res is null, res);
    }

    /// <summary>
    /// Waits for any button on the specified message to be pressed.
    /// </summary>
    /// <param name="message">The message to wait for the button on.</param>
    /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
    /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
    /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
    public Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, TimeSpan? timeoutOverride = null)
        => WaitForButtonAsync(message, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for any button on the specified message to be pressed.
    /// </summary>
    /// <param name="message">The message to wait for the button on.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
    /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
    public async Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, CancellationToken token)
    {
        if (message.Author != this.Client.CurrentUser)
        {
            throw new InvalidOperationException("Interaction events are only sent to the application that created them.");
        }

        if (message.Components.Count == 0)
        {
            throw new ArgumentException("Provided message does not contain any components.");
        }

        if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is DiscordComponentType.Button))
        {
            throw new ArgumentException("Provided message does not contain any button components.");
        }

        IEnumerable<string> ids = message.Components.SelectMany(m => m.Components).Select(c => c.CustomId);

        ComponentInteractionCreatedEventArgs? result =
            await
            this.ComponentEventWaiter
            .WaitForMatchAsync(new(message, c => c.Interaction.Data.ComponentType == DiscordComponentType.Button && ids.Contains(c.Id), token))
            ;

        return new(result is null, result);
    }

    /// <summary>
    /// Waits for any button on the specified message to be pressed by the specified user.
    /// </summary>
    /// <param name="message">The message to wait for the button on.</param>
    /// <param name="user">The user to wait for the button press from.</param>
    /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
    /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
    /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
    public Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, DiscordUser user, TimeSpan? timeoutOverride = null)
        => WaitForButtonAsync(message, user, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for any button on the specified message to be pressed by the specified user.
    /// </summary>
    /// <param name="message">The message to wait for the button on.</param>
    /// <param name="user">The user to wait for the button press from.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
    /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
    public async Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, DiscordUser user, CancellationToken token)
    {
        if (message.Author != this.Client.CurrentUser)
        {
            throw new InvalidOperationException("Interaction events are only sent to the application that created them.");
        }

        if (message.Components.Count == 0)
        {
            throw new ArgumentException("Provided message does not contain any components.");
        }

        if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is DiscordComponentType.Button))
        {
            throw new ArgumentException("Provided message does not contain any button components.");
        }

        ComponentInteractionCreatedEventArgs? result = await
            this.ComponentEventWaiter
            .WaitForMatchAsync(new(message, (c) => c.Interaction.Data.ComponentType is DiscordComponentType.Button && c.User == user, token))
            ;

        return new(result is null, result);

    }

    /// <summary>
    /// Waits for a button with the specified Id to be pressed.
    /// </summary>
    /// <param name="message">The message to wait for the button on.</param>
    /// <param name="id">The Id of the button to wait for.</param>
    /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
    /// <returns>A <see cref="InteractivityResult{T}"/> with the result of the operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
    /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
    public Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, string id, TimeSpan? timeoutOverride = null)
        => WaitForButtonAsync(message, id, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for a button with the specified Id to be pressed.
    /// </summary>
    /// <param name="message">The message to wait for the button on.</param>
    /// <param name="id">The Id of the button to wait for.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A <see cref="InteractivityResult{T}"/> with the result of the operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
    /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
    public async Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, string id, CancellationToken token)
    {
        if (message.Author != this.Client.CurrentUser)
        {
            throw new InvalidOperationException("Interaction events are only sent to the application that created them.");
        }

        if (message.Components.Count == 0)
        {
            throw new ArgumentException("Provided message does not contain any components.");
        }

        if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is DiscordComponentType.Button))
        {
            throw new ArgumentException("Provided message does not contain any button components.");
        }

        if (!message.Components.SelectMany(c => c.Components).OfType<DiscordButtonComponent>().Any(c => c.CustomId == id))
        {
            throw new ArgumentException($"Provided message does not contain button with Id of '{id}'.");
        }

        ComponentInteractionCreatedEventArgs? result = await
            this.ComponentEventWaiter
            .WaitForMatchAsync(new(message, (c) => c.Interaction.Data.ComponentType is DiscordComponentType.Button && c.Id == id, token))
            ;

        return new(result is null, result);
    }

    /// <summary>
    /// Waits for any button to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="predicate">The predicate to filter interactions by.</param>
    /// <param name="timeoutOverride">Override the timeout specified in <see cref="InteractivityConfiguration"/></param>
    public Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, Func<ComponentInteractionCreatedEventArgs, bool> predicate, TimeSpan? timeoutOverride = null)
        => WaitForButtonAsync(message, predicate, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for any button to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="predicate">The predicate to filter interactions by.</param>
    /// <param name="token">A token to cancel interactivity with at any time. Pass <see cref="CancellationToken.None"/> to wait indefinitely.</param>
    public async Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(DiscordMessage message, Func<ComponentInteractionCreatedEventArgs, bool> predicate, CancellationToken token)
    {
        if (message.Author != this.Client.CurrentUser)
        {
            throw new InvalidOperationException("Interaction events are only sent to the application that created them.");
        }

        if (message.Components.Count == 0)
        {
            throw new ArgumentException("Provided message does not contain any components.");
        }

        if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is DiscordComponentType.Button))
        {
            throw new ArgumentException("Provided message does not contain any button components.");
        }

        ComponentInteractionCreatedEventArgs? result = await
            this.ComponentEventWaiter
            .WaitForMatchAsync(new(message, c => c.Interaction.Data.ComponentType is DiscordComponentType.Button && predicate(c), token))
            ;

        return new(result is null, result);
    }

    /// <summary>
    /// Waits for any dropdown to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait for.</param>
    /// <param name="predicate">A filter predicate.</param>
    /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the message doesn't contain any dropdowns</exception>
    public Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForSelectAsync(DiscordMessage message, Func<ComponentInteractionCreatedEventArgs, bool> predicate, TimeSpan? timeoutOverride = null)
        => WaitForSelectAsync(message, predicate, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for any dropdown to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait for.</param>
    /// <param name="predicate">A filter predicate.</param>
    /// <param name="token">A token that can be used to cancel interactivity. Pass <see cref="CancellationToken.None"/> to wait indefinitely.</param>
    /// <exception cref="ArgumentException">Thrown when the message doesn't contain any dropdowns</exception>
    public async Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForSelectAsync(DiscordMessage message, Func<ComponentInteractionCreatedEventArgs, bool> predicate, CancellationToken token)
    {
        if (message.Author != this.Client.CurrentUser)
        {
            throw new InvalidOperationException("Interaction events are only sent to the application that created them.");
        }

        if (message.Components.Count == 0)
        {
            throw new ArgumentException("Provided message does not contain any components.");
        }

        if (!message.Components.SelectMany(c => c.Components).Any(IsSelect))
        {
            throw new ArgumentException("Provided message does not contain any select components.");
        }

        ComponentInteractionCreatedEventArgs? result = await
            this.ComponentEventWaiter
            .WaitForMatchAsync(new(message, c => IsSelect(c.Interaction.Data.ComponentType) && predicate(c), token))
            ;

        return new(result is null, result);
    }

    /// <summary>
    /// Waits for a dropdown to be interacted with.
    /// </summary>
    /// <remarks>This is here for backwards-compatibility and will internally create a cancellation token.</remarks>
    /// <param name="message">The message to wait on.</param>
    /// <param name="id">The Id of the dropdown to wait on.</param>
    /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
    public Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForSelectAsync(DiscordMessage message, string id, TimeSpan? timeoutOverride = null)
        => WaitForSelectAsync(message, id, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for a dropdown to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="id">The Id of the dropdown to wait on.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
    public async Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForSelectAsync(DiscordMessage message, string id, CancellationToken token)
    {
        if (message.Author != this.Client.CurrentUser)
        {
            throw new InvalidOperationException("Interaction events are only sent to the application that created them.");
        }

        if (message.Components.Count == 0)
        {
            throw new ArgumentException("Provided message does not contain any components.");
        }

        if (!message.Components.SelectMany(c => c.Components).Any(IsSelect))
        {
            throw new ArgumentException("Provided message does not contain any select components.");
        }

        if (message.Components.SelectMany(c => c.Components).Where(IsSelect).All(c => c.CustomId != id))
        {
            throw new ArgumentException($"Provided message does not contain select component with Id of '{id}'.");
        }

        ComponentInteractionCreatedEventArgs? result = await
            this.ComponentEventWaiter
            .WaitForMatchAsync(new(message, (c) => IsSelect(c.Interaction.Data.ComponentType) && c.Id == id, token))
            ;

        return new(result is null, result);
    }

    private bool IsSelect(DiscordComponent component)
        => IsSelect(component.Type);

    private static bool IsSelect(DiscordComponentType type)
        => type is
            DiscordComponentType.StringSelect or
            DiscordComponentType.UserSelect or
            DiscordComponentType.RoleSelect or
            DiscordComponentType.MentionableSelect or
            DiscordComponentType.ChannelSelect;

    /// <summary>
    /// Waits for a dropdown to be interacted with by a specific user.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="user">The user to wait on.</param>
    /// <param name="id">The Id of the dropdown to wait on.</param>
    /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
    public Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForSelectAsync(DiscordMessage message, DiscordUser user, string id, TimeSpan? timeoutOverride = null)
        => WaitForSelectAsync(message, user, id, GetCancellationToken(timeoutOverride));

    /// <summary>
    /// Waits for a dropdown to be interacted with by a specific user.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="user">The user to wait on.</param>
    /// <param name="id">The Id of the dropdown to wait on.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
    public async Task<InteractivityResult<ComponentInteractionCreatedEventArgs>> WaitForSelectAsync(DiscordMessage message, DiscordUser user, string id, CancellationToken token)
    {
        if (message.Author != this.Client.CurrentUser)
        {
            throw new InvalidOperationException("Interaction events are only sent to the application that created them.");
        }

        if (message.Components.Count == 0)
        {
            throw new ArgumentException("Provided message does not contain any components.");
        }

        if (!message.Components.SelectMany(c => c.Components).Any(IsSelect))
        {
            throw new ArgumentException("Provided message does not contain any select components.");
        }

        if (message.Components.SelectMany(c => c.Components).Where(IsSelect).All(c => c.CustomId != id))
        {
            throw new ArgumentException($"Provided message does not contain button with Id of '{id}'.");
        }

        ComponentInteractionCreatedEventArgs? result = await
            this.ComponentEventWaiter
            .WaitForMatchAsync(new(message, (c) => c.Id == id && c.User == user, token));

        return new(result is null, result);
    }

    /// <summary>
    /// Waits for a specific message.
    /// </summary>
    /// <param name="predicate">Predicate to match.</param>
    /// <param name="timeoutoverride">override timeout period.</param>
    /// <returns></returns>
    public async Task<InteractivityResult<DiscordMessage>> WaitForMessageAsync(Func<DiscordMessage, bool> predicate,
        TimeSpan? timeoutoverride = null)
    {
        if (!Utilities.HasMessageIntents(this.Client.Intents))
        {
            throw new InvalidOperationException("No message intents are enabled.");
        }

        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;
        MessageCreatedEventArgs? returns = await this.MessageCreatedWaiter.WaitForMatchAsync(new MatchRequest<MessageCreatedEventArgs>(x => predicate(x.Message), timeout));

        return new InteractivityResult<DiscordMessage>(returns == null, returns?.Message);
    }

    /// <summary>
    /// Wait for a specific reaction.
    /// </summary>
    /// <param name="predicate">Predicate to match.</param>
    /// <param name="timeoutoverride">override timeout period.</param>
    /// <returns></returns>
    public async Task<InteractivityResult<MessageReactionAddedEventArgs>> WaitForReactionAsync(Func<MessageReactionAddedEventArgs, bool> predicate,
        TimeSpan? timeoutoverride = null)
    {
        if (!Utilities.HasReactionIntents(this.Client.Intents))
        {
            throw new InvalidOperationException("No reaction intents are enabled.");
        }

        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;
        MessageReactionAddedEventArgs? returns = await this.MessageReactionAddWaiter.WaitForMatchAsync(new MatchRequest<MessageReactionAddedEventArgs>(predicate, timeout));

        return new InteractivityResult<MessageReactionAddedEventArgs>(returns == null, returns);
    }

    /// <summary>
    /// Wait for a specific reaction.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <see cref="BaseDiscordClient.Intents"/>
    /// </summary>
    /// <param name="message">Message reaction was added to.</param>
    /// <param name="user">User that made the reaction.</param>
    /// <param name="timeoutoverride">override timeout period.</param>
    /// <returns></returns>
    public async Task<InteractivityResult<MessageReactionAddedEventArgs>> WaitForReactionAsync(DiscordMessage message, DiscordUser user,
        TimeSpan? timeoutoverride = null)
        => await WaitForReactionAsync(x => x.User.Id == user.Id && x.Message.Id == message.Id, timeoutoverride);

    /// <summary>
    /// Waits for a specific reaction.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <see cref="BaseDiscordClient.Intents"/>
    /// </summary>
    /// <param name="predicate">Predicate to match.</param>
    /// <param name="message">Message reaction was added to.</param>
    /// <param name="user">User that made the reaction.</param>
    /// <param name="timeoutoverride">override timeout period.</param>
    /// <returns></returns>
    public async Task<InteractivityResult<MessageReactionAddedEventArgs>> WaitForReactionAsync(Func<MessageReactionAddedEventArgs, bool> predicate,
        DiscordMessage message, DiscordUser user, TimeSpan? timeoutoverride = null)
        => await WaitForReactionAsync(x => predicate(x) && x.User.Id == user.Id && x.Message.Id == message.Id, timeoutoverride);

    /// <summary>
    /// Waits for a specific reaction.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <see cref="BaseDiscordClient.Intents"/>
    /// </summary>
    /// <param name="predicate">predicate to match.</param>
    /// <param name="user">User that made the reaction.</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    /// <returns></returns>
    public async Task<InteractivityResult<MessageReactionAddedEventArgs>> WaitForReactionAsync(Func<MessageReactionAddedEventArgs, bool> predicate,
        DiscordUser user, TimeSpan? timeoutoverride = null)
        => await WaitForReactionAsync(x => predicate(x) && x.User.Id == user.Id, timeoutoverride);

    /// <summary>
    /// Waits for a user to start typing.
    /// </summary>
    /// <param name="user">User that starts typing.</param>
    /// <param name="channel">Channel the user is typing in.</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    /// <returns></returns>
    public async Task<InteractivityResult<TypingStartedEventArgs>> WaitForUserTypingAsync(DiscordUser user,
        DiscordChannel channel, TimeSpan? timeoutoverride = null)
    {
        if (!Utilities.HasTypingIntents(this.Client.Intents))
        {
            throw new InvalidOperationException("No typing intents are enabled.");
        }

        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;
        TypingStartedEventArgs? returns = await this.TypingStartWaiter.WaitForMatchAsync(
            new MatchRequest<TypingStartedEventArgs>(x => x.User.Id == user.Id && x.Channel.Id == channel.Id, timeout))
            ;

        return new InteractivityResult<TypingStartedEventArgs>(returns == null, returns);
    }

    /// <summary>
    /// Waits for a user to start typing.
    /// </summary>
    /// <param name="user">User that starts typing.</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    /// <returns></returns>
    public async Task<InteractivityResult<TypingStartedEventArgs>> WaitForUserTypingAsync(DiscordUser user, TimeSpan? timeoutoverride = null)
    {
        if (!Utilities.HasTypingIntents(this.Client.Intents))
        {
            throw new InvalidOperationException("No typing intents are enabled.");
        }

        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;
        TypingStartedEventArgs? returns = await this.TypingStartWaiter.WaitForMatchAsync(
            new MatchRequest<TypingStartedEventArgs>(x => x.User.Id == user.Id, timeout))
            ;

        return new InteractivityResult<TypingStartedEventArgs>(returns == null, returns);
    }

    /// <summary>
    /// Waits for any user to start typing.
    /// </summary>
    /// <param name="channel">Channel to type in.</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    /// <returns></returns>
    public async Task<InteractivityResult<TypingStartedEventArgs>> WaitForTypingAsync(DiscordChannel channel, TimeSpan? timeoutoverride = null)
    {
        if (!Utilities.HasTypingIntents(this.Client.Intents))
        {
            throw new InvalidOperationException("No typing intents are enabled.");
        }

        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;
        TypingStartedEventArgs? returns = await this.TypingStartWaiter.WaitForMatchAsync(
            new MatchRequest<TypingStartedEventArgs>(x => x.Channel.Id == channel.Id, timeout))
            ;

        return new InteractivityResult<TypingStartedEventArgs>(returns == null, returns);
    }

    /// <summary>
    /// Collects reactions on a specific message.
    /// </summary>
    /// <param name="m">Message to collect reactions on.</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    /// <returns></returns>
    public async Task<ReadOnlyCollection<Reaction>> CollectReactionsAsync(DiscordMessage m, TimeSpan? timeoutoverride = null)
    {
        if (!Utilities.HasReactionIntents(this.Client.Intents))
        {
            throw new InvalidOperationException("No reaction intents are enabled.");
        }

        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;
        ReadOnlyCollection<Reaction> collection = await this.ReactionCollector.CollectAsync(new ReactionCollectRequest(m, timeout));

        return collection;
    }

    /// <summary>
    /// Waits for specific event args to be received. Make sure the appropriate <see cref="DiscordIntents"/> are registered, if needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate"></param>
    /// <param name="timeoutoverride"></param>
    /// <returns></returns>
    public async Task<InteractivityResult<T>> WaitForEventArgsAsync<T>(Func<T, bool> predicate, TimeSpan? timeoutoverride = null) where T : AsyncEventArgs
    {
        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;

        using EventWaiter<T> waiter = new(this);
        T? res = await waiter.WaitForMatchAsync(new MatchRequest<T>(predicate, timeout));
        return new InteractivityResult<T>(res == null, res);
    }

    public async Task<ReadOnlyCollection<T>> CollectEventArgsAsync<T>(Func<T, bool> predicate, TimeSpan? timeoutoverride = null) where T : AsyncEventArgs
    {
        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;

        using EventWaiter<T> waiter = new(this);
        ReadOnlyCollection<T> res = await waiter.CollectMatchesAsync(new CollectRequest<T>(predicate, timeout));
        return res;
    }

    /// <summary>
    /// Sends a paginated message with buttons.
    /// </summary>
    /// <param name="channel">The channel to send it on.</param>
    /// <param name="user">User to give control.</param>
    /// <param name="pages">The pages.</param>
    /// <param name="buttons">Pagination buttons (pass null to use buttons defined in <see cref="InteractivityConfiguration"/>).</param>
    /// <param name="behaviour">Pagination behaviour.</param>
    /// <param name="deletion">Deletion behaviour</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    // Ideally this would take a [list of] builder(s), but there's complications with muddying APIs further than we already do.
    public async Task SendPaginatedMessageAsync(
        DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons,
        PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
    {
        PaginationBehaviour bhv = behaviour ?? this.Config.PaginationBehaviour;
        ButtonPaginationBehavior del = deletion ?? this.Config.ButtonBehavior;
        PaginationButtons bts = buttons ?? this.Config.PaginationButtons;

        bts = new(bts);

        Page[] pageArray = pages.ToArray();

        if (pageArray.Length == 1)
        {
            bts.SkipLeft.Disable();
            bts.Left.Disable();
            bts.Right.Disable();
            bts.SkipRight.Disable();
        }

        if (bhv is PaginationBehaviour.Ignore)
        {
            bts.SkipLeft.Disable();
            bts.Left.Disable();

            if (pageArray.Length == 2)
            {
                bts.SkipRight.Disable();
            }
        }

        DiscordMessageBuilder builder = new DiscordMessageBuilder()
            .WithContent(pageArray[0].Content)
            .AddEmbed(pageArray[0].Embed)
            .AddComponents(bts.ButtonArray);

        if (pageArray[0].Components is [..] pac)
        {
            builder.AddComponents(pac);
        }

        DiscordMessage message = await builder.SendAsync(channel);

        ButtonPaginationRequest req = new(message, user, bhv, del, bts, pageArray, token == default ? GetCancellationToken() : token);

        await this.compPaginator.DoPaginationAsync(req);
    }

    /// <summary>
    /// Sends a paginated message with buttons.
    /// </summary>
    /// <param name="channel">The channel to send it on.</param>
    /// <param name="user">User to give control.</param>
    /// <param name="pages">The pages.</param>
    /// <param name="buttons">Pagination buttons (pass null to use buttons defined in <see cref="InteractivityConfiguration"/>).</param>
    /// <param name="behaviour">Pagination behaviour.</param>
    /// <param name="deletion">Deletion behaviour</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    public Task SendPaginatedMessageAsync(
        DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons, TimeSpan? timeoutoverride,
        PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default)
        => SendPaginatedMessageAsync(channel, user, pages, buttons, behaviour, deletion, GetCancellationToken(timeoutoverride));

    /// <inheritdoc cref="SendPaginatedMessageAsync(DiscordChannel, DiscordUser, IEnumerable{Page}, PaginationButtons, PaginationBehaviour?, ButtonPaginationBehavior?, CancellationToken)"/>
    /// <remarks>This is the "default" overload for SendPaginatedMessageAsync, and will use buttons. Feel free to specify default(PaginationEmojis) to use reactions and emojis specified in <see cref="InteractivityConfiguration"/>, instead. </remarks>
    public Task SendPaginatedMessageAsync(DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
        => SendPaginatedMessageAsync(channel, user, pages, default, behaviour, deletion, token);

    /// <inheritdoc cref="SendPaginatedMessageAsync(DiscordChannel, DiscordUser, IEnumerable{Page}, PaginationButtons, TimeSpan?, PaginationBehaviour?, ButtonPaginationBehavior?)"/>
    /// <remarks>This is the "default" overload for SendPaginatedMessageAsync, and will use buttons. Feel free to specify default(PaginationEmojis) to use reactions and emojis specified in <see cref="InteractivityConfiguration"/>, instead. </remarks>
    public Task SendPaginatedMessageAsync(DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, TimeSpan? timeoutoverride, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default)
        => SendPaginatedMessageAsync(channel, user, pages, default, timeoutoverride, behaviour, deletion);

    /// <summary>
    /// Sends a paginated message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <see cref="BaseDiscordClient.Intents"/>
    /// </summary>
    /// <param name="channel">Channel to send paginated message in.</param>
    /// <param name="user">User to give control.</param>
    /// <param name="pages">Pages.</param>
    /// <param name="emojis">Pagination emojis.</param>
    /// <param name="behaviour">Pagination behaviour (when hitting max and min indices).</param>
    /// <param name="deletion">Deletion behaviour.</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    public async Task SendPaginatedMessageAsync(DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationEmojis emojis,
        PaginationBehaviour? behaviour = default, PaginationDeletion? deletion = default, TimeSpan? timeoutoverride = null)
    {
        DiscordMessageBuilder builder = new DiscordMessageBuilder()
            .WithContent(pages.First().Content)
            .AddEmbed(pages.First().Embed);
        DiscordMessage m = await builder.SendAsync(channel);

        TimeSpan timeout = timeoutoverride ?? this.Config.Timeout;

        PaginationBehaviour bhv = behaviour ?? this.Config.PaginationBehaviour;
        PaginationDeletion del = deletion ?? this.Config.PaginationDeletion;
        PaginationEmojis ems = emojis ?? this.Config.PaginationEmojis;

        PaginationRequest prequest = new(m, user, bhv, del, ems, timeout, pages.ToArray());

        await this.Paginator.DoPaginationAsync(prequest);
    }

    /// <summary>
    /// Sends a paginated message in response to an interaction.
    /// <para>
    /// <b>Pass the interaction directly. Interactivity will ACK it.</b>
    /// </para>
    /// </summary>
    /// <param name="interaction">The interaction to create a response to.</param>
    /// <param name="ephemeral">Whether the response should be ephemeral.</param>
    /// <param name="user">The user to listen for button presses from.</param>
    /// <param name="pages">The pages to paginate.</param>
    /// <param name="buttons">Optional: custom buttons</param>
    /// <param name="behaviour">Pagination behaviour.</param>
    /// <param name="deletion">Deletion behaviour</param>
    /// <param name="asEditResponse">If the response as edit of previous response.</param>
    /// <param name="disableBehavior">Whether to disable or remove the buttons if there is only one page</param>
    /// <param name="disabledButtons">Disabled buttons</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    public async Task SendPaginatedResponseAsync(DiscordInteraction interaction, bool ephemeral, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons = null, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, bool asEditResponse = false, ButtonDisableBehavior disableBehavior = ButtonDisableBehavior.Disable, List<PaginationButtonType> disabledButtons = null, CancellationToken token = default)
    {
        PaginationBehaviour bhv = behaviour ?? this.Config.PaginationBehaviour;
        ButtonPaginationBehavior del = deletion ?? this.Config.ButtonBehavior;
        PaginationButtons bts = buttons ?? this.Config.PaginationButtons;
        disabledButtons ??= [];

        bts = new(bts); // Copy //

        if (pages.Count() == 1)
        {
            if (disableBehavior == ButtonDisableBehavior.Disable)
            {
                bts.SkipLeft.Disable();
                bts.Left.Disable();
                bts.Right.Disable();
                bts.SkipRight.Disable();
            }
            else
            {
                disabledButtons
                    .AddRange(new[] { PaginationButtonType.Left, PaginationButtonType.Right, PaginationButtonType.SkipLeft, PaginationButtonType.SkipRight });
            }
        }

        if (bhv is PaginationBehaviour.Ignore)
        {
            if (disableBehavior == ButtonDisableBehavior.Disable)
            {
                bts.SkipLeft.Disable();
                bts.Left.Disable();
            }
            else
            {
                disabledButtons.AddRange(new[] { PaginationButtonType.SkipLeft, PaginationButtonType.Left });
            }

            if (pages.Count() == 2)
            {
                if (disableBehavior == ButtonDisableBehavior.Disable)
                {
                    bts.SkipRight.Disable();
                }
                else
                {
                    disabledButtons.AddRange(new[] { PaginationButtonType.SkipRight });
                }

            }

        }

        DiscordMessage message;
        DiscordButtonComponent[] buttonArray = bts.ButtonArray;
        if (disabledButtons.Count != 0)
        {
            List<DiscordButtonComponent> buttonList = [.. buttonArray];
            if (disabledButtons.Contains(PaginationButtonType.Left))
            {
                buttonList.Remove(bts.Left);
            }
            if (disabledButtons.Contains(PaginationButtonType.Right))
            {
                buttonList.Remove(bts.Right);
            }
            if (disabledButtons.Contains(PaginationButtonType.SkipLeft))
            {
                buttonList.Remove(bts.SkipLeft);
            }
            if (disabledButtons.Contains(PaginationButtonType.SkipRight))
            {
                buttonList.Remove(bts.SkipRight);
            }
            if (disabledButtons.Contains(PaginationButtonType.Stop))
            {
                buttonList.Remove(bts.Stop);
            }

            buttonArray = [.. buttonList];
        }

        Page[] pageArray = pages.ToArray();

        if (asEditResponse)
        {
            DiscordWebhookBuilder builder = new DiscordWebhookBuilder()
                .WithContent(pageArray[0].Content)
                .AddEmbed(pageArray[0].Embed)
                .AddComponents(buttonArray);

            if (pageArray[0].Components is [..] pageArrayComponents)
            {
                builder.AddComponents(pageArrayComponents);
            }

            message = await interaction.EditOriginalResponseAsync(builder);
        }
        else
        {
            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
                .WithContent(pageArray[0].Content)
                .AddEmbed(pageArray[0].Embed)
                .AsEphemeral(ephemeral)
                .AddComponents(buttonArray);

            if (pageArray[0].Components is [..] pageArrayComponents)
            {
                builder.AddComponents(pageArrayComponents);
            }

            await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, builder);
            message = await interaction.GetOriginalResponseAsync();
        }

        InteractionPaginationRequest req = new(interaction, message, user, bhv, del, bts, pages, token);

        await this.compPaginator.DoPaginationAsync(req);
    }

    /// <summary>
    /// Waits for a custom pagination request to finish.
    /// This does NOT handle removing emojis after finishing for you.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task WaitForCustomPaginationAsync(IPaginationRequest request) => await this.Paginator.DoPaginationAsync(request);

    /// <summary>
    /// Waits for custom button-based pagination request to finish.
    /// <br/>
    /// This does <i><b>not</b></i> invoke <see cref="IPaginationRequest.DoCleanupAsync"/>.
    /// </summary>
    /// <param name="request">The request to wait for.</param>
    public async Task WaitForCustomComponentPaginationAsync(IPaginationRequest request) => await this.compPaginator.DoPaginationAsync(request);

    /// <summary>
    /// Generates pages from a string, and puts them in message content.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="splittype">How to split input string.</param>
    /// <returns></returns>
    public static IEnumerable<Page> GeneratePagesInContent(string input, SplitType splittype = SplitType.Character)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("You must provide a string that is not null or empty!");
        }

        List<Page> result = [];
        List<string> split;

        switch (splittype)
        {
            default:
            case SplitType.Character:
                split = [.. SplitString(input, 500)];
                break;
            case SplitType.Line:
                string[] subsplit = input.Split('\n');

                split = [];
                string s = "";

                for (int i = 0; i < subsplit.Length; i++)
                {
                    s += subsplit[i];
                    if (i >= 15 && i % 15 == 0)
                    {
                        split.Add(s);
                        s = "";
                    }
                }
                if (s != "" && split.All(x => x != s))
                {
                    split.Add(s);
                }

                break;
        }

        int page = 1;
        foreach (string s in split)
        {
            result.Add(new Page($"Page {page}:\n{s}"));
            page++;
        }

        return result;
    }

    /// <summary>
    /// Generates pages from a string, and puts them in message embeds.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="splittype">How to split input string.</param>
    /// <param name="embedbase">Base embed for output embeds.</param>
    /// <returns></returns>
    public static IEnumerable<Page> GeneratePagesInEmbed(string input, SplitType splittype = SplitType.Character, DiscordEmbedBuilder embedbase = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("You must provide a string that is not null or empty!");
        }

        DiscordEmbedBuilder embed = embedbase ?? new DiscordEmbedBuilder();

        List<Page> result = [];
        List<string> split;

        switch (splittype)
        {
            default:
            case SplitType.Character:
                split = [.. SplitString(input, 500)];
                break;
            case SplitType.Line:
                string[] subsplit = input.Split('\n');

                split = [];
                string s = "";

                for (int i = 0; i < subsplit.Length; i++)
                {
                    s += $"{subsplit[i]}\n";
                    if (i % 15 == 0 && i != 0)
                    {
                        split.Add(s);
                        s = "";
                    }
                }
                if (s != "" && split.All(x => x != s))
                {
                    split.Add(s);
                }

                break;
        }

        int page = 1;
        foreach (string s in split)
        {
            result.Add(new Page("", new DiscordEmbedBuilder(embed).WithDescription(s).WithFooter($"Page {page}/{split.Count}")));
            page++;
        }

        return result;
    }

    private static List<string> SplitString(string str, int chunkSize)
    {
        List<string> res = [];
        int len = str.Length;
        int i = 0;

        while (i < len)
        {
            int size = Math.Min(len - i, chunkSize);
            res.Add(str.Substring(i, size));
            i += size;
        }

        return res;
    }

    private CancellationToken GetCancellationToken(TimeSpan? timeout = null) => new CancellationTokenSource(timeout ?? this.Config.Timeout).Token;

    public void Dispose()
    {
        this.ComponentEventWaiter?.Dispose();
        this.ModalEventWaiter?.Dispose();
        this.ReactionCollector?.Dispose();
        this.ComponentInteractionWaiter?.Dispose();
        this.MessageCreatedWaiter?.Dispose();
        this.MessageReactionAddWaiter?.Dispose();
        this.Paginator?.Dispose();
        this.Poller?.Dispose();
        this.TypingStartWaiter?.Dispose();
        this.compPaginator?.Dispose();

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
