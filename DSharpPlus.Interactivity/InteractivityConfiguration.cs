using System;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

namespace DSharpPlus.Interactivity;

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
    /// How to handle invalid [component] interactions. Defaults to <see cref="InteractionResponseBehavior.Ignore"/>
    /// </summary>
    public InteractionResponseBehavior ResponseBehavior { internal get; set; } = InteractionResponseBehavior.Ignore;

    /// <summary>
    /// Provides a string factory to generate a response when processing invalid interactions. This is ignored if <see cref="ResponseBehavior"/> is not <see cref="InteractionResponseBehavior.Respond"/>
    /// </summary>
    /// <remarks>
    /// An invalid interaction in this case is considered as an interaction on a component where the invoking user does not match the specified user to wait for.
    /// </remarks>
    public Func<ComponentInteractionCreatedEventArgs, IServiceProvider, string> ResponseMessageFactory
    {
        internal get;
        set;
    } = (_, _) => "This message is not meant for you!";
    
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
        this.PaginationButtons = other.PaginationButtons;
        this.ButtonBehavior = other.ButtonBehavior;
        this.PaginationBehaviour = other.PaginationBehaviour;
        this.PaginationDeletion = other.PaginationDeletion;
        this.ResponseBehavior = other.ResponseBehavior;
        this.PaginationEmojis = other.PaginationEmojis;
        this.ResponseMessageFactory = other.ResponseMessageFactory;
        this.ResponseMessage = other.ResponseMessage;
        this.PollBehaviour = other.PollBehaviour;
        this.Timeout = other.Timeout;

        if (this.ResponseBehavior is InteractionResponseBehavior.Respond && string.IsNullOrWhiteSpace(this.ResponseMessage))
        {
            throw new ArgumentException($"{nameof(this.ResponseMessage)} cannot be null, empty, or whitespace when {nameof(this.ResponseBehavior)} is set to respond.");
        }
    }
}
