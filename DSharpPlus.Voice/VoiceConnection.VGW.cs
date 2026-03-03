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
using DSharpPlus.Voice.Protocol.Gateway.DaveV1;
using DSharpPlus.Voice.Protocol.Gateway.DaveV1.Inbound;
using DSharpPlus.Voice.Protocol.Gateway.DaveV1.Outbound;
using DSharpPlus.Voice.Transport;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private const int MaxDaveVersion = 1;
    private int daveVersion;
    private DateTimeOffset lastSentHeartbeat;
    private int pendingHeartbeats;

    // [NOTE] this code makes the assumption that discord will have the good sense to not change the semantics or contents of opcodes
    // that get exchanged before we know the DAVE version in future DAVE versions. if they don't show that modicum of common sense, 
    // that is a problem.
    private async Task ConnectAsync(ulong channelId, ulong guildId)
    {
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

        await this.voiceGateway.SendTextAsync<VoiceGatewayMessage<VoiceIdentifyData>>(new()
        {
            Opcode = (int)VoiceGatewayOpcode.Identify,
            Data = new()
            {
                GuildId = guildId,
                UserId = this.userId,
                SessionId = this.sessionId,
                Token = this.token,
                MaxSupportedDaveVersion = MaxDaveVersion
            }
        });

        // at this point, we will receive opcode 8 HELLO and opcode 2 READY in no particular order. in theory, READY contains everything
        // we need including the heartbeat interval (which is HELLO's sole raison d'être), however, the heartbeat interval in READY is a
        // lying liar who lies and is incorrect, so we need HELLO anyway.

        bool helloReceived = false, readyReceived = false;
        string selectedEncryptionMode = default;

        while (!helloReceived && !readyReceived)
        {
            GatewayTransportFrame frame = await this.voiceGateway.ReceiveAsync();

            if (frame.Opcode == (int)VoiceGatewayOpcode.Ready && !frame.IsBinary)
            {
                readyReceived = true;
                
                VoiceGatewayMessage<VoiceReadyData> ready = JsonSerializer.Deserialize<VoiceGatewayMessage<VoiceReadyData>>(frame.Payload);
                this.remoteUdpEndpoint = new(IPAddress.Parse(ready.Data.IP), ready.Data.Port);
                this.ssrc = ready.Data.Ssrc;
                selectedEncryptionMode = this.cryptorFactory.SelectPreferredEncryptionMode(ready.Data.SupportedEncryptionModes);

                this.lastSequence = ready.Sequence;
            }

            if (frame.Opcode == (int)VoiceGatewayOpcode.Hello && !frame.IsBinary)
            {
                helloReceived = true;
                
                VoiceGatewayMessage<VoiceHelloPayload> hello = JsonSerializer.Deserialize<VoiceGatewayMessage<VoiceHelloPayload>>(frame.Payload);
                this.heartbeatTask = HeartbeatAsync(hello.Data.HeartbeatInterval);
            }
        }

        // we have now received every bit of information necessary to connect to UDP...
        await this.mediaTransport.ConnectAsync(this.remoteUdpEndpoint);
        IPEndPoint localEndpoint = await PerformIPDiscoveryAsync(this.ssrc);

        // ... and to select the protocol we want
        await this.voiceGateway.SendTextAsync<VoiceGatewayMessage<VoiceSelectProtocolData>>(new()
        {
            Opcode = (int)VoiceGatewayOpcode.SelectProtocol,
            Data = new()
            {
                InnerData = new()
                {
                    Address = localEndpoint.Address.ToString(),
                    Port = localEndpoint.Port,
                    Mode = selectedEncryptionMode
                }
            }
        });

        while (true)
        {
            GatewayTransportFrame sessionDescriptionFrame = await this.voiceGateway.ReceiveAsync();

            if (sessionDescriptionFrame.Opcode != (int)VoiceGatewayOpcode.SessionDescription || sessionDescriptionFrame.IsBinary)
            {
                // something silly happened
                continue;
            }

            VoiceGatewayMessage<VoiceSessionDescription> sessionDescription 
                = JsonSerializer.Deserialize<VoiceGatewayMessage<VoiceSessionDescription>>(sessionDescriptionFrame.Payload);

            if (selectedEncryptionMode != sessionDescription.Data.Mode)
            {
                throw new ConnectingFailedException("Failed to negotiate transport encryption mode.");
            }

            this.cryptor = this.cryptorFactory.CreateCryptor(selectedEncryptionMode, sessionDescription.Data.SecretKey);
            this.daveVersion = sessionDescription.Data.DaveProtocolVersion;

            break;
        }

        this.vgwTask = ReceiveVoiceGatewayEventsAsync();
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
        await this.voiceGateway.SendTextAsync<VoiceGatewayMessage<VoiceSpeakingData>>(new()
        {
            Data = new()
            {
                Delay = 0,
                Speaking = flags,
                Ssrc = this.ssrc,
            },
            Opcode = (int)VoiceGatewayOpcode.Speaking
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

                await this.voiceGateway.SendTextAsync<VoiceGatewayMessage<VoiceHeartbeatData>>(new()
                {
                    Opcode = (int)VoiceGatewayOpcode.Heartbeat,
                    Data = new()
                    {
                        Timestamp = (ulong)time.ToUnixTimeMilliseconds(),
                        LastSequence = this.lastSequence
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
        
    }
}