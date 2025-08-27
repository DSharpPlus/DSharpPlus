using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;

namespace DSharpPlus.Voice.Transport.Factories;

public class VoiceStateFactory : IVoiceStateFactory
{
    private readonly ITransportServiceBuilder transportServiceBuilder;
    private readonly ICryptorFactory cryptorFactory;

    public VoiceStateFactory(ITransportServiceBuilder transportServiceBuilder, ICryptorFactory cryptorFactory)
    {
        this.transportServiceBuilder = transportServiceBuilder;
        this.cryptorFactory = cryptorFactory;
    }

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

    private void SetupInitialCallbacks(IInitializedTransportServiceBuilder transportServiceBuilder, VoiceState state)
    {
        void setEncryptionStatusCb(bool active) => state.E2EEIsActive = active;
        MlsSession? e2ee = null;

        // External Sender (25 - DAVE) - Includes all data required to add an external send to the MLS group's extensions
        transportServiceBuilder.AddBinaryHandler(25, (frame, client) =>
        {
            state.DaveStateHandler?.OnExternalSender(frame.Span);
            return Task.CompletedTask;
        });

        // Prepare Epoch (24 - DAVE) - Lets us know that we are moving to a new epoch. Includes the new epochs upcoming protocol.
        // If the epoch is 1 then we know it is actually a new MLS group we are creating with the given protocol
        transportServiceBuilder.AddJsonHandler<VoicePrepareEpochPayload>(24, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnPrepareEpochAsync(frame);
            }
        });

        // Prepare Transition (21 - DAVE) - This informs us that we are about to transition to a new protocol?
        // If the transition id is 0 then its a (re)initialization and it can be executed immediately. 
        transportServiceBuilder.AddJsonHandler<VoicePrepareTransitionPayload>(21, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnPrepareTransitionAsync(frame);
            }
        });


        // Execute Transition (22 - DAVE) - Informs us to execute the transition we were warned about in OPCODE 21
        transportServiceBuilder.AddJsonHandler<VoicePrepareTransitionPayload>(22, (frame, client) =>
        {
            state.DaveStateHandler?.OnExecuteTransition(frame.Data.TransitionId);
            return Task.CompletedTask;
        });

        // MLS Proposals (27 - DAVE) - Lists proposals that we want to append and/or revoke for our MLS group
        transportServiceBuilder.AddBinaryHandler(27, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnProposalsAsync(frame.ToArray(), [.. state.OtherUsersInVoice]);
            }
        });

        // MLS Announce Commit (29 - DAVE) - Ask jesus what this one does because I don't know
        transportServiceBuilder.AddBinaryHandler(29, (frame, client) =>
        {
            state.DaveStateHandler?.OnAnnounceCommitTransition(frame.Slice(2).ToArray());
            return Task.CompletedTask;
        });

        // MLS Welcome (30 - DAVE) - A targeted message that includes the MLS welcome that adds the pending
        // members to the group, and also has the transition id for the group transition.
        transportServiceBuilder.AddBinaryHandler(30, (frame, client) =>
        {
            state.DaveStateHandler?.OnWelcome(frame.Span.Slice(2), state.OtherUsersInVoice.AsSpan());
            return Task.CompletedTask;
        });

        // why do I have 2 here, dunno
        transportServiceBuilder.AddJsonHandler<VoiceSessionDescriptionPayload>(30, (x, y) =>
        {
            state.VoiceCryptor = this.cryptorFactory.CreateCryptor([x.Data.Mode], [.. x.Data.SecretKey]);
            state.DaveStateHandler?.SetNegotiatedDaveVersion(1);
            return Task.CompletedTask;
        });

        // Clients Connect (11 - DAVE) - Gives the snowflake user id's of users that have connected to the media session. Used to ensure
        // the proposed addition of the mls group members match the expected media session users
        transportServiceBuilder.AddJsonHandler<VoiceUserIdsPayload>(11, (x, y) =>
        {
            state.OtherUsersInVoice = [.. x.Data.UserIds.Select(x => ulong.Parse(x))];
            return Task.CompletedTask;
        });

        // Client Disconnect (13 - DAVE) - Lets us know a user disconnected and gives us their user snowflake id
        transportServiceBuilder.AddJsonHandler<VoiceUserPayload>(13, (x, y) =>
        {
            state.OtherUsersInVoice.Remove(ulong.Parse(x.Data.UserId));
            return Task.CompletedTask;
        });

        // Select Protocol Ack (4 - DAVE) - Lets us know what version of dave we are using and what cryptors to use
        transportServiceBuilder.AddJsonHandler<VoiceSessionDescriptionPayload>(4, async (x, y) =>
        {
            state.VoiceCryptor = this.cryptorFactory.CreateCryptor([x.Data.Mode], [.. x.Data.SecretKey]);
            state.DaveStateHandler?.SetNegotiatedDaveVersion(x.Data.DaveProtocolVersion);
            await (state.DaveStateHandler?.SendMlsKeyPackageAsync() ?? Task.CompletedTask);
        });

        // (2) Lets us know we are ready for voice. Here we send a message to discords udp discover endpoint in order to
        // get our own IP address.
        transportServiceBuilder.AddJsonHandler<VoiceReadyPayload>(2, async (x, client) =>
        {
            string selectedMode = this.cryptorFactory.SupportedEncryptionModes
                .First(m => x.Data.Modes.Contains(m));

            MediaTransportFactory factory = new MediaTransportFactory();
            IMediaTransportService udp = factory.Create(null, new(IPAddress.Parse(x.Data.Ip), x.Data.Port));
            state.Ssrc = x.Data.Ssrc;
            e2ee = new(protocolVersion: 1, channelId: ulong.Parse(state.ConnectedChannel), userId: ulong.Parse(state.UserId), ssrc: state.Ssrc); // User id needs to be confirmed not null here
            state.DaveStateHandler = new(e2ee, client, setEncryptionStatusCb);

            // Send probe
            byte[] probe = BuildIpDiscoveryProbe(x.Data.Ssrc);
            await udp.SendAsync(probe);

            // Receive response
            ArrayBufferWriter<byte> writer = new ArrayBufferWriter<byte>(70);
            await udp.ReceiveAsync(writer);
            ReadOnlySpan<byte> resp = writer.WrittenSpan; // should be 70 bytes

            (string ip, int port) = ParseIpDiscoveryResponse(resp);

            var selectProtocol = new
            {
                op = 1,
                d = new
                {
                    protocol = "udp",
                    data = new
                    {
                        address = ip,
                        port,
                        mode = selectedMode
                    }
                }
            };

            await client.SendAsync(selectProtocol);
        });

        // (8) Lets us know that we are ready to send data.
        // After this we identify with the discord gateway
        transportServiceBuilder.AddJsonHandler<BaseDiscordGatewayMessage>(8, async (x, client) =>
        {
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
                OpCode = 0
            };
            await client.SendAsync(payload);
        });
    }

    private static byte[] BuildIpDiscoveryProbe(uint ssrc)
    {
        byte[] buf = new byte[74];
        BinaryPrimitives.WriteUInt16BigEndian(buf.AsSpan(0, 2), 0x0001);
        BinaryPrimitives.WriteUInt16BigEndian(buf.AsSpan(2, 2), 70);
        BinaryPrimitives.WriteUInt32BigEndian(buf.AsSpan(4, 4), ssrc);

        return buf;
    }

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
