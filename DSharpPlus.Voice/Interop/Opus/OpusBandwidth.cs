namespace DSharpPlus.Voice.Interop.Opus;

internal enum OpusBandwidth
{
    Auto,

    // 4khz bandpass
    Narrowband = 1101,

    // 6khz bandpass
    Mediumband = 1102,

    // 8khz bandpass
    Wideband = 1103,

    // <12khz bandpass
    Superwideband = 1104,

    // <20khz bandpass
    Fullband = 1105
}
