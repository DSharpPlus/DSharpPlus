using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.Models;
using DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads;
using DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

namespace DSharpPlus.Voice.Transport.Factories;

/// <inheritdoc/>
public class VoiceStateFactory : IVoiceStateFactory
{
    private readonly ITransportServiceBuilder transportServiceBuilder;
    private readonly ICryptorFactory cryptorFactory;

    public VoiceStateFactory(ITransportServiceBuilder transportServiceBuilder, ICryptorFactory cryptorFactory)
    {
        this.transportServiceBuilder = transportServiceBuilder;
        this.cryptorFactory = cryptorFactory;
    }

    /// <inheritdoc/>
    public VoiceState Create(string userId, string serverId, string channelId, string token, string sessionId, string endpoint)
    {
        // Create Initial voice state with known values
        VoiceState state = CreateInitialVoiceState(userId, serverId, channelId, token, sessionId, endpoint);

        // Create builder for the Voice State Negotiator Service
        IInitializedTransportServiceBuilder voiceDiscordServiceBuilder = this.transportServiceBuilder.CreateBuilder();

        // Setup our callbacks for Voice State Negotiation events
        SetupInitialCallbacks(voiceDiscordServiceBuilder, state);

        // Build our Voice State Negotiator Service
        state.VoiceNegotiationTransportService = voiceDiscordServiceBuilder.Build(new Uri($"wss://{state.Endpoint}?v=8"));
        state.VoiceNegotiationTransportService.ConnectAsync();

        // Now return our VoiceState that represents our connection state in any voice channel
        return state;
    }

    /// <summary>
    /// Creates a voice state with the required initial data.
    /// </summary>
    private static VoiceState CreateInitialVoiceState(string userId, string serverId, string channelId, string token, string sessionId, string endpoint) =>
        new()
        {
            UserId = userId,
            ServerId = serverId,
            Endpoint = endpoint,
            ConnectedChannel = channelId,
            VoiceToken = token,
            SessionId = sessionId,
            E2EEIsActive = false
        };

