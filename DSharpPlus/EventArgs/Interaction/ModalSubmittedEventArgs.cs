using System.Collections.Generic;
using System.Linq;

using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when a modal is submitted. Note that this event is fired only if the modal is submitted by the user, and not if the modal is closed.
/// </summary>
public class ModalSubmittedEventArgs : InteractionCreatedEventArgs
{
    /// <summary>
    /// A dictionary of submitted fields, keyed on the custom id of the input component.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, IModalSubmission> Values { get; }

    /// <summary>
    /// The custom ID this modal was sent with.
    /// </summary>
    [JsonIgnore]
    public string Id => this.Interaction.Data.CustomId;

    internal ModalSubmittedEventArgs(DiscordInteraction interaction)
    {
        this.Interaction = interaction;

        Dictionary<string, IModalSubmission> dict = [];

        foreach (DiscordComponent component in interaction.Data.components)
        {
            if (component is not DiscordLabelComponent label)
            {
                continue;
            }

            dict.Add
            (
                label.Component.CustomId, label.Component switch
                {
                    DiscordTextInputComponent input => new TextInputModalSubmission(input.CustomId, input.Value),

                    DiscordSelectComponent select => new SelectMenuModalSubmission(select.CustomId, select.SubmittedValues ?? []),

                    DiscordChannelSelectComponent channel
                        => new ChannelSelectMenuModalSubmission(channel.CustomId, (channel.SubmittedValues ?? []).Select(ulong.Parse).ToArray()),

                    DiscordUserSelectComponent user
                        => new UserSelectMenuModalSubmission(user.CustomId, (user.SubmittedValues ?? []).Select(ulong.Parse).ToArray()),

                    DiscordRoleSelectComponent role
                        => new RoleSelectMenuModalSubmission(role.CustomId, (role.SubmittedValues ?? []).Select(ulong.Parse).ToArray()),

                    DiscordMentionableSelectComponent mentionable
                        => new MentionableSelectMenuModalSubmission(mentionable.CustomId, (mentionable.SubmittedValues ?? []).Select(ulong.Parse).ToArray()),

                    DiscordFileUploadComponent fileUpload
                        => new FileUploadModalSubmission(fileUpload.CustomId, (fileUpload.Values ?? [])
                            .Select(x => interaction.Data.Resolved.Attachments[x])
                            .ToArray()),

                    DiscordRadioGroupComponent radioGroup => new RadioGroupModalSubmission(radioGroup.CustomId, radioGroup.Value!),

                    DiscordCheckboxGroupComponent checkboxGroup => new CheckboxGroupModalSubmission(checkboxGroup.CustomId, checkboxGroup.Values!),

                    DiscordCheckboxComponent checkbox => new CheckboxModalSubmission(checkbox.CustomId, checkbox.Value!.Value),

                    _ => new UnknownComponentModalSubmission(label.Component.Type, label.Component.CustomId, label.Component)
                }
            );
        }

        this.Values = dict;
    }
}
