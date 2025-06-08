namespace DSharpPlus.Voice.Interop.Aerith;

/// <summary>
/// Represents the severity of an error encountered when processing a commit.
/// </summary>
internal enum AerithCommitError
{
    Success = 0,

    /// <summary>
    /// Indicates the message was rejected and should trigger a reset.
    /// </summary>
    HardError = 1,

    /// <summary>
    /// Indicates the message was rejected but should not trigger a reset.
    /// </summary>
    SoftError = 2
}
