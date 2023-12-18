using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Newtonsoft.Json;
namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when a modal is submitted. Note that this event is fired only if the modal is submitted by the user, and not if the modal is closed.
/// </summary>
public class ModalSubmitEventArgs : InteractionCreateEventArgs
{
    /// <summary>
    /// A dictionary of submitted fields, keyed on the custom id of the input component.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, string> Values { get; }

    internal ModalSubmitEventArgs(DiscordInteraction interaction)
    {
        this.Interaction = interaction;

        Dictionary<string, string> dict = new Dictionary<string, string>();

        foreach (DiscordActionRowComponent component in interaction.Data._components)
        {
            if (component.Components.First() is TextInputComponent input)
            {
                dict.Add(input.CustomId, input.Value);
            }
        }

        this.Values = dict;
    }
}
