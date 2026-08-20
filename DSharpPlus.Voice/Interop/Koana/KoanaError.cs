namespace DSharpPlus.Voice.Interop.Koana;

internal enum KoanaError
{
    Success = 0,

    /// <summary>
    /// Returned if a native operation ran out of memory.
    /// </summary>
    OutOfMemory = 1,

    /// <summary>
    /// Returned if an uneven amount of keys and values was found inside a roster due to concurrent modification.
    /// </summary>
    LengthMismatch = 2,

    /// <summary>
    /// Returned if a frame could not be successfully encrypted.
    /// </summary>
    EncryptionFailure = 100,

    /// <summary>
    /// Returned if a MLS error occurred that requires resetting the session.
    /// </summary>
    MlsCommitResetError = 1000,

    /// <summary>
    /// Returned if a MLS error occurred that may be ignored.
    /// </summary>
    MlsCommitIgnorableError = 1001
}
