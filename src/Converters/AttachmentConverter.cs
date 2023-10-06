using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class AttachmentConverter
    {
        [Converter]
        public static Task<Optional<DiscordAttachment>> ConvertAsync(ConverterContext context)
        {
            // Figure out which position the argument is in and how many attachments were sent.
            int totalAttachmentArguments = context.Command.Arguments.Count(argument => argument.Type == typeof(DiscordAttachment));
            int currentAttachmentArgument = context.Command.Arguments.IndexOf(context.Argument);

            if (context.EventArgs is MessageCreateEventArgs messageCreateEventArgs
                // Ensure we have enough attachments to match to arguments.
                && messageCreateEventArgs.Message.Attachments.Count > currentAttachmentArgument)
            {
                return Task.FromResult(Optional.FromValue(messageCreateEventArgs.Message.Attachments[currentAttachmentArgument]));
            }
            else if (context.EventArgs is InteractionCreateEventArgs eventArgs
                && eventArgs.Interaction.Type == InteractionType.ApplicationCommand
                // Ensure we have enough attachments to match to arguments.
                && eventArgs.Interaction.Data.Options.Count(argument => argument.Type == ApplicationCommandOptionType.Attachment) > currentAttachmentArgument)
            {
                return Task.FromResult(Optional.FromValue((DiscordAttachment)eventArgs.Interaction.Data.Options.ElementAt(currentAttachmentArgument).Value));
            }

            // Either we didn't have enough attachments to match to arguments or the event args were an unsupported type.
            return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
        }
    }
}
