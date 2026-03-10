#pragma warning disable IDE0040

using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Clients;
using DSharpPlus.EventArgs;
using DSharpPlus.Voice.Exceptions;
using DSharpPlus.Voice.Protocol;
using DSharpPlus.Voice.Protocol.Gateway;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.Bidirectional;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.Clientbound;
using DSharpPlus.Voice.Protocol.Gateway.Payloads.Serverbound;
using DSharpPlus.Voice.Protocol.RTCP.Payloads;
using DSharpPlus.Voice.Transport;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private const int MaxDaveVersion = 1;
    
    // [NOTE] this code makes the assumption that discord will have the good sense to not change the semantics or contents of opcodes
    // that get exchanged before we know the DAVE version in future DAVE versions. if they don't show that modicum of common sense, 
    // that is a problem.
    internal async Task ConnectAsync()
    {
        ulong channelId = this.channelId;
        ulong guildId = this.guildId;
        this.mlsReady = new();

        this.logger = this.loggerFactory.CreateLogger($"DSharpPlus.Voice.VoiceConnection - Channel {channelId}");
        this.logger.LogDebug("Initiating connection to the voice gateway, DAVE version {daveVersion}", MaxDaveVersion);

        this.logger.LogTrace("Retrieving voice server information from the main gateway.");

        // set up our machinery for receiving events from the main gateway
        Task<EventWaiterResult<VoiceStateUpdatedEventArgs>> voiceStateUpdateTask = this.dispatcher.CreateEventWaiter<VoiceStateUpdatedEventArgs>
        (
            x => x.ChannelId == channelId,
            TimeSpan.FromSeconds(15)
        ).Task;

        Task<EventWaiterResult<VoiceServerUpdatedEventArgs>> voiceServerUpdateTask = this.dispatcher.CreateEventWaiter<VoiceServerUpdatedEventArgs>
        (
            x => x.Guild.Id == guildId,
            TimeSpan.FromSeconds(15)
        ).Task;

        VoiceStateUpdateEvent update = new()
        {
            // [TODO] do we want to allow passing mute/deafen here?
            Data = new()
            {
                GuildId = guildId,
                ChannelId = channelId,
                Mute = false,
                Deafen = false
            }
        };

        await this.shardOrchestrator.SendOutboundEventAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(update)), guildId);
        await Task.WhenAll(voiceStateUpdateTask, voiceServerUpdateTask);

        // if one (or both) of our events timed out, abandon ship
        if (voiceStateUpdateTask.Result.TimedOut || voiceServerUpdateTask.Result.TimedOut)
        {
            throw new ConnectingFailedException("Failed to open a connection to the voice gateway because Discord did not provide the necessary information.");
        }

        VoiceStateUpdatedEventArgs voiceState = voiceStateUpdateTask.Result.Value!;
        VoiceServerUpdatedEventArgs voiceServer = voiceServerUpdateTask.Result.Value!;

        this.sessionId = voiceState.SessionId;
        this.endpoint = $"wss://{voiceServer.Endpoint}?v=8";
        this.token = voiceServer.VoiceToken;

        // we have all necessary information, connect to the vgw and identify
        await this.voiceGateway.ConnectAsync(this.endpoint, channelId);

        await this.voiceGateway.SendTextAsync(new()
        {
            Opcode = VoiceGatewayOpcode.Identify,
            Payload = new VoiceIdentifyPayload()
            {
                GuildId = guildId,
                UserId = this.userId,
                SessionId = this.sessionId,
                Token = this.token,
                HighestSupportedDaveVersion = MaxDaveVersion
            }
        });

        // at this point, we will receive opcode 8 HELLO and opcode 2 READY in no particular order. in theory, READY contains everything
        // we need including the heartbeat interval (which is HELLO's sole raison d'être), however, the heartbeat interval in READY is a
        // lying liar who lies and is incorrect, so we need HELLO anyway.

        bool helloReceived = false, readyReceived = false;
        string selectedEncryptionMode = default;
        IPEndPoint remoteUdpEndpoint = default;

        while (!helloReceived || !readyReceived)
        {
            VoiceGatewayTransportFrame frame = await this.voiceGateway.ReceiveAsync();

            if (frame.Type == WebSocketMessageType.Close)
            {
                await HandleCloseCodeAsync(frame.Error);
            }

            if (frame.Opcode == VoiceGatewayOpcode.Ready && frame.Type == WebSocketMessageType.Text)
            {
                readyReceived = true;
                
                VoiceGatewayMessage readyMessage = JsonSerializer.Deserialize<VoiceGatewayMessage>(frame.Payload)!;
                VoiceReadyPayload ready = (VoiceReadyPayload)readyMessage.Payload;

                remoteUdpEndpoint = new(IPAddress.Parse(ready.IPAddress), ready.Port);
                this.ssrc = ready.SSRC;
                selectedEncryptionMode = this.cryptorFactory.SelectPreferredEncryptionMode(ready.EncryptionModes);

                this.lastSequence = readyMessage.Sequence;
            }

            if (frame.Opcode == VoiceGatewayOpcode.Hello && frame.Type == WebSocketMessageType.Text)
            {
                helloReceived = true;
                
                VoiceGatewayMessage helloMessage = JsonSerializer.Deserialize<VoiceGatewayMessage>(frame.Payload)!;
                VoiceHelloPayload hello = (VoiceHelloPayload)helloMessage.Payload;

                _ = HeartbeatAsync(hello.HeartbeatInterval, this.heartbeatCancellation.Token);
            }
        }

        // we have now received every bit of information necessary to connect to UDP...
        await this.mediaTransport.ConnectAsync(remoteUdpEndpoint);
        IPEndPoint localEndpoint = await PerformIPDiscoveryAsync(this.ssrc);

        // ... and to select the protocol we want
        await this.voiceGateway.SendTextAsync(new()
        {
            Opcode = VoiceGatewayOpcode.SelectProtocol,
            Payload = new VoiceSelectProtocolPayload()
            {
                Protocol = "udp",
                Data = new()
                {
                    IPAddress = localEndpoint.Address.ToString(),
                    Port = localEndpoint.Port,
                    EncryptionMode = selectedEncryptionMode
                }
            }
        });

        while (true)
        {
            VoiceGatewayTransportFrame sessionDescriptionFrame = await this.voiceGateway.ReceiveAsync();

            if (sessionDescriptionFrame.Type == WebSocketMessageType.Close)
            {
                await HandleCloseCodeAsync(sessionDescriptionFrame.Error);
            }

            if (sessionDescriptionFrame.Opcode != VoiceGatewayOpcode.SessionDescription || sessionDescriptionFrame.Type == WebSocketMessageType.Binary)
            {
                // something silly happened
                continue;
            }

            VoiceGatewayMessage sessionDescription = JsonSerializer.Deserialize<VoiceGatewayMessage>(sessionDescriptionFrame.Payload);
            VoiceSessionDescriptionPayload sd = (VoiceSessionDescriptionPayload)sessionDescription.Payload;

            if (selectedEncryptionMode != sd.EncryptionMode)
            {
                throw new ConnectingFailedException("Failed to negotiate transport encryption mode.");
            }

            this.cryptor = this.cryptorFactory.CreateCryptor(selectedEncryptionMode, sd.SecretKey);
            this.daveVersion = sd.DaveProtocolVersion;

            break;
        }

        this.e2ee.Initialize((ushort)this.daveVersion, channelId, this.userId, this.ssrc);
        _ = ReceiveVoiceGatewayEventsAsync(this.vgwCancellation.Token);

        // this is when we tell the gateway about our local encryption keys
        await (this.daveVersion switch
        {
            1 => DaveV1AnnounceKeyPackageAsync(),
            _ => LogErrorAndReconnectAsync("Invalid DAVE version {daveVersion}.", this.daveVersion)
        });

        // ... and we await its response (which comes with the other users' keys, kind of important)
        await this.mlsReady.Task;

        // make ourselves known across RTCP. we must send a receiver report first thing, and we can just
        // concatenate that with a description packet
        RTCPReceiverReportPacket initial = new()
        {
            SSRC = this.ssrc,
            ReceptionReports = []
        };

        RTCPSourceDescriptionPacket description = new()
        {
            SourceDescriptions = [new()
            {
                SSRC = this.ssrc,
                DescriptionItems = 
                [
                    new()
                    {
                        Type = SourceDescriptionItemType.CanonicalName,
                        Value = this.userId.ToString()
                    },
                    new()
                    {
                        Type = SourceDescriptionItemType.ApplicationName,
                        Value = $"DSharpPlus.Voice v{Utilities.Version}"
                    }
                ]
            }]
        };

        await SendRTCPPacketsAsync([initial, description]);

        // only start these loops the first time
        this.receiveAudioTask ??= ReceiveAudioAsync(this.audioCancellation.Token);
        this.audioKeepaliveTask ??= AudioKeepaliveAsync(this.audioCancellation.Token);
        this.sendAudioTask ??= SendAudioAsync(this.audioCancellation.Token);
    }

    /// <summary>
    /// Disconnects from the voice chat. It is not possible to reconnect again.
    /// </summary>
    public async Task DisconnectAsync()
    {
        this.isDisconnecting = true;
        await DisconnectAndReportReasonAsync(VoiceDisconnectReason.Disconnected);
    }


    /// <summary>
    /// Reconnects to the Discord voice gateway.
    /// </summary>
    public async Task<bool> ReconnectAsync()
        => await ReconnectInternalAsync(true);

    private async Task<bool> ReconnectInternalAsync(bool manual)
    {
        if (!manual && !this.options.AutoReconnect)
        {
            this.logger.LogDebug("Abandoning automatic reconnect because automatic reconnecting was disabled by the user.");
            return false;
        }

        // we're already disconnecting, leave it
        if (this.isDisconnecting)
        {
            return false;
        }

        for (uint i = 0; i < this.options.MaxReconnects; i++)
        {
            try
            {
                await ReconnectCoreAsync();
                return true;
            }
            catch (Exception e) when (e is ConnectingFailedException or InvalidOperationException)
            {
                this.logger.LogError(e, "Automatic reconnecting failed due to the following error, abandoning:");
                return false;
            }
            catch (Exception e)
            {
                this.logger.LogTrace(e, "Automatic reconnecting failed due to the following error, retrying:");
                continue;
            }
        }

        this.logger.LogDebug("Abandoning automatic reconnect because the attempt limit was exceeded.");
        return false;
    }

    /// <summary>
    /// Indicates to Discord that we are ready to send audio.
    /// </summary>
    public async Task StartSpeakingAsync()
    {
        this.logger.LogDebug("Announcing that we will start sending audio.");

        await SendSpeakingStatusAsync(VoiceSpeakingFlags.Microphone);

        this.IsSpeaking = true;
    }

    /// <summary>
    /// Indicates to Discord that we will not send further audio.
    /// </summary>
    public async Task StopSpeakingAsync()
    {
        this.logger.LogDebug("Announcing that we will stop sending audio.");

        await SendSpeakingStatusAsync(VoiceSpeakingFlags.None);

        this.IsSpeaking = false;
    }

    private async Task SendSpeakingStatusAsync(VoiceSpeakingFlags flags)
    {
        await this.voiceGateway.SendTextAsync(new()
        {
            Payload = new VoiceSpeakingPayload()
            {
                Delay = 0,
                SpeakingMode = flags,
                SSRC = this.ssrc,
            },
            Opcode = VoiceGatewayOpcode.Speaking
        });
    }

    private async Task HeartbeatAsync(int heartbeatInterval, CancellationToken ct)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(heartbeatInterval));

        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                DateTimeOffset time = DateTimeOffset.UtcNow;

                await this.voiceGateway.SendTextAsync(new()
                {
                    Opcode = VoiceGatewayOpcode.Heartbeat,
                    Payload = new VoiceHeartbeatPayload()
                    {
                        Nonce = time.ToUnixTimeMilliseconds(),
                        LastAcknowledgedSequence = this.lastSequence
                    }
                });

                this.logger.LogTrace("Heartbeat sent with sequence number {sequence}.", this.lastSequence);

                this.pendingHeartbeats++;

                if (this.pendingHeartbeats > 5)
                {
                    await ResumeAndReconnectAsync();
                }
            }
            catch (WebSocketException e)
            {
                this.logger.LogWarning("The connection died or entered an invalid state, reconnecting. Exception: {message}", e.Message);
                await ResumeAndReconnectAsync();

                return;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "An error occurred during heartbeating.");
            }
        }
    }

    private async Task ReceiveVoiceGatewayEventsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            VoiceGatewayTransportFrame frame = await this.voiceGateway.ReceiveAsync();

            try
            {
                await HandleReceivedEventAsync(frame);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Encountered an error while handling a voice gateway event.");
            }
        }
    }

    private async Task HandleReceivedEventAsync(VoiceGatewayTransportFrame frame)
    {
        if (frame.Type == WebSocketMessageType.Text)
        {
            VoiceGatewayMessage message = JsonSerializer.Deserialize<VoiceGatewayMessage>(frame.Payload)!;

            if (message.Sequence != -1)
            {
                this.lastSequence = message.Sequence;
            }

            switch (frame.Opcode)
            {
                case VoiceGatewayOpcode.HeartbeatAck:

                    this.pendingHeartbeats = 0;

                    break;

                case VoiceGatewayOpcode.ClientsConnected:

                    VoiceClientsConnectedPayload clientsConnected = (VoiceClientsConnectedPayload)message.Payload;
                    
                    this.connectedUsers.AddRange(clientsConnected.UserIds);

                    break;

                case VoiceGatewayOpcode.ClientDisconnected:

                    VoiceClientDisconnectedPayload clientDisconnected = (VoiceClientDisconnectedPayload)message.Payload;

                    this.connectedUsers.Remove(clientDisconnected.UserId);

                    break;

                case VoiceGatewayOpcode.Speaking:

                    VoiceSpeakingPayload speaking = (VoiceSpeakingPayload)message.Payload;

                    this.Receiver.IntroduceNewUser(new()
                    {
                        UserId = speaking.UserId,
                        SSRC = speaking.SSRC,
                        IsSpeaking = speaking.SpeakingMode.HasFlag(VoiceSpeakingFlags.Microphone)
                    });

                    break;

                case VoiceGatewayOpcode.PrepareTransition:
                case VoiceGatewayOpcode.ExecuteTransition:
                case VoiceGatewayOpcode.PrepareEpoch:

                    await (this.daveVersion switch
                    {
                        1 => HandleDaveV1JsonPayloadsAsync(message),
                        _ => LogErrorAndReconnectAsync("Invalid DAVE version {daveVersion}.", this.daveVersion)
                    });

                    break;

                case (VoiceGatewayOpcode)10:
                case (VoiceGatewayOpcode)15:
                case (VoiceGatewayOpcode)16:
                case (VoiceGatewayOpcode)17:
                case (VoiceGatewayOpcode)18:
                case (VoiceGatewayOpcode)19:
                case (VoiceGatewayOpcode)20:

                    // undocumented opcodes that will be sent around
                    break;

                default:

                    // we don't really need to reconnect here, discord tests in prod all the time
                    this.logger.LogWarning("Received invalid opcode {opcode}.", message.Opcode);
                    break;
            }
        }
        else if (frame.Type == WebSocketMessageType.Binary)
        {
            await (this.daveVersion switch
            {
                1 => HandleDaveV1BinaryPayloadsAsync(frame),
                _ => LogErrorAndReconnectAsync("Invalid DAVE version {daveVersion}.", this.daveVersion)
            });
        }
        else
        {
            await HandleCloseCodeAsync(frame.Error);
        }
    }

    // this exists so we can use it in switch statements. i understand that it reports CA2254, it's fine.
