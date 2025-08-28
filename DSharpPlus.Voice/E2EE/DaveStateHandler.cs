using System;
using System.Threading.Tasks;

using DSharpPlus.Voice.Transport;

namespace DSharpPlus.Voice.E2EE;

/// <summary>
/// Keeps track of the DAVE state for a single media connection
/// </summary>
public class DaveStateHandler
{
    public ushort ProtocolVersion { get; private set; }
    public uint CurrentEpoch { get; private set; }

    private uint? pendingTransitionId;
    private uint? pendingEpochId;
    private bool pendingDowngrade;
    private readonly MlsSession mls;
    private readonly ITransportService voiceNegotiationTransportService;
    private readonly Action<bool> setE2eeActive;

    internal DaveStateHandler(MlsSession mls, ITransportService voiceNegotiationTransportService, Action<bool> setE2eeActive)
    {
        this.mls = mls;
        this.voiceNegotiationTransportService = voiceNegotiationTransportService;
        this.setE2eeActive = setE2eeActive;
    }

    /// <summary>
    /// Sets the version of DAVE we are using
    /// </summary>
    /// <param name="v">DAVE version</param>
    public void SetNegotiatedDaveVersion(ushort v) => this.ProtocolVersion = v;

    /// <summary>
    /// add an external send to the MLS group's extensions
    /// </summary>
    /// <param name="payload"></param>
    public void OnExternalSender(ReadOnlySpan<byte> payload) => this.mls.SetExternalSender(payload);

    /// <summary>
    /// Lets us know that we are moving to a new epoch. The payload includes the new epochs upcoming protocol.
    ///  If the epoch is 1 then we know it is actually a new MLS group we are creating with the given protocol
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Sends our KeyPackage over to the voice negotiation client
    /// </summary>
    /// <returns></returns>
    public async Task SendMlsKeyPackageAsync()
    {
        CommunityToolkit.HighPerformance.Buffers.ArrayPoolBufferWriter<byte> writer = new();
        this.mls.WriteKeyPackage(writer);
        await SendDaveBinaryAsync(this.voiceNegotiationTransportService, 26, writer.WrittenSpan);
    }

    /// <summary>
    /// We have been requested to downgrade to a lower DAVE version
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Execute a pending transition. If the transitionId is 0 we are (re)initializing
    /// </summary>
    /// <param name="transitionId"></param>
    public void OnExecuteTransition(uint transitionId)
    {
        if (this.pendingTransitionId != transitionId && transitionId != 0)
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

    /// <summary>
    /// Processes proposals that we want to append and/or revoke for our MLS group
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="roster"></param>
    /// <returns></returns>
    public async Task OnProposalsAsync(byte[] payload, ulong[] roster)
    {
        byte[] commitBytes = this.mls.ProcessProposals(payload, roster);
        if (commitBytes is { Length: > 0 })
        {
            await SendDaveBinaryAsync(this.voiceNegotiationTransportService, 28, commitBytes);  // MLS Commit/Welcome
        }
    }

    /// <summary>
    /// Proccesses a commit made to the MLS group
    /// </summary>
    /// <param name="payload"></param>
    public void OnAnnounceCommitTransition(ReadOnlySpan<byte> payload) => this.mls.ProcessCommit(payload.ToArray());

    /// <summary>
    /// Processes the MLS welcome message
    /// </summary>
    /// <param name="payload">Welcome Message</param>
    /// <param name="roster">Roster of users in the media channel</param>
    public void OnWelcome(ReadOnlySpan<byte> payload, ReadOnlySpan<ulong> roster) => this.mls.ProcessWelcome(payload.ToArray(), roster.ToArray());


    /// <summary>
    /// Sends a DAVE binary payload to the transport service
    /// </summary>
    /// <param name="client">Transport service</param>
    /// <param name="opcode">OpCode to send</param>
    /// <param name="payload">Data frame to send</param>
    /// <returns></returns>
    private static Task SendDaveBinaryAsync(ITransportService client, byte opcode, ReadOnlySpan<byte> payload)
    {
        byte[] buf = new byte[1 + payload.Length];
        buf[0] = opcode;
        payload.CopyTo(buf.AsSpan(1));
        return client.SendAsync((ReadOnlyMemory<byte>)buf, null);
    }
}
