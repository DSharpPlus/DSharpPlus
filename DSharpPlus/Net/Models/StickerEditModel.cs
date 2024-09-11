using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a sticker.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class StickerEditModel : BaseEditModel
{
    /// <summary>
    /// The new sticker name.
    /// </summary>
    public Optional<string> Name { get; set; }

    /// <summary>
    /// The new sticker description.
    /// </summary>
    public Optional<string> Description { get; set; }

    /// <summary>
    /// The new sticker tags, delimited by commas.
    /// </summary>
    public Optional<string> Tags { get; set; }
}
