using System.Diagnostics;
using System.Linq;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.TextCommands;

public record TextConverterContext : ConverterContext
{
    public required string RawArguments { get => this.rawArguments; init => this.rawArguments = value; }
    public required DiscordMessage Message { get => this.message; init => this.message = value; }
    public required TextArgumentSplicer Splicer { get; init; }
    public required int PrefixLength { internal get; init; }
    public string? Prefix => this.Message.Content?[..this.PrefixLength];
    public new string Argument => base.Argument as string ?? string.Empty;
    public int CurrentArgumentIndex { get; internal set; }
    public int NextArgumentIndex { get; internal set; }
    public bool IsOnMessageReply { get; private set; }

    // We don't use an auto-property here because we
    // want a public init and private set at the same time.
    private string rawArguments = null!;
    private DiscordMessage message = null!;
    private TextMessageReplyAttribute? replyAttribute;
    private TextConverterContext? replyConverterContext;

    public override bool NextParameter()
    {
        // If there's not another parameter, don't try to
        // resolve require reply attribute logic.
        if (!base.NextParameter())
        {
            return false;
        }

        // If the parameter wants a reply, switch to it.
        this.replyAttribute = this.Parameter.Attributes.OfType<TextMessageReplyAttribute>().FirstOrDefault();
        if (this.replyAttribute is not null && this.IsOnMessageReply)
        {
            return false;
        }

        SwitchToReply(this.IsOnMessageReply);
        return true;
    }

    public override bool NextArgument()
    {
        if (this.replyAttribute is not null && this.CurrentArgumentIndex == -1)
        {
            // If the argument converter does not require text, TextCommandProcessor.ExecuteConverter<T>
            // will set the argument index to -1 and change it back to the previous index after the conversion.
            // However if this is called twice, that means the argument converter required text only if the reply doesn't exist.
            // In that case, we should skip the reply and move to the next argument.
            this.CurrentArgumentIndex = 0;
            return true;
        }
        else if (this.NextArgumentIndex >= this.RawArguments.Length || this.NextArgumentIndex == -1)
        {
            return false;
        }

        this.CurrentArgumentIndex = this.NextArgumentIndex;
        int nextTextIndex = this.NextArgumentIndex;
        string? nextText = this.Splicer(this.Extension, this.RawArguments, ref nextTextIndex);
        if (string.IsNullOrEmpty(nextText))
        {
            base.Argument = string.Empty;
            return false;
        }

        this.NextArgumentIndex = nextTextIndex;
        base.Argument = nextText;
        return true;
    }

    /// <summary>
    /// Whether to switch to the original message or the reply that message references.
    /// </summary>
    /// <param name="value">Whether to switch to the original message from the reply.</param>
    public void SwitchToReply(bool value)
    {
        // If the value is the same as the current state, don't do anything
        if (this.IsOnMessageReply == value)
        {
            return;
        }

        // If we're not on the reply and we need to switch to the reply,
        // copy this context's state to the reply context.
        if (value && !this.IsOnMessageReply)
        {
            // If the reply context is null, create a new one
            if (this.replyConverterContext is null)
            {
                // Copy this context to the reply context
                this.replyConverterContext = this with
                {
                    CurrentArgumentIndex = this.CurrentArgumentIndex,
                    NextArgumentIndex = this.NextArgumentIndex,
                };
            }
            else
            {
                // Copy the state of the reply context to the current context
                this.replyConverterContext.CurrentArgumentIndex = this.CurrentArgumentIndex;
                this.replyConverterContext.NextArgumentIndex = this.NextArgumentIndex;
            }

            // Set this context to the reply's properties
            this.message = this.message.ReferencedMessage!;
            this.rawArguments = this.message?.Content!;
            this.CurrentArgumentIndex = 0;
            this.NextArgumentIndex = 0;

            this.IsOnMessageReply = value;
            return;
        }

        // If we're on the reply and we need to switch to the original message,
        // copy the other context's state to this context.
        if (this.replyConverterContext is null)
        {
            throw new UnreachableException("The reply context should not be null when switching to the original message.");
        }

        // We're no longer on a reply parameter, so we need to switch back to the original context
        int currentArgumentIndex = this.CurrentArgumentIndex;
        int nextArgumentIndex = this.NextArgumentIndex;

        this.message = this.replyConverterContext.Message;
        this.rawArguments = this.replyConverterContext.RawArguments;
        this.CurrentArgumentIndex = this.replyConverterContext.CurrentArgumentIndex;
        this.NextArgumentIndex = this.replyConverterContext.NextArgumentIndex;

        // Set the state in the reply context
        this.replyConverterContext.CurrentArgumentIndex = currentArgumentIndex;
        this.replyConverterContext.NextArgumentIndex = nextArgumentIndex;
        this.IsOnMessageReply = value;
    }
}
