using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class AttachmentConverter
    : ISlashArgumentConverter<DiscordAttachment>,
        ITextArgumentConverter<DiscordAttachment>
{
    public DiscordApplicationCommandOptionType ParameterType =>
        DiscordApplicationCommandOptionType.Attachment;
    public string ReadableName => "Discord File";
    public bool RequiresText => false;

    public Task<Optional<DiscordAttachment>> ConvertAsync(ConverterContext context)
    {
        IReadOnlyList<CommandParameter> attachmentParameters = context
            .Command.Parameters.Where(argument => argument.Type == typeof(DiscordAttachment))
            .ToList();

        int currentAttachmentArgumentIndex = attachmentParameters.IndexOf(context.Parameter);
        if (context is TextConverterContext textConverterContext)
        {
            foreach (CommandParameter attachmentParameter in attachmentParameters)
            {
                // Don't increase past the current attachment parameter
                if (attachmentParameter == context.Parameter)
                {
                    break;
                }
                else if (
                    attachmentParameter.Attributes.FirstOrDefault(attribute =>
                        attribute is MultiArgumentAttribute
                    )
                    is MultiArgumentAttribute multiArgumentAttribute
                )
                {
                    // Increase the index by however many attachments we've already parsed
                    // We add by maximum argument count because the attachment converter will never fail to parse
                    // the attachment when it's present.
                    currentAttachmentArgumentIndex = multiArgumentAttribute.MaximumArgumentCount;
                }
            }

            // Add the currently parsed attachment count to the index
            currentAttachmentArgumentIndex += context.MultiArgumentParameterIndex;

            // Return the attachment from the original message
            return textConverterContext.Message.Attachments.Count <= currentAttachmentArgumentIndex
                ? Task.FromResult(Optional.FromNoValue<DiscordAttachment>())
                : Task.FromResult(
                    Optional.FromValue(
                        textConverterContext.Message.Attachments[currentAttachmentArgumentIndex]
                    )
                );
        }
        else if (
            context is InteractionConverterContext interactionConverterContext
            &&
            // Resolved can be null on autocomplete contexts
            interactionConverterContext.Interaction.Data.Resolved
                is not null
            // Check if we have enough attachments to fetch the current attachment
            && interactionConverterContext.Interaction.Data.Options.Count(argument =>
                argument.Type == DiscordApplicationCommandOptionType.Attachment
            ) >= currentAttachmentArgumentIndex
            // Check if we can parse the attachment ID (this should be guaranteed by Discord)
            && ulong.TryParse(
                interactionConverterContext.Argument?.RawValue,
                CultureInfo.InvariantCulture,
                out ulong attachmentId
            )
            // Check if the attachment exists
            && interactionConverterContext.Interaction.Data.Resolved.Attachments.TryGetValue(
                attachmentId,
                out DiscordAttachment? attachment
            )
        )
        {
            return Task.FromResult(Optional.FromValue(attachment));
        }

        return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
    }
}
