namespace DSharpPlus.Interactivity;
using System;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// Configuration class for your Interactivity extension
/// </summary>
public sealed class InteractivityConfiguration
{
    /// <summary>
    /// <para>Sets the default interactivity action timeout.</para>
    /// <para>Defaults to 1 minute.</para>
    /// </summary>
    public TimeSpan Timeout { internal get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// What to do after the poll ends
    /// </summary>
    public PollBehaviour PollBehaviour { internal get; set; } = PollBehaviour.DeleteEmojis;

    /// <summary>
    /// Emojis to use for pagination
    /// </summary>
    public PaginationEmojis PaginationEmojis { internal get; set; } = new();

    /// <summary>
    /// Buttons to use for pagination.
    /// </summary>
    public PaginationButtons PaginationButtons { internal get; set; } = new();

    /// <summary>
    /// How to handle buttons after pagination ends.
    /// </summary>
    public ButtonPaginationBehavior ButtonBehavior { internal get; set; } = new();

    /// <summary>
    /// How to handle pagination. Defaults to WrapAround.
    /// </summary>
    public PaginationBehaviour PaginationBehaviour { internal get; set; } = PaginationBehaviour.WrapAround;

    /// <summary>
    /// How to handle pagination deletion. Defaults to DeleteEmojis.
    /// </summary>
    public PaginationDeletion PaginationDeletion { internal get; set; } = PaginationDeletion.DeleteEmojis;

    /// <summary>
    /// How to handle invalid interactions. Defaults to Ignore.
    /// </summary>
    public InteractionResponseBehavior ResponseBehavior { internal get; set; } = InteractionResponseBehavior.Ignore;

    /// <summary>
    /// The message to send to the user when processing invalid interactions. Ignored if <see cref="ResponseBehavior"/> is not set to <see cref="InteractionResponseBehavior.Respond"/>.
    /// </summary>
    public string ResponseMessage { internal get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="InteractivityConfiguration"/>.
    /// </summary>
    public InteractivityConfiguration()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="InteractivityConfiguration"/>, copying the properties of another configuration.
    /// </summary>
    /// <param name="other">Configuration the properties of which are to be copied.</param>
    public InteractivityConfiguration(InteractivityConfiguration other)
    {
        PaginationButtons = other.PaginationButtons;
        ButtonBehavior = other.ButtonBehavior;
        PaginationBehaviour = other.PaginationBehaviour;
        PaginationDeletion = other.PaginationDeletion;
        ResponseBehavior = other.ResponseBehavior;
        PaginationEmojis = other.PaginationEmojis;
        ResponseMessage = other.ResponseMessage;
        PollBehaviour = other.PollBehaviour;
        Timeout = other.Timeout;

        if (ResponseBehavior is InteractionResponseBehavior.Respond && string.IsNullOrWhiteSpace(ResponseMessage))
        {
            throw new ArgumentException($"{nameof(ResponseMessage)} cannot be null, empty, or whitespace when {nameof(ResponseBehavior)} is set to respond.");
        }
    }
}
