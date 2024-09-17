namespace DSharpPlus.Net.Gateway.Compression.Zstd;

/// <summary>
/// Enumerates zstd error codes we care about handling in code.
/// </summary>
internal enum ZstdErrorCode
{
    NoError = 0,
    Generic = 1,
    VersionUnsupported = 12,
    InitMissing = 62,
    DestinationSizeTooSmall = 70,
    SourceSizeWrong = 72,
    DestinationBufferNull = 74,
    DestinationFull = 80,
    SourceEmpty = 82
}
