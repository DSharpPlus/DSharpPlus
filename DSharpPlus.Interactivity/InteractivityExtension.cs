// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using Emzi0767.Utilities;

namespace DSharpPlus.Interactivity
{
    /// <summary>
    /// Extension class for DSharpPlus.Interactivity
    /// </summary>
    public class InteractivityExtension : BaseExtension
    {

        #pragma warning disable IDE1006 // Naming Styles
        internal InteractivityConfiguration Config { get; }

        private EventWaiter<MessageCreateEventArgs> MessageCreatedWaiter;

        private EventWaiter<MessageReactionAddEventArgs> MessageReactionAddWaiter;

        private EventWaiter<TypingStartEventArgs> TypingStartWaiter;

        private EventWaiter<ComponentInteractionCreateEventArgs> ComponentInteractionWaiter;

        private ComponentEventWaiter ComponentEventWaiter;

        private ReactionCollector ReactionCollector;

        private Poller Poller;

        private Paginator Paginator;
        private  ComponentPaginator _compPaginator;

        #pragma warning restore IDE1006 // Naming Styles

        internal InteractivityExtension(InteractivityConfiguration cfg)
        {
            this.Config = new InteractivityConfiguration(cfg);
        }

        protected internal override void Setup(DiscordClient client)
        {
            this.Client = client;
            this.MessageCreatedWaiter = new EventWaiter<MessageCreateEventArgs>(this.Client);
            this.MessageReactionAddWaiter = new EventWaiter<MessageReactionAddEventArgs>(this.Client);
            this.ComponentInteractionWaiter = new EventWaiter<ComponentInteractionCreateEventArgs>(this.Client);
            this.TypingStartWaiter = new EventWaiter<TypingStartEventArgs>(this.Client);
            this.Poller = new Poller(this.Client);
            this.ReactionCollector = new ReactionCollector(this.Client);
            this.Paginator = new Paginator(this.Client);
            this._compPaginator = new(this.Client, this.Config);
            this.ComponentEventWaiter = new(this.Client, this.Config);

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
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            if (!emojis.Any())
                throw new ArgumentException("You need to provide at least one emoji for a poll!");

            foreach (var em in emojis)
                await m.CreateReactionAsync(em).ConfigureAwait(false);

            var res = await this.Poller.DoPollAsync(new PollRequest(m, timeout ?? this.Config.Timeout, emojis)).ConfigureAwait(false);

            var pollbehaviour = behaviour ?? this.Config.PollBehaviour;
            var thismember = await m.Channel.Guild.GetMemberAsync(this.Client.CurrentUser.Id).ConfigureAwait(false);

            if (pollbehaviour == PollBehaviour.DeleteEmojis && m.Channel.PermissionsFor(thismember).HasPermission(Permissions.ManageMessages))
                await m.DeleteAllReactionsAsync().ConfigureAwait(false);

            return new ReadOnlyCollection<PollEmoji>(res.ToList());
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
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, IEnumerable<DiscordButtonComponent> buttons, TimeSpan? timeoutOverride = null)
            => this.WaitForButtonAsync(message, buttons, this.GetCancellationToken(timeoutOverride));

        /// <summary>
        /// Waits for any button in the specified collection to be pressed.
        /// </summary>
        /// <param name="message">The message to wait on.</param>
        /// <param name="buttons">A collection of buttons to listen for.</param>
        /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, IEnumerable<DiscordButtonComponent> buttons, CancellationToken token = default)
        {
            if (message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (message.Components is null || !message.Components.Select(a => a.Components).Any())
                throw new ArgumentException("Message does not contain any buttons.");

            if (!buttons.Any())
                throw new ArgumentException("You must specify at least one button to listen for.");

            var res = await this.ComponentEventWaiter
                .WaitForMatchAsync(new(message.Id,
                    c =>
                        c.Interaction.Data.ComponentType == ComponentType.Button &&
                        buttons.Any(b => b.CustomId == c.Id), token));

            return new(res is null, res);
        }

        /// <summary>
        /// Waits for any button on the specified message to be pressed.
        /// </summary>
        /// <param name="message">The message to wait for the button on.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message)
            => this.WaitForButtonAsync(message);

        /// <summary>
        /// Waits for any button on the specified message to be pressed.
        /// </summary>
        /// <param name="message">The message to wait for the button on.</param>
        /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, TimeSpan? timeoutOverride = null)
            => this.WaitForButtonAsync(message, this.GetCancellationToken(timeoutOverride));

