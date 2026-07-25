#pragma warning disable IDE0040

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Components;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Interactivity;

static partial class MessageExtensions
{
    internal static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForComponentInteractionInternalAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        Func<ComponentInteractionCreatedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        TimeSpan timeout = timeoutOverride ?? message.Discord.GetDefaultInteractivityTimeout();
        IComponentWaiter componentWaiter = message.Discord.AsDiscordClient().ServiceProvider.GetRequiredService<IComponentWaiter>();

        return await componentWaiter.WaitForComponentInteractionAsync(message.Id, customIds, condition, timeout);
    }

    /// <summary>
    /// Waits for a button to be pressed on the specified message. Button presses not matching the specified custom IDs will be ignored, while
    /// button presses failing the provided condition will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="customIds">The custom IDs of buttons to consider.</param>
    /// <param name="condition">An additional condition that button presses have to match.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        Func<ComponentInteractionCreatedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        return await WaitForComponentInteractionInternalAsync
        (
            message,
            customIds,
            args =>
            {
                return args.Interaction.Data.ComponentType != DiscordComponentType.Button
                    ? throw new InvalidOperationException($"WaitForButtonAsync cannot wait for interactions of type {args.Interaction.Data.ComponentType}")
                    : condition(args);
            },
            timeoutOverride
        );
    }

    /// <summary>
    /// Waits for a button to be pressed on the specified message. Button presses not matching the specified custom ID will be ignored, while
    /// button presses failing the provided condition will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="customId">The custom ID to wait on.</param>
    /// <param name="condition">An additional condition that button presses have to match.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        string customId,
        Func<ComponentInteractionCreatedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForButtonAsync(message, [customId], condition, timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed on the specified message. Button presses not matching the specified custom IDs will be ignored, while
    /// button presses not from the requested users will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="customIds">The custom IDs of buttons to consider.</param>
    /// <param name="users">A list of users permitted to answer this button.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        IReadOnlyList<DiscordUser> users,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForButtonAsync(message, customIds, args => users.Any(x => x.Id == args.User.Id), timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed on the specified message. Button presses not matching the specified custom ID will be ignored, while
    /// button presses not from the requested users will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="customId">The custom ID to wait on.</param>
    /// <param name="users">A list of users permitted to answer this button.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        string customId,
        IReadOnlyList<DiscordUser> users,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForButtonAsync(message, [customId], args => users.Any(x => x.Id == args.User.Id), timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed on the specified message. Button presses not matching the specified custom IDs will be ignored, while
    /// button presses not from the requested users will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="customIds">The custom IDs of buttons to consider.</param>
    /// <param name="user">The user permitted to answer this button.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        DiscordUser user,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForButtonAsync(message, customIds, args => user.Id == args.User.Id, timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed on the specified message. Button presses not matching the specified custom ID will be ignored, while
    /// button presses not from the requested users will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="customId">The custom ID to wait on.</param>
    /// <param name="user">The user permitted to answer this button.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        string customId,
        DiscordUser user,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForButtonAsync(message, [customId], args => user.Id == args.User.Id, timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed on the specified message. Button presses not matching the specified custom IDs will be ignored. 
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="customIds">The custom IDs of buttons to consider.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForButtonAsync(message, customIds, _ => true, timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed on the specified message. Button presses not matching the specified custom ID will be ignored. 
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="customId">The custom ID to wait on.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        string customId,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForButtonAsync(message, [customId], _ => true, timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed matching the specified condition.
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="condition">The condition a button press needs to match to be considered successful.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        Func<ComponentInteractionCreatedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    )
        => await WaitForButtonAsync(message, ["*"], condition, timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed by one of the specified users on the specified message.
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="users">The users to accept button presses from.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync
    (
        this DiscordMessage message,
        IReadOnlyList<DiscordUser> users,
        TimeSpan? timeoutOverride = null
    )
        => await WaitForButtonAsync(message, ["*"], users, timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed by the specified user on the specified message.
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="user">The user to accept button presses from.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(this DiscordMessage message, DiscordUser user, TimeSpan? timeoutOverride = null)
        => await WaitForButtonAsync(message, ["*"], [user], timeoutOverride);

    /// <summary>
    /// Waits for a button to be pressed on the specified message.
    /// </summary>
    /// <param name="message">The message to wait for a button on.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned button interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForButtonAsync(this DiscordMessage message, TimeSpan? timeoutOverride = null)
        => await WaitForButtonAsync(message, ["*"], timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message. Select menus not matching the specified custom IDs will be ignored, while
    /// select menu interactions failing the provided condition will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a selection on.</param>
    /// <param name="customIds">The custom IDs of select menus to consider.</param>
    /// <param name="condition">An additional condition that select menu interactions have to match.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        Func<ComponentInteractionCreatedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        return await WaitForComponentInteractionInternalAsync
        (
            message,
            customIds,
            args =>
            {
                return IsSelect(args.Interaction.Data.ComponentType)
                    ? throw new InvalidOperationException($"WaitForSelectMenuAsync cannot wait for interactions of type {args.Interaction.Data.ComponentType}")
                    : condition(args);
            },
            timeoutOverride
        );

        static bool IsSelect(DiscordComponentType type)
        {
            return type is DiscordComponentType.StringSelect
                or DiscordComponentType.UserSelect
                or DiscordComponentType.RoleSelect
                or DiscordComponentType.MentionableSelect
                or DiscordComponentType.ChannelSelect;
        }
    }

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message. Select menus not matching the specified custom ID will be ignored, while
    /// select menu interactions failing the provided condition will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a selection on.</param>
    /// <param name="customId">The custom ID to wait on.</param>
    /// <param name="condition">An additional condition that select menu interactions have to match.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        string customId,
        Func<ComponentInteractionCreatedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForSelectMenuAsync(message, [customId], condition, timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message. Select menus not matching the specified custom IDs will be ignored, while
    /// select menu interactions not from the requested users will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a selection on.</param>
    /// <param name="customIds">The custom IDs of select menus to consider.</param>
    /// <param name="users">A list of users permitted to answer this select menu.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        IReadOnlyList<DiscordUser> users,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForSelectMenuAsync(message, customIds, args => users.Any(x => x.Id == args.User.Id), timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message. Select menus not matching the specified custom ID will be ignored, while
    /// select menu interactions not from the requested users will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a selection on.</param>
    /// <param name="customId">The custom ID to wait on.</param>
    /// <param name="users">A list of users permitted to answer this select menu.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        string customId,
        IReadOnlyList<DiscordUser> users,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForSelectMenuAsync(message, [customId], args => users.Any(x => x.Id == args.User.Id), timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message. Select menus not matching the specified custom IDs will be ignored, while
    /// select menu interactions not from the requested users will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a selection on.</param>
    /// <param name="customIds">The custom IDs of select menus to consider.</param>
    /// <param name="user">The user permitted to answer this select menu.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        DiscordUser user,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForSelectMenuAsync(message, customIds, args => user.Id == args.User.Id, timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message. Select menus not matching the specified custom IDs will be ignored, while
    /// select menu interactions not from the requested users will invoke the component reaction behaviour specified in <see cref="InteractivityOptions"/>. 
    /// </summary>
    /// <param name="message">The message to wait for a selection on.</param>
    /// <param name="customId">The custom ID to wait on.</param>
    /// <param name="user">The user permitted to answer this select menu.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        string customId,
        DiscordUser user,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForSelectMenuAsync(message, [customId], args => user.Id == args.User.Id, timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message. Select menus not matching the specified custom IDs will be ignored.
    /// </summary>
    /// <param name="message">The message to wait for a select menu on.</param>
    /// <param name="customIds">The custom IDs of select menus to consider.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        IReadOnlyList<string> customIds,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForSelectMenuAsync(message, customIds, _ => true, timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message. Select menus not matching the specified custom IDs will be ignored.
    /// </summary>
    /// <param name="message">The message to wait for a select menu on.</param>
    /// <param name="customId">The custom ID to wait on.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        string customId,
        TimeSpan? timeoutOverride = null
    ) 
        => await WaitForSelectMenuAsync(message, [customId], _ => true, timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with matching the specified condition.
    /// </summary>
    /// <param name="message">The message to wait for a select menu interaction on.</param>
    /// <param name="condition">The condition a select menu interaction needs to match to be considered successful.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        Func<ComponentInteractionCreatedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    )
        => await WaitForSelectMenuAsync(message, ["*"], condition, timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with by one of the specified users on the specified message.
    /// </summary>
    /// <param name="message">The message to wait for a select menu interaction on.</param>
    /// <param name="users">The users to accept select menu interactions from.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync
    (
        this DiscordMessage message,
        IReadOnlyList<DiscordUser> users,
        TimeSpan? timeoutOverride = null
    )
        => await WaitForSelectMenuAsync(message, ["*"], users, timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted wtih by the specified user on the specified message.
    /// </summary>
    /// <param name="message">The message to wait for a select menu interaction on.</param>
    /// <param name="user">The user to accept select menu interactions from.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync(this DiscordMessage message, DiscordUser user, TimeSpan? timeoutOverride = null)
        => await WaitForSelectMenuAsync(message, ["*"], [user], timeoutOverride);

    /// <summary>
    /// Waits for a select menu to be interacted with on the specified message.
    /// </summary>
    /// <param name="message">The message to wait for a select menu interaction on.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned select menu interaction or an error that might have occurred.</returns>
    public static async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForSelectMenuAsync(this DiscordMessage message, TimeSpan? timeoutOverride = null)
        => await WaitForSelectMenuAsync(message, ["*"], timeoutOverride);
}
