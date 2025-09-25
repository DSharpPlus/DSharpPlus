using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Provides information about files uploaded through a modal submission.
/// </summary>
public sealed class FileUploadModalSubmission : IModalSubmission
{
    internal FileUploadModalSubmission(string customId, IReadOnlyList<DiscordAttachment> uploadedFiles)
    {
        this.CustomId = customId;
        this.UploadedFiles = uploadedFiles;
    }

    /// <inheritdoc/>
    public DiscordComponentType ComponentType => DiscordComponentType.FileUpload;

    /// <inheritdoc/>
    public string CustomId { get; internal set; }

    /// <summary>
    /// The files uploaded to the modal.
    /// </summary>
    public IReadOnlyList<DiscordAttachment> UploadedFiles { get; internal set; }
}
