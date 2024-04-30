namespace DSharpPlus.Net.Models;
using DSharpPlus.Entities;

public class StickerEditModel : BaseEditModel
{
    public Optional<string> Name { internal get; set; }

    public Optional<string> Description { internal get; set; }

    public Optional<string> Tags { internal get; set; }
}