        /// <summary>
        /// Waits for any button on the specified message to be pressed.
        /// </summary>
        /// <param name="message">The message to wait for the button on.</param>
        /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, CancellationToken token = default)
        {
            if (message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (message.Components is null || !message.Components.Select(a => a.Components).Any())
                throw new ArgumentException("Message does not contain any buttons.");

            var ids = message.Components.SelectMany(m => m.Components).Select(c => c.CustomId);



            var result =
                await this
                .ComponentEventWaiter
                .WaitForMatchAsync(new(message.Id, c => c.Interaction.Data.ComponentType == ComponentType.Button, token))
                .ConfigureAwait(false);

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
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, DiscordUser user, TimeSpan? timeoutOverride = null)
            => this.WaitForButtonAsync(message, user, this.GetCancellationToken(timeoutOverride));

        /// <summary>
        /// Waits for any button on the specified message to be pressed by the specified user.
        /// </summary>
        /// <param name="message">The message to wait for the button on.</param>
        /// <param name="user">The user to wait for the button press from.</param>
        /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, DiscordUser user, CancellationToken token = default)
        {
            if (message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (message.Components is null || !message.Components.Select(a => a.Components).Any())
                throw new ArgumentException("Message does not contain any buttons.");



            var result = await this
                .ComponentEventWaiter
                .WaitForMatchAsync(new(message.Id, (c) => c.Interaction.Data.ComponentType is ComponentType.Button && c.User == user, token))
                .ConfigureAwait(false);

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
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, string id, TimeSpan? timeoutOverride = null)
            => this.WaitForButtonAsync(message, id, this.GetCancellationToken(timeoutOverride));

        /// <summary>
        /// Waits for a button with the specified Id to be pressed.
        /// </summary>
        /// <param name="message">The message to wait for the button on.</param>
        /// <param name="id">The Id of the button to wait for.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of the operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, string id, CancellationToken token = default)
        {
            if (message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (message.Components is null || !message.Components.Select(a => a.Components).Any())
                throw new ArgumentException("Message does not contain any buttons.");

            if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is ComponentType.Button && c.CustomId == id))
                throw new ArgumentException($"Message does not contain button with Id of '{id}'.");



            var result = await this
                .ComponentEventWaiter
                .WaitForMatchAsync(new(message.Id, (c) => c.Interaction.Data.ComponentType is ComponentType.Button && c.Id == id, token))
                .ConfigureAwait(false);

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
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, string id, TimeSpan? timeoutOverride = null)
            => this.WaitForSelectAsync(message, id, this.GetCancellationToken(timeoutOverride));

        /// <summary>
        /// Waits for a dropdown to be interacted with.
        /// </summary>
        /// <param name="message">The message to wait on.</param>
        /// <param name="id">The Id of the dropdown to wait on.</param>
        /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
        /// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, string id, CancellationToken token = default)
        {
            if (!message.Components.Any())
                throw new ArgumentException("Message doesn't contain any components!");

            var scmps = message.Components.SelectMany(c => c.Components).Where(c => c.Type is ComponentType.Select).ToList();

            if (!scmps.Any())
                throw new ArgumentException("Message doesn't contain any select menus!");

            if (scmps.All(c => c.CustomId != id))
                throw new ArgumentException("Message doesn't contain a select menu with that Id!");

            var result = await this
                .ComponentEventWaiter
                .WaitForMatchAsync(new(message.Id, (c) => c.Interaction.Data.ComponentType is ComponentType.Select && c.Id == id, token))
                .ConfigureAwait(false);

            return new(result is null, result);
        }

        /// <summary>
        /// Waits for a dropdown to be interacted with by a specific user.
        /// </summary>
        /// <param name="message">The message to wait on.</param>
        /// <param name="user">The user to wait on.</param>
        /// <param name="id">The Id of the dropdown to wait on.</param>
        /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, DiscordUser user, string id, TimeSpan? timeoutOverride = null)
            => this.WaitForSelectAsync(message, user, id, this.GetCancellationToken(timeoutOverride));

        /// <summary>
        /// Waits for a dropdown to be interacted with by a specific user.
        /// </summary>
        /// <param name="message">The message to wait on.</param>
        /// <param name="user">The user to wait on.</param>
        /// <param name="id">The Id of the dropdown to wait on.</param>
        /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
        /// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, DiscordUser user, string id, CancellationToken token = default)
        {
            if (!message.Components.Any())
                throw new ArgumentException("Message doesn't contain any components!");

            var scmps = message.Components.SelectMany(c => c.Components).Where(c => c.Type is ComponentType.Select).ToList();

            if (!scmps.Any())
                throw new ArgumentException("Message doesn't contain any select menus!");

            if (scmps.All(c => c.CustomId != id))
                throw new ArgumentException("Message doesn't contain a select menu with that Id!");

            var result = await this
                .ComponentEventWaiter
                .WaitForMatchAsync(new(message.Id, (c) => c.Id == id && c.User == user, token)).ConfigureAwait(false);

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
            if (!Utilities.HasMessageIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No message intents are enabled.");

            var timeout = timeoutoverride ?? this.Config.Timeout;
            var returns = await this.MessageCreatedWaiter.WaitForMatch(new MatchRequest<MessageCreateEventArgs>(x => predicate(x.Message), timeout)).ConfigureAwait(false);

            return new InteractivityResult<DiscordMessage>(returns == null, returns?.Message);
        }

        /// <summary>
        /// Wait for a specific reaction.
        /// </summary>
        /// <param name="predicate">Predicate to match.</param>
        /// <param name="timeoutoverride">override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> predicate,
            TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            var timeout = timeoutoverride ?? this.Config.Timeout;
            var returns = await this.MessageReactionAddWaiter.WaitForMatch(new MatchRequest<MessageReactionAddEventArgs>(predicate, timeout)).ConfigureAwait(false);

            return new InteractivityResult<MessageReactionAddEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Wait for a specific reaction.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <param name="message">Message reaction was added to.</param>
        /// <param name="user">User that made the reaction.</param>
        /// <param name="timeoutoverride">override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(DiscordMessage message, DiscordUser user,
            TimeSpan? timeoutoverride = null)
            => await this.WaitForReactionAsync(x => x.User.Id == user.Id && x.Message.Id == message.Id, timeoutoverride).ConfigureAwait(false);

        /// <summary>
        /// Waits for a specific reaction.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <param name="predicate">Predicate to match.</param>
        /// <param name="message">Message reaction was added to.</param>
        /// <param name="user">User that made the reaction.</param>
        /// <param name="timeoutoverride">override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> predicate,
            DiscordMessage message, DiscordUser user, TimeSpan? timeoutoverride = null)
            => await this.WaitForReactionAsync(x => predicate(x) && x.User.Id == user.Id && x.Message.Id == message.Id, timeoutoverride).ConfigureAwait(false);

        /// <summary>
        /// Waits for a specific reaction.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <param name="predicate">predicate to match.</param>
        /// <param name="user">User that made the reaction.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> predicate,
            DiscordUser user, TimeSpan? timeoutoverride = null)
            => await this.WaitForReactionAsync(x => predicate(x) && x.User.Id == user.Id, timeoutoverride).ConfigureAwait(false);

        /// <summary>
        /// Waits for a user to start typing.
        /// </summary>
        /// <param name="user">User that starts typing.</param>
        /// <param name="channel">Channel the user is typing in.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(DiscordUser user,
            DiscordChannel channel, TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = timeoutoverride ?? this.Config.Timeout;
            var returns = await this.TypingStartWaiter.WaitForMatch(
                new MatchRequest<TypingStartEventArgs>(x => x.User.Id == user.Id && x.Channel.Id == channel.Id, timeout))
                .ConfigureAwait(false);

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Waits for a user to start typing.
        /// </summary>
        /// <param name="user">User that starts typing.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(DiscordUser user, TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = timeoutoverride ?? this.Config.Timeout;
            var returns = await this.TypingStartWaiter.WaitForMatch(
                new MatchRequest<TypingStartEventArgs>(x => x.User.Id == user.Id, timeout))
                .ConfigureAwait(false);

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Waits for any user to start typing.
        /// </summary>
        /// <param name="channel">Channel to type in.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForTypingAsync(DiscordChannel channel, TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = timeoutoverride ?? this.Config.Timeout;
            var returns = await this.TypingStartWaiter.WaitForMatch(
                new MatchRequest<TypingStartEventArgs>(x => x.Channel.Id == channel.Id, timeout))
                .ConfigureAwait(false);

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Collects reactions on a specific message.
        /// </summary>
        /// <param name="m">Message to collect reactions on.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<ReadOnlyCollection<Reaction>> CollectReactionsAsync(DiscordMessage m, TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            var timeout = timeoutoverride ?? this.Config.Timeout;
            var collection = await this.ReactionCollector.CollectAsync(new ReactionCollectRequest(m, timeout)).ConfigureAwait(false);

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
            var timeout = timeoutoverride ?? this.Config.Timeout;

            using var waiter = new EventWaiter<T>(this.Client);
            var res = await waiter.WaitForMatch(new MatchRequest<T>(predicate, timeout)).ConfigureAwait(false);
            return new InteractivityResult<T>(res == null, res);
        }

        public async Task<ReadOnlyCollection<T>> CollectEventArgsAsync<T>(Func<T, bool> predicate, TimeSpan? timeoutoverride = null) where T : AsyncEventArgs
        {
            var timeout = timeoutoverride ?? this.Config.Timeout;

            using var waiter = new EventWaiter<T>(this.Client);
            var res = await waiter.CollectMatches(new CollectRequest<T>(predicate, timeout)).ConfigureAwait(false);
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
        public async Task SendPaginatedMessageAsync(
            DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons,
            PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
        {
            var bhv = behaviour ?? this.Config.PaginationBehaviour;
            var del = deletion ?? this.Config.ButtonBehavior;
            var bts = buttons ?? this.Config.PaginationButtons;

            var builder = new DiscordMessageBuilder()
                .WithContent(pages.First().Content)
                .WithEmbed(pages.First().Embed);

            var message = await builder.SendAsync(channel).ConfigureAwait(false);

            var req = new ButtonPaginationRequest(message, user, bhv, del, bts, pages.ToArray(), token);

            await builder // We *COULD* just construct a req with a null message and grab the buttons from that, but eh. //
                .AddComponents(await req.GetButtonsAsync().ConfigureAwait(false))
                .ModifyAsync(message).ConfigureAwait(false);

            await this._compPaginator.DoPaginationAsync(req).ConfigureAwait(false);
        }

        /// <inheritdoc cref="SendPaginatedMessageAsync(DSharpPlus.Entities.DiscordChannel,DSharpPlus.Entities.DiscordUser,System.Collections.Generic.IEnumerable{DSharpPlus.Interactivity.Page},DSharpPlus.Interactivity.EventHandling.PaginationButtons,System.Nullable{DSharpPlus.Interactivity.Enums.PaginationBehaviour},System.Nullable{DSharpPlus.Interactivity.Enums.ButtonPaginationBehavior},System.Threading.CancellationToken)"/>
        /// <remarks>This is the "default" overload for SendPaginatedMessageAsync, and will use buttons. Feel free to specify default(PaginationEmojis) to use reactions and emojis specified in <see cref="InteractivityConfiguration"/>, instead. </remarks>
        public async Task SendPaginatedMessageAsync(DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
            => this.SendPaginatedMessageAsync(channel, user, pages, default, behaviour, deletion, token);


        /// <summary>
        /// Sends a paginated message.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
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
            var builder = new DiscordMessageBuilder()
                .WithContent(pages.First().Content)
                .WithEmbed(pages.First().Embed);
            var m = await builder.SendAsync(channel).ConfigureAwait(false);

            var timeout = timeoutoverride ?? this.Config.Timeout;

            var bhv = behaviour ?? this.Config.PaginationBehaviour;
            var del = deletion ?? this.Config.PaginationDeletion;
            var ems = emojis ?? this.Config.PaginationEmojis;

            var prequest = new PaginationRequest(m, user, bhv, del, ems, timeout, pages.ToArray());

            await this.Paginator.DoPaginationAsync(prequest).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for a custom pagination request to finish.
        /// This does NOT handle removing emojis after finishing for you.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task WaitForCustomPaginationAsync(IPaginationRequest request) => await this.Paginator.DoPaginationAsync(request).ConfigureAwait(false);

        /// <summary>
        /// Waits for custom button-based pagination request to finish.
        /// <br/>
        /// This does <i><b>not</b></i> invoke <see cref="IPaginationRequest.DoCleanupAsync"/>.
        /// </summary>
        /// <param name="request">The request to wait for.</param>
        public async Task WaitForCustomComponentPaginationAsync(IPaginationRequest request) => await this._compPaginator.DoPaginationAsync(request).ConfigureAwait(false);

        /// <summary>
        /// Generates pages from a string, and puts them in message content.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="splittype">How to split input string.</param>
        /// <returns></returns>
        public IEnumerable<Page> GeneratePagesInContent(string input, SplitType splittype = SplitType.Character)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("You must provide a string that is not null or empty!");

            var result = new List<Page>();
            List<string> split;

            switch (splittype)
            {
                default:
                case SplitType.Character:
                    split = this.SplitString(input, 500).ToList();
                    break;
                case SplitType.Line:
                    var subsplit = input.Split('\n');

                    split = new List<string>();
                    var s = "";

                    for (var i = 0; i < subsplit.Length; i++)
                    {
                        s += subsplit[i];
                        if (i >= 15 && i % 15 == 0)
                        {
                            split.Add(s);
                            s = "";
                        }
                    }
                    if (split.All(x => x != s))
                        split.Add(s);
                    break;
            }

            var page = 1;
            foreach (var s in split)
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
        public IEnumerable<Page> GeneratePagesInEmbed(string input, SplitType splittype = SplitType.Character, DiscordEmbedBuilder embedbase = null)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("You must provide a string that is not null or empty!");

            var embed = embedbase ?? new DiscordEmbedBuilder();

            var result = new List<Page>();
            List<string> split;

            switch (splittype)
            {
                default:
                case SplitType.Character:
                    split = this.SplitString(input, 500).ToList();
                    break;
                case SplitType.Line:
                    var subsplit = input.Split('\n');

                    split = new List<string>();
                    var s = "";

                    for (var i = 0; i < subsplit.Length; i++)
                    {
                        s += $"{subsplit[i]}\n";
                        if (i % 15 == 0 && i != 0)
                        {
                            split.Add(s);
                            s = "";
                        }
                    }
                    if (!split.Any(x => x == s))
                        split.Add(s);
                    break;
            }

            var page = 1;
            foreach (var s in split)
            {
                result.Add(new Page("", new DiscordEmbedBuilder(embed).WithDescription(s).WithFooter($"Page {page}/{split.Count}")));
                page++;
            }

            return result;
        }

        private List<string> SplitString(string str, int chunkSize)
        {
            var res = new List<string>();
            var len = str.Length;
            var i = 0;

            while (i < len)
            {
                var size = Math.Min(len - i, chunkSize);
                res.Add(str.Substring(i, size));
                i += size;
            }

            return res;
        }

        private CancellationToken GetCancellationToken(TimeSpan? timeout) => new CancellationTokenSource(timeout ?? this.Config.Timeout).Token;

        private async Task HandleInvalidInteraction(DiscordInteraction interaction)
        {
            var at = this.Config.ResponseBehavior switch
            {
                InteractionResponseBehavior.Ack => interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate),
                InteractionResponseBehavior.Respond => interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = this.Config.ResponseMessage, IsEphemeral = true}),
                InteractionResponseBehavior.Ignore => Task.CompletedTask,
                _ => throw new ArgumentException("Unknown enum value.")
            };

            await at;
        }
    }
}
