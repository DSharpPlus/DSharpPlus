using System;
using System.Threading.Tasks;

using DSharpPlus.Voice.Transport;

namespace DSharpPlus.Voice.E2EE;

public class DaveStateHandler(MlsSession mls, ITransportService voiceNegotiationTransportService, Action<bool> setE2eeActive)
{
    public ushort ProtocolVersion { get; private set; }
    public uint CurrentEpoch { get; private set; }

    private uint? pendingTransitionId;
    private uint? pendingEpochId;
    private bool pendingDowngrade;

    private readonly MlsSession mls = mls;
    private readonly ITransportService voiceNegotiationTransportService = voiceNegotiationTransportService;
    private readonly Action<bool> setE2eeActive = setE2eeActive;

    public void SetNegotiatedDaveVersion(ushort v) => this.ProtocolVersion = v;

    public void OnExternalSender(ReadOnlySpan<byte> payload) => this.mls.SetExternalSender(payload.ToArray());

    public async Task OnPrepareEpochAsync(VoicePrepareEpochPayload payload)
    {
        if (payload.Data.ProtocolVersion != this.ProtocolVersion || this.CurrentEpoch == 0)
        {
            this.mls.ReinitializeE2EESession(payload.Data.ProtocolVersion);
            this.ProtocolVersion = payload.Data.ProtocolVersion;
        }

        if (payload.Data.Epoch == 1)
        {
            await SendMlsKeyPackageAsync();
        }
    }

    public async Task SendMlsKeyPackageAsync()
    {
        CommunityToolkit.HighPerformance.Buffers.ArrayPoolBufferWriter<byte> writer = new();
        this.mls.WriteKeyPackage(writer);
        await SendDaveBinaryAsync(this.voiceNegotiationTransportService, 26, writer.WrittenSpan);
    }

    // op 21: downgrade requested
    public async Task OnPrepareTransitionAsync(VoicePrepareTransitionPayload payload)
    {
        uint transitionId = payload.Data.TransitionId;
        this.pendingTransitionId = transitionId;
        this.pendingEpochId = null;
        this.pendingDowngrade = true;

        await this.voiceNegotiationTransportService.SendAsync<VoicePrepareTransitionPayload>(
            new()
            {
                OpCode = 23,
                Data = new() { TransitionId = payload.Data.TransitionId }
            }); // Transition Ready
    }

    // op 22: execute transition
    public void OnExecuteTransition(uint transitionId)
    {
        if (this.pendingTransitionId != transitionId)
        {
            Console.WriteLine("The pending transaction and the one asked to execute didnt match.");
        }

        if (this.pendingDowngrade)
        {
            this.setE2eeActive(false);
            this.ProtocolVersion = 0;
            this.CurrentEpoch = 0;
        }
        else
        {
            this.setE2eeActive(true);
            if (this.pendingEpochId.HasValue)
            {
                this.CurrentEpoch = this.pendingEpochId.Value;
            }
        }

        this.pendingTransitionId = null;
        this.pendingEpochId = null;
        this.pendingDowngrade = false;
    }

    public async Task OnProposalsAsync(byte[] payload, ulong[] roster)
    {
        byte[] commitBytes = this.mls.ProcessProposals(payload, roster);
        if (commitBytes is { Length: > 0 })
        {
            await SendDaveBinaryAsync(this.voiceNegotiationTransportService, 28, commitBytes);  // MLS Commit/Welcome
        }
    }

    public void OnAnnounceCommitTransition(ReadOnlySpan<byte> payload) => this.mls.ProcessCommit(payload.ToArray());
    public void OnWelcome(ReadOnlySpan<byte> payload, ReadOnlySpan<ulong> roster) => this.mls.ProcessWelcome(payload.ToArray(), roster.ToArray());

    private static Task SendDaveBinaryAsync(ITransportService client, byte opcode, ReadOnlySpan<byte> payload)
    {
        byte[] buf = new byte[1 + payload.Length];
        buf[0] = opcode;
        payload.CopyTo(buf.AsSpan(1));
        return client.SendAsync((ReadOnlyMemory<byte>)buf, null);
    }
}
