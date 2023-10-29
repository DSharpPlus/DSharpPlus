using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class AttachmentConverter : ISlashArgumentConverter<DiscordAttachment>, ITextArgumentConverter<DiscordAttachment>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Attachment;

        public Task<Optional<DiscordAttachment>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
        {
            int currentAttachmentArgumentIndex = context.Command.Arguments.Where(argument => argument.Type == typeof(DiscordAttachment)).IndexOf(context.Argument);
            return eventArgs.Message.Attachments.Count < currentAttachmentArgumentIndex
                ? Task.FromResult(Optional.FromNoValue<DiscordAttachment>())
                : Task.FromResult(Optional.FromValue(eventArgs.Message.Attachments[currentAttachmentArgumentIndex]));
        }

        public Task<Optional<DiscordAttachment>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
        {
            if (eventArgs.Interaction.Type != InteractionType.ApplicationCommand)
            {
                return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
            }

            int currentAttachmentArgumentIndex = context.Command.Arguments.Where(argument => argument.Type == typeof(DiscordAttachment)).IndexOf(context.Argument);
            if (eventArgs.Interaction.Data.Options.Count(argument => argument.Type == ApplicationCommandOptionType.Attachment) < currentAttachmentArgumentIndex)
            {
                return Task.FromResult(Optional.FromNoValue<DiscordAttachment>());
            }

            DiscordInteractionDataOption option = eventArgs.Interaction.Data.Options.ElementAt(context.ArgumentIndex);
            return Task.FromResult(Optional.FromValue(eventArgs.Interaction.Data.Resolved.Attachments[(ulong)option.Value]));
        }
    }
}