    /// <summary>
    /// Sets up the callbacks for the different Discord Voice Gateway events.
    /// </summary>
    /// <param name="transportServiceBuilder"></param>
    /// <param name="state"></param>
    private void SetupInitialCallbacks(IInitializedTransportServiceBuilder transportServiceBuilder, VoiceState state)
    {
        void setEncryptionStatusCb(bool active) => state.E2EEIsActive = active;
        MlsSession? e2ee = null;

        // External Sender (25 - DAVE) - Includes all data required to add an external send to the MLS group's extensions
        transportServiceBuilder.AddBinaryHandler((int)VoiceGatewayOpcode.MlsExternalSender, (frame, client) =>
        {
            state.DaveStateHandler?.OnExternalSender(frame.Span);
            return Task.CompletedTask;
        });

        // Prepare Epoch (24 - DAVE) - Lets us know that we are moving to a new epoch. Includes the new epochs upcoming protocol.
        // If the epoch is 1 then we know it is actually a new MLS group we are creating with the given protocol
        transportServiceBuilder.AddJsonHandler<VoicePrepareEpochPayload>((int)VoiceGatewayOpcode.PrepareEpoch, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnPrepareEpochAsync(frame);
            }
        });

        // Prepare Transition (21 - DAVE) - This informs us that we are about to transition to a new protocol?
        // If the transition id is 0 then its a (re)initialization and it can be executed immediately. 
        transportServiceBuilder.AddJsonHandler<VoicePrepareTransitionPayload>((int)VoiceGatewayOpcode.PrepareTransition, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnPrepareTransitionAsync(frame);
            }
        });

        // Execute Transition (22 - DAVE) - Informs us to execute the transition we were warned about in OPCODE 21
        transportServiceBuilder.AddJsonHandler<VoicePrepareTransitionPayload>((int)VoiceGatewayOpcode.ExecuteTransition, (frame, client) =>
        {
            state.DaveStateHandler?.OnExecuteTransition(frame.Data.TransitionId);
            return Task.CompletedTask;
        });

        // MLS Proposals (27 - DAVE) - Lists proposals that we want to append and/or revoke for our MLS group
        transportServiceBuilder.AddBinaryHandler((int)VoiceGatewayOpcode.MlsProposals, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnProposalsAsync(frame.ToArray(), [.. state.OtherUsersInVoice.Select(x => x.Value.UserId)]);
            }
        });

        // MLS Announce Commit (29 - DAVE) - Proccesses a commit made to the MLS group
        transportServiceBuilder.AddBinaryHandler((int)VoiceGatewayOpcode.MlsAnnounceCommitTransition, (frame, client) =>
        {
            state.DaveStateHandler?.OnAnnounceCommitTransition(frame[2..].ToArray());
            return Task.CompletedTask;
        });

        // MLS Welcome (30 - DAVE) - A targeted message that includes the MLS welcome that adds the pending
        // members to the group, and also has the transition id for the group transition.
        transportServiceBuilder.AddBinaryHandler((int)VoiceGatewayOpcode.MlsWelcome, (frame, client) =>
        {
            state.DaveStateHandler?.OnWelcome(frame.Span[2..], [.. state.OtherUsersInVoice.Select(x => x.Value.UserId)]);
            return Task.CompletedTask;
        });

        // Not sure if this is needed
        transportServiceBuilder.AddJsonHandler<VoiceSessionDescriptionPayload>((int)VoiceGatewayOpcode.MlsWelcome, (x, y) =>
        {
            state.VoiceCryptor = this.cryptorFactory.CreateCryptor([x.Data.Mode], [.. x.Data.SecretKey]);
            state.DaveStateHandler?.SetNegotiatedDaveVersion(x.Data.DaveProtocolVersion);
            return Task.CompletedTask;
        });

        // Clients Connect (11 - DAVE) - Gives the snowflake user id's of users that have connected to the media session. Used to ensure
        // the proposed addition of the mls group members match the expected media session users
        transportServiceBuilder.AddJsonHandler<VoiceUserIdsPayload>((int)VoiceGatewayOpcode.ClientsConnected, (x, y) =>
        {
            IEnumerable<KeyValuePair<ulong, UserInVoice>> kvps = x.Data.UserIds.Select(x =>
            {
                ulong userId = ulong.Parse(x);
                return new KeyValuePair<ulong, UserInVoice>(userId, new() 
                { 
                    IsSpeaking = false, 
                    UserId = userId 
                });
            });

            state.OtherUsersInVoice = new System.Collections.Concurrent.ConcurrentDictionary<ulong, UserInVoice>(kvps);
            return Task.CompletedTask;
        });

        // Client Disconnect (13 - DAVE) - Lets us know a user disconnected and gives us their user snowflake id
        transportServiceBuilder.AddJsonHandler<VoiceUserPayload>((int)VoiceGatewayOpcode.ClientDisconnected, (x, y) =>
        {
            state.OtherUsersInVoice.Remove(ulong.Parse(x.Data.UserId), out _);
            return Task.CompletedTask;
        });
        
        // Select Protocol (4 - DAVE) - Lets us know what version of dave we are using and what cryptors to use
        transportServiceBuilder.AddJsonHandler<VoiceSessionDescriptionPayload>((int)VoiceGatewayOpcode.SessionDescription, async (x, y) =>
        {
            state.VoiceCryptor = this.cryptorFactory.CreateCryptor([x.Data.Mode], [.. x.Data.SecretKey]);
            state.DaveStateHandler?.SetNegotiatedDaveVersion(x.Data.DaveProtocolVersion);
            await (state.DaveStateHandler?.SendMlsKeyPackageAsync() ?? Task.CompletedTask);
        });

        // (2) Lets us know we are ready for voice. Here we send a message to discords udp discover endpoint in order to
        // get our own IP address.
        transportServiceBuilder.AddJsonHandler<VoiceReadyPayload>((int)VoiceGatewayOpcode.Ready, async (x, client) =>
        {
            string selectedMode = this.cryptorFactory.SupportedEncryptionModes
                .First(m => x.Data.Modes.Contains(m));

            MediaTransportFactory factory = new MediaTransportFactory();

            // not sure if we need to persist this since its for IP lookup, voice data I think will go through a different udp connection
            state.MediaTransportService = factory.Create(null, new(IPAddress.Parse(x.Data.Ip), x.Data.Port));
            state.Ssrc = x.Data.Ssrc;
            e2ee = new(protocolVersion: 1, channelId: ulong.Parse(state.ConnectedChannel), userId: ulong.Parse(state.UserId), ssrc: state.Ssrc); 
            state.DaveStateHandler = new(e2ee, client, setEncryptionStatusCb);

            // Send probe
            byte[] probe = BuildIpDiscoveryProbe(x.Data.Ssrc);
            await state.MediaTransportService.SendAsync(probe);

            // Receive response
            ArrayBufferWriter<byte> writer = new(70);
            await state.MediaTransportService.ReceiveAsync(writer);
            ReadOnlySpan<byte> resp = writer.WrittenSpan; // should be 70 bytes

            (string ip, int port) = ParseIpDiscoveryResponse(resp);

            VoiceSelectProtocolPayload payload = new()
            {
                Data = new()
                {
                    Protocol = "udp",
                    InnerData = new()
                    {
                        Address = ip,
                        Port = port,
                        Mode = selectedMode
                    }
                },
                OpCode = (int)VoiceGatewayOpcode.SelectProtocol
            };

            await client.SendAsync(payload);
        });

        // (8) Lets us know that we are ready to send data.
        // After this we identify with the discord gateway
        transportServiceBuilder.AddJsonHandler<VoiceHelloPayload>((int)VoiceGatewayOpcode.Hello, async (x, client) =>
        {
            state.HeartbeatInterval = x.HeartbeatInterval;

            VoiceIdentifyPayload payload = new()
            {
                Data = new()
                {
                    ServerId = state.ServerId,
                    UserId = state.UserId,
                    Token = state.VoiceToken,
                    SessionId = state.SessionId,
                    MaxSupportedDaveVersion = 1
                },
                OpCode = (int)VoiceGatewayOpcode.Identify
            };
            await client.SendAsync(payload);
        });

        // (5) Lets us know a user in out voice chat is speaking
        transportServiceBuilder.AddJsonHandler<VoiceOtherUserSpeakingPayload>((int)VoiceGatewayOpcode.Speaking, (x, client) =>
        {
            state.SsrcMap[x.Data.UserId] = x.Data.Ssrc;
            if (!state.OtherUsersInVoice.TryGetValue(x.Data.UserId, out UserInVoice userInVoice))
            {
                state.OtherUsersInVoice[x.Data.UserId] = userInVoice = new UserInVoice()
                {
                    IsSpeaking = true,
                    Ssrc = x.Data.Ssrc,
                    UserId = x.Data.UserId,
                };

                return Task.CompletedTask;
            }

            userInVoice.Ssrc = x.Data.Ssrc;
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Builds the IP Discovery Probe to send to discord.
    /// </summary>
    /// <param name="ssrc"></param>
    /// <returns></returns>
    private static byte[] BuildIpDiscoveryProbe(uint ssrc)
    {
        byte[] buf = new byte[74];
        BinaryPrimitives.WriteUInt16BigEndian(buf.AsSpan(0, 2), 0x0001);
        BinaryPrimitives.WriteUInt16BigEndian(buf.AsSpan(2, 2), 70);
        BinaryPrimitives.WriteUInt32BigEndian(buf.AsSpan(4, 4), ssrc);

        return buf;
    }

    /// <summary>
    /// Parses the response received from the IP Discovery Endpoint.
    /// </summary>
    /// <param name="resp"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static (string ip, int port) ParseIpDiscoveryResponse(ReadOnlySpan<byte> resp)
    {
        if (resp.Length < 74)
        {
            throw new InvalidOperationException($"Response too short: {resp.Length}");
        }

        ushort type = BinaryPrimitives.ReadUInt16BigEndian(resp.Slice(0, 2));
        ushort len = BinaryPrimitives.ReadUInt16BigEndian(resp.Slice(2, 2));
        if (type != 0x0002)
        {
            throw new InvalidOperationException($"Unexpected type: 0x{type:X4}");
        }

        if (len != 70)
        {
            throw new InvalidOperationException($"Unexpected length: {len}");
        }

        ReadOnlySpan<byte> ipBytes = resp.Slice(8, 64);
        int zero = ipBytes.IndexOf((byte)0);
        if (zero >= 0)
        {
            ipBytes = ipBytes[..zero];
        }

        string ip = Encoding.ASCII.GetString(ipBytes);

        ushort port = BinaryPrimitives.ReadUInt16BigEndian(resp.Slice(72, 2));
        return (ip, port);
    }
}
