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

namespace DSharpPlus.Voice.Transport;

public class VoiceStateFactory(ITransportServiceBuilder transportServiceBuilder, ICryptorFactory cryptorFactory)
{
    private readonly ITransportServiceBuilder transportServiceBuilder = transportServiceBuilder;
    private readonly ICryptorFactory cryptorFactory = cryptorFactory;

    public VoiceState Create(string userId, string serverId, string channelId, string token, string sessionId, string endpoint)
    {
        VoiceState state = new VoiceState()
        {
            UserId = userId,
            ServerId = serverId,
            Endpoint = endpoint,
            ConnectedChannel = channelId,
            VoiceToken = token,
            SessionId = sessionId,
            E2EEIsActive = false
        };

        IInitializedTransportServiceBuilder voiceDiscordServiceBuilder = this.transportServiceBuilder.CreateBuilder();

        // --------------------Dave------------------------------ //

        void setEncryptionStatusCb(bool active) => state.E2EEIsActive = active;
        MlsSession? e2ee = null;

        // External Sender (25)
        voiceDiscordServiceBuilder.AddBinaryHandler(25, (frame, client) =>
        {
            state.DaveStateHandler?.OnExternalSender(frame.Span);
            return Task.CompletedTask;
        });

        // Prepare Epoch (24)
        voiceDiscordServiceBuilder.AddJsonHandler<VoicePrepareEpochPayload>(24, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnPrepareEpochAsync(frame);
            }
        });

        // Prepare Transition (21)
        voiceDiscordServiceBuilder.AddJsonHandler<VoicePrepareTransitionPayload>(21, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnPrepareTransitionAsync(frame);
            }
        });

        // Execute Transition (22)
        voiceDiscordServiceBuilder.AddJsonHandler<VoicePrepareTransitionPayload>(22, (frame, client) =>
        {
            state.DaveStateHandler?.OnExecuteTransition(frame.Data.TransitionId);
            return Task.CompletedTask;
        });

        // MLS Proposals (27)
        voiceDiscordServiceBuilder.AddBinaryHandler(27, async (frame, client) =>
        {
            if (state.DaveStateHandler != null)
            {
                await state.DaveStateHandler.OnProposalsAsync(frame.ToArray(), [.. state.OtherUsersInVoice]);
            }
        });

        // MLS Announce Commit (29)
        voiceDiscordServiceBuilder.AddBinaryHandler(29, (frame, client) =>
        {
            state.DaveStateHandler?.OnAnnounceCommitTransition(frame.Slice(2).ToArray());
            return Task.CompletedTask;
        });

        // MLS Welcome (30)
        voiceDiscordServiceBuilder.AddBinaryHandler(30, (frame, client) =>
        {
            state.DaveStateHandler?.OnWelcome(frame.Span.Slice(2), state.OtherUsersInVoice.AsSpan());
            return Task.CompletedTask;
        });

        voiceDiscordServiceBuilder.AddJsonHandler<VoiceSessionDescriptionPayload>(30, (x, y) =>
        {
            state.VoiceCryptor = this.cryptorFactory.CreateCryptor([x.Data.Mode], [.. x.Data.SecretKey]);
            state.DaveStateHandler?.SetNegotiatedDaveVersion(1);
            return Task.CompletedTask;
        });

        voiceDiscordServiceBuilder.AddJsonHandler<VoiceUserIdsPayload>(11, (x, y) =>
        {
            state.OtherUsersInVoice = [.. x.Data.UserIds.Select(x => ulong.Parse(x))];
            return Task.CompletedTask;
        });

        voiceDiscordServiceBuilder.AddJsonHandler<VoiceUserPayload>(13, (x, y) =>
        {
            state.OtherUsersInVoice.Remove(ulong.Parse(x.Data.UserId));
            return Task.CompletedTask;
        });

        // ------------------------------------------------------ //


        voiceDiscordServiceBuilder.AddJsonHandler<VoiceSessionDescriptionPayload>(4, async (x, y) =>
        {
            state.VoiceCryptor = this.cryptorFactory.CreateCryptor([x.Data.Mode], [.. x.Data.SecretKey]);
            state.DaveStateHandler?.SetNegotiatedDaveVersion(1);
            await (state.DaveStateHandler?.SendMlsKeyPackageAsync() ?? Task.CompletedTask);
        });

        voiceDiscordServiceBuilder.AddJsonHandler<VoiceReadyPayload>(2, async (x, client) =>
        {
            string selectedMode = this.cryptorFactory.SupportedEncryptionModes
                .First(m => x.Data.Modes.Contains(m));

            MediaTransportFactory factory = new MediaTransportFactory();
            IMediaTransportService udp = factory.Create(null, new(IPAddress.Parse(x.Data.Ip), x.Data.Port));
            state.Ssrc = x.Data.Ssrc;
            e2ee = new(protocolVersion: 1, channelId: ulong.Parse(state.ConnectedChannel), userId: ulong.Parse(state.UserId), ssrc: (uint)state.Ssrc); // User id needs to be confirmed not null here
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

        voiceDiscordServiceBuilder.AddJsonHandler<BaseDiscordGatewayMessage>(8, async (x, client) =>
        {
            short retryCount = 0;
            while (state.VoiceToken is null || state.SessionId is null)
            {
                await Task.Delay(1000);
                retryCount++;

                if (retryCount > 3)
                {
                    throw new InvalidOperationException("OpCode 8 received without VoiceToken or SessionId!");
                }
            }

            VoiceIdentifyPayload payload = new VoiceIdentifyPayload()
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

        state.VoiceNegotiationTransportService = voiceDiscordServiceBuilder.Build(new Uri($"wss://{state.Endpoint}?v=8"));
        state.VoiceNegotiationTransportService.ConnectAsync();

        return state;
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
