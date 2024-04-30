namespace DSharpPlus.Net.Models;

using DSharpPlus.Entities;

public class MembershipScreeningEditModel : BaseEditModel
{
    /// <summary>
    /// Sets whether membership screening should be enabled for this guild
    /// </summary>
    public Optional<bool> Enabled { internal get; set; }

    /// <summary>
    /// Sets the server description shown in the membership screening form
    /// </summary>
    public Optional<string> Description { internal get; set; }

    /// <summary>
    /// Sets the fields in this membership screening form
    /// </summary>
    public Optional<DiscordGuildMembershipScreeningField[]> Fields { internal get; set; }

    internal MembershipScreeningEditModel() { }
}
