#pragma warning disable IDE0040

using System.Threading.Tasks;

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
            
                this.logger.LogDebug("The voice gateway connection was closed by the remote server, reconnecting.");
                _ = ResumeAndReconnectAsync();

                break;

            case VoiceGatewayCloseCode.InvalidMessageType:
            case VoiceGatewayCloseCode.InvalidPayloadData:

                this.logger.LogCritical("A payload was sent with an incorrect or invalid message type. This is a library bug - please report to library developers.");
                // [TODO] find a way to throw this up to the user

            break;

            case VoiceGatewayCloseCode.PolicyViolation:
                      
                this.logger.LogWarning("The voice gateway connection was closed on account of client misbehaviour. Please consult with library developers.");
                _ = ResumeAndReconnectAsync();

                break;

            case VoiceGatewayCloseCode.EndpointUnavailable:

                this.logger.LogError("The specified voice endpoint was unavailable.");
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.MessageTooBig:

                this.logger.LogCritical("A voice gateway payload we sent was too big. This is a library bug - please report to library developers.");
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.UnknownOpcode:

                this.logger.LogCritical("A payload was sent with an invalid opcode. This is a library bug - please report to library developers.");
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.FailedToAuthenticate:
            case VoiceGatewayCloseCode.AlreadyAuthenticated:

                this.logger.LogCritical("An invalid IDENTIFY payload was sent. This is a library bug - please report to library developers.");
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.NotAuthenticated:

                this.logger.LogCritical("A payload was sent before IDENTIFY. This is a library bug - please report to library developers.");
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.AuthenticationFailed:
            case VoiceGatewayCloseCode.ServerNotFound:

                this.logger.LogError("The voice session has ceased to exist, disconnecting.");
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.SessionInvalid:
            case VoiceGatewayCloseCode.SessionTimeout:

                this.logger.LogDebug("The voice session is invalid or has timed out, reconnecting.");
                _ = ReconnectInternalAsync(true);

                break;

            case VoiceGatewayCloseCode.UnknownProtocol:

                this.logger.LogCritical("An invalid audio protocol was specified. This is a library bug - please report to library developers.");
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.Disconnected:

                this.logger.LogDebug("The bot was kicked from the voice channel, disconnecting.");
                // [TODO] find a way to throw this up to the user

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
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.BadRequest:

                this.logger.LogDebug("A malformed payload was sent to the gateway, resuming.");
                _ = ResumeAndReconnectAsync();

                break;

            case VoiceGatewayCloseCode.RateLimited:

                this.logger.LogError("Voice connection ratelimit encountered.");
                // [TODO] find a way to throw this up to the user

                break;

            case VoiceGatewayCloseCode.CallTerminated:

                this.logger.LogDebug("The call was forcibly terminated.");
                // [TODO] find a way to throw this up to the user

                break;
        }
    }
}