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

        this.logger = this.loggerFactory.CreateLogger($"DSharpPlus.Voice.VoiceConnection - Channel {channelId}");
        this.logger.LogDebug("Initiating connection to the voice gateway, DAVE version {daveVersion}", this.daveVersion);

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

        VoiceServerUpdateEvent update = new()
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

        while (!helloReceived && !readyReceived)
        {
            VoiceGatewayTransportFrame frame = await this.voiceGateway.ReceiveAsync();

            if (frame.Opcode == VoiceGatewayOpcode.Ready && !frame.IsBinary)
            {
                readyReceived = true;
                
                VoiceGatewayMessage readyMessage = JsonSerializer.Deserialize<VoiceGatewayMessage>(frame.Payload)!;
                VoiceReadyPayload ready = (VoiceReadyPayload)readyMessage.Payload;

                remoteUdpEndpoint = new(IPAddress.Parse(ready.IPAddress), ready.Port);
                this.ssrc = ready.SSRC;
                selectedEncryptionMode = this.cryptorFactory.SelectPreferredEncryptionMode(ready.EncryptionModes);

                this.lastSequence = readyMessage.Sequence;
            }

            if (frame.Opcode == VoiceGatewayOpcode.Hello && !frame.IsBinary)
            {
                helloReceived = true;
                
                VoiceGatewayMessage helloMessage = JsonSerializer.Deserialize<VoiceGatewayMessage>(frame.Payload)!;
                VoiceHelloPayload hello = (VoiceHelloPayload)helloMessage.Payload;

                this.heartbeatTask = HeartbeatAsync(hello.HeartbeatInterval);
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

            if (sessionDescriptionFrame.Opcode != VoiceGatewayOpcode.SessionDescription || sessionDescriptionFrame.IsBinary)
            {
                // something silly happened
                continue;
            }

            VoiceGatewayMessage sessionDescription = JsonSerializer.Deserialize<VoiceGatewayMessage>(sessionDescriptionFrame.Payload);
            VoiceSessionDescriptionPayload sd  = (VoiceSessionDescriptionPayload)sessionDescription.Payload;

            if (selectedEncryptionMode != sd.EncryptionMode)
            {
                throw new ConnectingFailedException("Failed to negotiate transport encryption mode.");
            }

            this.cryptor = this.cryptorFactory.CreateCryptor(selectedEncryptionMode, sd.SecretKey);
            this.daveVersion = sd.DaveProtocolVersion;

            break;
        }

        this.e2ee.Initialize((ushort)this.daveVersion, channelId, this.userId, this.ssrc);
        this.vgwTask = ReceiveVoiceGatewayEventsAsync();

        this.audioClient = new
        (
            this.mediaTransport,
            this.receiver,
            this.cryptor,
            this.e2ee,
            this.sendingAudioChannel.Reader,
            this.encoder,
            this.ssrc
        );
    }

    /// <summary>
    /// Indicates to Discord that we are ready to send audio.
    /// </summary>
    public async Task StartSpeakingAsync()
    {
        this.logger.LogDebug("Announcing that we will start sending audio.");

        await SendSpeakingStatusAsync(VoiceSpeakingFlags.Microphone);

        this.isSpeaking = true;
    }

    /// <summary>
    /// Indicates to Discord that we will not send further audio.
    /// </summary>
    public async Task StopSpeakingAsync()
    {
        this.logger.LogDebug("Announcing that we will stop sending audio.");

        await SendSpeakingStatusAsync(VoiceSpeakingFlags.None);

        this.isSpeaking = false;
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

    private async Task HeartbeatAsync(int heartbeatInterval)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(heartbeatInterval));

        while (await timer.WaitForNextTickAsync())
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

                this.lastSentHeartbeat = time;
                this.pendingHeartbeats++;

                if (this.pendingHeartbeats > 5)
                {
                    // [TODO] find a mechanism to expose this to the user, we zombied
                }
            }
            catch (WebSocketException e)
            {
                this.logger.LogWarning("The connection died or entered an invalid state, reconnecting. Exception: {message}", e.Message);
                // [TODO] reconnect

                return;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "An error occurred during heartbeating.");
            }
        }
    }

    private async Task ReceiveVoiceGatewayEventsAsync()
    {
        while (true)
        {
            VoiceGatewayTransportFrame frame = await this.voiceGateway.ReceiveAsync();

            if (!frame.IsBinary)
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

                    case VoiceGatewayOpcode.PrepareTransition:
                    case VoiceGatewayOpcode.ExecuteTransition:
                    case VoiceGatewayOpcode.PrepareEpoch:

                        await (this.daveVersion switch
                        {
                            1 => HandleDaveV1JsonPayloadsAsync(message),
                            _ => throw new InvalidOperationException($"Invalid DAVE version {this.daveVersion}.")
                        });

                        break;

                    default:

                        throw new InvalidOperationException($"Received invalid opcode {message.Opcode}.");
                }
            }
            else
            {
                await (this.daveVersion switch
                {
                    1 => HandleDaveV1BinaryPayloadsAsync(frame),
                    _ => throw new InvalidOperationException($"Invalid DAVE version {this.daveVersion}.")
                });
            }
        }
    }
}