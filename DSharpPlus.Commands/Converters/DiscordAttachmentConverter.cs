
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;
public class AttachmentConverter : ISlashArgumentConverter<DiscordAttachment>, ITextArgumentConverter<DiscordAttachment>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.Attachment;
    public bool RequiresText { get; init; }

    public Task<Optional<DiscordAttachment>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs)
    {
        DiscordMessage message = eventArgs.Message;
        int currentAttachmentArgumentIndex = context.Command.Parameters.Where(argument => argument.Type == typeof(DiscordAttachment)).IndexOf(context.Parameter);
        if (context.Parameter.Attributes.FirstOrDefault(x => x is TextMessageReplyAttribute) is TextMessageReplyAttribute textMessageReplyAttribute)
        {
            if (eventArgs.Message.ReferencedMessage is not null)
            {
                message = eventArgs.Message.ReferencedMessage;
            }
            else if (textMessageReplyAttribute.RequireReplies)
            {
                // No referenced message, but the attribute requires it
                return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
            }
        }

        return message.Attachments.Count <= currentAttachmentArgumentIndex
            ? Task.FromResult(Optional.FromNoValue<DiscordAttachment>())
            : Task.FromResult(Optional.FromValue(message.Attachments[currentAttachmentArgumentIndex]));
    }

    public Task<Optional<DiscordAttachment>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        int currentAttachmentArgumentIndex = context.Command.Parameters.Where(argument => argument.Type == typeof(DiscordAttachment)).IndexOf(context.Parameter);
        if (eventArgs.Interaction.Data.Resolved is null)
        {
            // Resolved can be null on autocomplete contexts
            return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
        }
        else if (eventArgs.Interaction.Data.Options.Count(argument => argument.Type == DiscordApplicationCommandOptionType.Attachment) < currentAttachmentArgumentIndex)
        {
            // Too many parameters, not enough attachments
            return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
        }
        else if (!ulong.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out ulong attachmentId))
        {
            // Invalid attachment ID
            return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
        }
        else if (!eventArgs.Interaction.Data.Resolved.Attachments.TryGetValue(attachmentId, out DiscordAttachment? attachment))
        {
            // Attachment not found
            return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
        }
        else
        {
            return Task.FromResult(Optional.FromValue(attachment));
        }
    }
}
