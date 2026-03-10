#pragma warning disable IDE0040

using System.Threading.Tasks;

using DSharpPlus.Voice.Exceptions;

using DSharpPlus.Voice.Protocol.Gateway;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private async Task HandleCloseCodeAsync(VoiceGatewayCloseCode closeCode)
    {
        // note regarding discarding the resume/reconnect tasks in this switch: this loop gets restarted by ReconnectCoreAsync (since it needs to ensure the VGW
        // is in the proper state to just arbitrarily receive events again). if we awaited them, we would nuke our own async call stack, and that would not be ideal.
        switch (closeCode)
        {
            case VoiceGatewayCloseCode.NormalClosure:
            case VoiceGatewayCloseCode.Empty:
            case VoiceGatewayCloseCode.InternalServerError:
            
                // we get one tick of NormalClosure when we disconnect, don't auto-reconnect on that
                if (!this.isDisconnecting)
                {
                    this.logger.LogDebug("The voice gateway connection was closed by the remote server, reconnecting.");
                    _ = ResumeAndReconnectAsync();
                }
                else
                {
                    this.logger.LogDebug("Disconnecting from the voice gateway.");
                }

                break;

            case VoiceGatewayCloseCode.InvalidMessageType:
            case VoiceGatewayCloseCode.InvalidPayloadData:

                this.logger.LogCritical("A payload was sent with an incorrect or invalid message type. This is a library bug - please report to library developers.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.LibraryIssue);

            break;

            case VoiceGatewayCloseCode.PolicyViolation:
                      
                this.logger.LogWarning("The voice gateway connection was closed on account of client misbehaviour. Please consult with library developers.");
                _ = ResumeAndReconnectAsync();

                break;

            case VoiceGatewayCloseCode.EndpointUnavailable:

                this.logger.LogError("The specified voice endpoint was unavailable.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.VoiceEndpointUnavailable);

                break;

            case VoiceGatewayCloseCode.MessageTooBig:

                this.logger.LogCritical("A voice gateway payload we sent was too big. This is a library bug - please report to library developers.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.LibraryIssue);

                break;

            case VoiceGatewayCloseCode.UnknownOpcode:

                this.logger.LogCritical("A payload was sent with an invalid opcode. This is a library bug - please report to library developers.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.LibraryIssue);

                break;

            case VoiceGatewayCloseCode.FailedToAuthenticate:
            case VoiceGatewayCloseCode.AlreadyAuthenticated:

                this.logger.LogCritical("An invalid IDENTIFY payload was sent. This is a library bug - please report to library developers.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.LibraryIssue);

                break;

            case VoiceGatewayCloseCode.NotAuthenticated:

                this.logger.LogCritical("A payload was sent before IDENTIFY. This is a library bug - please report to library developers.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.LibraryIssue);

                break;

            case VoiceGatewayCloseCode.AuthenticationFailed:
            case VoiceGatewayCloseCode.ServerNotFound:

                this.logger.LogError("The voice session has ceased to exist, disconnecting.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.SessionInvalid);

                break;

            case VoiceGatewayCloseCode.SessionInvalid:
            case VoiceGatewayCloseCode.SessionTimeout:

                this.logger.LogDebug("The voice session is invalid or has timed out, reconnecting.");
                _ = ReconnectInternalAsync(true);

                break;

            case VoiceGatewayCloseCode.UnknownProtocol:

                this.logger.LogCritical("An invalid audio protocol was specified. This is a library bug - please report to library developers.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.LibraryIssue);

                break;

            case VoiceGatewayCloseCode.Disconnected:

                this.logger.LogDebug("The bot was kicked from the voice channel, disconnecting.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.Kicked);

                break;

            case VoiceGatewayCloseCode.VoiceServerCrashed:

                this.logger.LogDebug("The voice server crashed, resuming.");
                _ = ResumeAndReconnectAsync();

                break;

            case VoiceGatewayCloseCode.UnknownEncryptionMode:

                this.logger.LogDebug("We specified an unknown encryption mode, reconnecting and renegotiating.");
                _ = ReconnectInternalAsync(true);

                break;

            case VoiceGatewayCloseCode.E2EERequired:

                this.logger.LogCritical("Failed to use end-to-end encryption. This is a library bug - please report to library developers.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.LibraryIssue);

                break;

            case VoiceGatewayCloseCode.BadRequest:

                this.logger.LogDebug("A malformed payload was sent to the gateway, resuming.");
                _ = ResumeAndReconnectAsync();

                break;

            case VoiceGatewayCloseCode.Ratelimited:

                this.logger.LogError("Voice connection ratelimit encountered.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.Ratelimited);

                break;

            case VoiceGatewayCloseCode.CallTerminated:

                this.logger.LogDebug("The call was forcibly terminated.");
                await DisconnectAndReportReasonAsync(VoiceDisconnectReason.CallTerminated);

                break;
        }
    }

    private async Task DisconnectAndReportReasonAsync(VoiceDisconnectReason reason)
    {
        this.isDisconnecting = true;

        if (this.disconnectHandler is not null)
        {
            await this.disconnectHandler(reason, this.disconnectHandlerState);
        }
        else
        {
            // if the handler is null, but the 'mlsReady' TCS isn't null, we're connecting. otherwise, don't throw (something something UnobservedTaskException)
            // and let the log message suffice
            this.mlsReady?.TrySetException(new ConnectingFailedException($"Our connection from the voice channel was severed for the following reason: {reason}. "
                + "More information may have been logged by the extension."));
        }

        await DisposeAsync();
    }
}