#pragma warning disable CA2254
    private async Task LogErrorAndReconnectAsync(string message, params object[] values)
    {
        this.logger.LogError(message, values);
        await ReconnectInternalAsync(false);
    }
#pragma warning restore CA2254

    private async Task ResumeAndReconnectAsync()
    {
        // a disconnect was explicitly requested
        if (this.isDisconnecting)
        {
            return;
        }

        this.vgwCancellation.Cancel();
        await this.voiceGateway.DisconnectAsync(WebSocketCloseStatus.NormalClosure);
        await this.voiceGateway.ConnectAsync(this.endpoint, this.channelId);
        this.vgwCancellation = new();

        await this.voiceGateway.SendTextAsync(new()
        {
            Opcode = VoiceGatewayOpcode.Resume,
            Payload = new VoiceResumePayload
            {
                ServerId = this.guildId,
                SessionId = this.sessionId,
                Token = this.token,
                LastAcknowledgedSequenceNumber = this.lastSequence
            }
        });

        VoiceGatewayTransportFrame frame = await this.voiceGateway.ReceiveAsync();

        if (frame.Opcode == VoiceGatewayOpcode.Resumed)
        {
            // resume successful, restart the receive loop 
            _ = ReceiveVoiceGatewayEventsAsync(this.vgwCancellation.Token);
            return;
        }

        // if we got here: resume unsuccessful, fully reconnect and log any error we got

        if (frame.Type == WebSocketMessageType.Close)
        {
            this.logger.LogDebug("Failed to resume, reconnecting on close code {code}.", frame.Error);
        }
        else
        {
            this.logger.LogDebug("Received invalid opcode {opcode} from the voice gateway upon attempting to resume, reconnecting.", frame.Opcode);
        }

        await ReconnectInternalAsync(false);
    }

    private async Task ReconnectCoreAsync()
    {
        this.logger.LogDebug("Attempting to reconnect to the voice gateway.");

        this.vgwCancellation.Cancel();
        this.heartbeatCancellation.Cancel();

        await this.voiceGateway.DisconnectAsync(WebSocketCloseStatus.NormalClosure);

        this.vgwCancellation = new();
        this.heartbeatCancellation = new();

        await ConnectAsync();

        this.logger.LogDebug("Successfully reconnected to the voice gateway.");
    }
}