namespace DSharpPlus;

/// <summary>
/// A list of ansi colors supported by Discord.
/// </summary>
/// <remarks>
/// Background support in the client is dodgy at best.
/// These colors are mapped as per the ansi standard, but may not appear correctly in the client.
/// </remarks>
public enum AnsiColor
{
    Reset = 0,
    Bold = 1,
    Underline = 4,

    Black = 30,
    Red = 31,
    Green = 32,
    Yellow = 33,
    Blue = 34,
    Magenta = 35,
    Cyan = 36,
    White = 37,
    LightGray = 38,

    BlackBackground = 40,
    RedBackground = 41,
    GreenBackground = 42,
    YellowBackground = 43,
    BlueBackground = 44,
    MagentaBackground = 45,
    CyanBackground = 46,
    WhiteBackground = 47,

}
