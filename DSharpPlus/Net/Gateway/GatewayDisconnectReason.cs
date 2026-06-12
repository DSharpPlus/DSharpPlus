namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Specifies why the gateway disconnected.
/// </summary>
public enum GatewayDisconnectReason
{
    /// <summary>
    /// An unknown error occurred.
    /// </summary>
    UnknownError,

    /// <summary>
    /// Shutdown was explicitly requested by the user.
    /// </summary>
    UserRequested,

    /// <summary>
    /// An irrecoverable close code was encountered. Orchestrators should not attempt to reconnect.
    /// </summary>
    IrrecoverableCloseCode,

    /// <summary>
    /// A recoverable close code was encountered. Orchestrators may attempt to reconnect.
    /// </summary>
    RecoverableCloseCode,

    /// <summary>
    /// The connection zombied. Orchestrators may attempt to reconnect.
    /// </summary>
    Zombied,

    /// <summary>
    /// An internal error was encountered and the gateway needs to restart. Orchestrators should attempt to reconnect.
    /// </summary>
    InternalError,

    /// <summary>
    /// The session was invalidated and needs to be restarted.
    /// </summary>
    SessionInvalidated,

    /// <summary>
    /// The internet connection was severed.
    /// </summary>
    ConnectionSevered
}
