using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class AttachmentConverter
    {
        [Converter<DiscordAttachment>, SlashConverter(ApplicationCommandOptionType.Attachment)]
        public static Task<IOptional> ConvertAsync(ConverterContext context)
        {
            // Figure out which position the argument is in and how many attachments were sent.
            IEnumerable<CommandArgument> totalAttachmentArguments = context.Command.Arguments.Where(argument => argument.Type == typeof(DiscordAttachment));
            int currentAttachmentArgument = totalAttachmentArguments.ToList().IndexOf(context.Argument!);

            if (context.EventArgs is MessageCreateEventArgs messageCreateEventArgs
                // Ensure we have enough attachments to match to arguments.
                && messageCreateEventArgs.Message.Attachments.Count > currentAttachmentArgument)
            {
                return Task.FromResult<IOptional>(Optional.FromValue(messageCreateEventArgs.Message.Attachments[currentAttachmentArgument]));
            }
            else if (context.EventArgs is InteractionCreateEventArgs eventArgs
                && eventArgs.Interaction.Type == InteractionType.ApplicationCommand
                // Ensure we have enough attachments to match to arguments.
                && eventArgs.Interaction.Data.Options.Count(argument => argument.Type == ApplicationCommandOptionType.Attachment) > currentAttachmentArgument)
            {
                DiscordInteractionDataOption option = eventArgs.Interaction.Data.Options.ElementAt(context.Command.Arguments.IndexOf(context.Argument));
                return Task.FromResult<IOptional>(Optional.FromValue(eventArgs.Interaction.Data.Resolved.Attachments[(ulong)option.Value]));
            }

            // Either we didn't have enough attachments to match to arguments or the event args were an unsupported type.
            return Task.FromResult<IOptional>(Optional.FromNoValue<DiscordAttachment>());
        }
    }
}
