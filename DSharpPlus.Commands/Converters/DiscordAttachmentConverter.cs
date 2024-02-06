namespace DSharpPlus.Commands.Converters;

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class AttachmentConverter : ISlashArgumentConverter<DiscordAttachment>, ITextArgumentConverter<DiscordAttachment>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Attachment;
    public bool RequiresText { get; init; }

    public Task<Optional<DiscordAttachment>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
    {
        int currentAttachmentArgumentIndex = context.Command.Parameters.Where(argument => argument.Type == typeof(DiscordAttachment)).IndexOf(context.Parameter);
        return eventArgs.Message.Attachments.Count <= currentAttachmentArgumentIndex
            ? Task.FromResult(Optional.FromNoValue<DiscordAttachment>()) // Too many parameters, not enough attachments
            : Task.FromResult(Optional.FromValue(eventArgs.Message.Attachments[currentAttachmentArgumentIndex]));
    }

    public Task<Optional<DiscordAttachment>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        int currentAttachmentArgumentIndex = context.Command.Parameters.Where(argument => argument.Type == typeof(DiscordAttachment)).IndexOf(context.Parameter);
        return eventArgs.Interaction.Data.Options.Count(argument => argument.Type == ApplicationCommandOptionType.Attachment) < currentAttachmentArgumentIndex
            ? Task.FromResult(Optional.FromNoValue<DiscordAttachment>()) // Too many parameters, not enough attachments
            : Task.FromResult(Optional.FromValue(eventArgs.Interaction.Data.Resolved.Attachments[(ulong)context.As<SlashConverterContext>().Argument.Value]));
    }
}
