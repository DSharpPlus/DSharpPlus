using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class AttachmentConverter : ISlashArgumentConverter<DiscordAttachment>, ITextArgumentConverter<DiscordAttachment>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Attachment;
    public string ReadableName => "Discord File";
    public bool RequiresText => false;

    public Task<Optional<DiscordAttachment>> ConvertAsync(ConverterContext context)
    {
        IReadOnlyList<CommandParameter> attachmentParameters = context.Command.Parameters.Where(argument => argument.Type == typeof(DiscordAttachment)).ToList();
        int currentAttachmentArgumentIndex = attachmentParameters.IndexOf(context.Parameter);

        if (context is TextConverterContext textConverterContext)
        {
            foreach (Attribute attribute in textConverterContext.Parameter.Attributes)
            {
                if (attribute is not TextMessageReplyAttribute textMessageReplyAttribute)
                {
                    continue;
                }
                else if (textConverterContext.Message.ReferencedMessage is not null)
                {
                    // Search for the attachment index in the referenced message
                    // This will be separate from the current attachment index
                    // sent with the original message
                    currentAttachmentArgumentIndex = attachmentParameters
                        .Where(parameter => parameter.Attributes.Any(attribute => attribute is TextMessageReplyAttribute))
                        .IndexOf(context.Parameter);

                    // Double check that the message reply has the required attachments
                    if (textConverterContext.Message.ReferencedMessage.Attachments.Count <= currentAttachmentArgumentIndex)
                    {
                        return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
                    }

                    // Return the attachment from the referenced message
                    return Task.FromResult(Optional.FromValue(textConverterContext.Message.ReferencedMessage.Attachments[currentAttachmentArgumentIndex]));
                }
                else if (textMessageReplyAttribute.RequireReplies)
                {
                    // No referenced message, but the attribute requires it
                    return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
                }
            }

            // The parameter does not have a TextMessageReplyAttribute,
            // meaning we'll fetch the attachment from the original message
            // Check to see if the original message has enough attachments
            if (textConverterContext.Message.Attachments.Count > currentAttachmentArgumentIndex)
            {
                // Return the attachment from the original message
                return Task.FromResult(Optional.FromValue(textConverterContext.Message.Attachments[currentAttachmentArgumentIndex]));
            }
        }
        else if (context is InteractionConverterContext interactionConverterContext &&
            // Resolved can be null on autocomplete contexts
            interactionConverterContext.Interaction.Data.Resolved is not null
            // Check if we have enough attachments to fetch the current attachment
            && interactionConverterContext.Interaction.Data.Options.Count(argument => argument.Type == DiscordApplicationCommandOptionType.Attachment) >= currentAttachmentArgumentIndex
            // Check if we can parse the attachment ID (this should be guaranteed by Discord)
            && ulong.TryParse(interactionConverterContext.Argument?.RawValue, CultureInfo.InvariantCulture, out ulong attachmentId)
            // Check if the attachment exists
            && interactionConverterContext.Interaction.Data.Resolved.Attachments.TryGetValue(attachmentId, out DiscordAttachment? attachment))
        {
            return Task.FromResult(Optional.FromValue(attachment));
        }

        return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
    }
}
